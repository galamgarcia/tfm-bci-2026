using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.UI;

public class ReceiveScanDevice : UnityEvent<string> { }


[System.Serializable]
public class ThinkGearManager : MonoBehaviour
{
    public static ThinkGearManager instance;

    [Header("Demo")]
    [Tooltip("Simula un BrainLink conectado para poder probar la aplicación desde el Editor sin un dispositivo físico.")]
    public bool isDemo = false;
    [Tooltip("Muestra el icono de batería cuando el modo demo está activo.")]
    public bool isShowBattery = false;

    [Header("Signal UI")]
    [Tooltip("Icono mostrado cuando no existe conexión Bluetooth.")]
    public Sprite signal0;
    [Tooltip("Icono mostrado cuando el dispositivo está conectado pero no hay contacto con la piel (PoorSignal = 200).")]
    public Sprite signal1;
    [Tooltip("Icono mostrado cuando la calidad de la señal es baja.")]
    public Sprite signal2;
    [Tooltip("Icono mostrado cuando la calidad de la señal es aceptable.")]
    public Sprite signal3;
    [Tooltip("Icono mostrado cuando la calidad de la señal es óptima.")]
    public Sprite signal4;
    [Tooltip("Imagen utilizada para representar el estado de la señal EEG.")]
    public Image image_signal;
    [Tooltip("Contenedor del indicador visual de batería.")]
    public GameObject signal_battery;
    [Tooltip("Barra de relleno de la batería del dispositivo.")]
    public Image filled_battary;

    [Header("Demo Values")]
    [Tooltip("Nivel de batería simulado durante el modo demo.")]
    [Range(0, 100)]
    public int batteryCapacity4_0 = 0;
    [Tooltip("Valor RAW simulado generado durante el modo demo.")]
    [Range(-2000, 2000)]
    public int demo_raw;

    [Header("Device Settings")]
    [Tooltip("Interruptor para activar la función AP del dispositivo.")]
    public Toggle toggle_ap;
    [Tooltip("Interruptor para activar el modo Circle del dispositivo.")]
    public Toggle toggle_circle;
    [Tooltip("Estado enviado al dispositivo para la característica Circle.")]
    public int isCircleOn = 0;
    [Tooltip("Estado enviado al dispositivo para la característica AP.")]
    public int isApOn = 0;

    [Header("Device Discovery")]
    [Tooltip("Tabla donde se muestran los dispositivos encontrados durante el escaneo.")]
    public UITableView tableView;
    [Tooltip("Evento lanzado cada vez que se detecta un BrainLink durante el escaneo Bluetooth.")]
    public ReceiveScanDevice receiveScanDevice;

    // === Connection Status ===
    [HideInInspector]
    [Tooltip("Indica si existe una conexión Bluetooth activa en Android. Se actualiza automáticamente mediante callbacks del SDK.")]
    public bool bAndroidHeadsetConnected;
    [HideInInspector]
    [Tooltip("Indica si existe una conexión Bluetooth activa en iOS. Se actualiza automáticamente mediante callbacks del SDK.")]
    public bool bIOSHeadsetConnected;

    // === EEG Data ===
    // Versión hardware reportada por el SDK.
    private int hardwareversion;
    //Última muestra RAW recibida del EEG.
    private int Raw;
    // Calidad de la señal: 0 (perfecto), 1-199 (parcial), 200 (sin conexión).
    public int PoorSignal = 200;
    // Nivel de atención (0-100).
    private int Attention;
    // Nivel de meditación (0-100).
    private int Meditation;
    // Intensidad del último parpadeo detectado.
    private int Blink;
    // Banda Delta.
    private float Delta;
    // Banda Theta.
    private float Theta;
    // Banda Low Alpha.
    private float LowAlpha;
    // Banda High Alpha.
    private float HighAlpha;
    // Banda Low Beta.
    private float LowBeta;
    // Banda High Beta.
    private float HighBeta;
    // Banda Low Gamma.
    private float LowGamma;
    // Banda High Gamma.
    private float HighGamma;

    // === Additional Sensors ===
    // Versión hardware en formato texto.
    private string Hardwareversion4_0 = string.Empty;
    // Frecuencia cardíaca.
    private int heartRate;
    // Temperatura corporal.
    private float temperature;
    // Heart Rate Variability.
    private string HRV = string.Empty;
    // Acelerómetro eje X.
    private float xvalue4_0;
    // Acelerómetro eje Y.
    private float yvalue4_0;
    // Acelerómetro eje Z.
    private float zvalue4_0;
    // Porcentaje de batería recibido del dispositivo.
    private int BatteryCapacity4_0;
    // Valor AP recibido desde el dispositivo.
    private int ap;
    // Intensidad de rechinar los dientes.
    private int grind;

    // === Internal State ===
    // Estado interno de conexión.
    private bool isConnect;
    // Nombre del dispositivo conectado.
    private string deviceName = string.Empty;
    // Indica si ya se enviaron los parámetros iniciales al dispositivo y evita reenviarlos cada vez que llega un paquete de señal.
    private bool isSendFirst;

    // Returns whether the headset should be treated as connected on the current runtime platform. In the Unity Editor, demo mode acts as the connection source.
    private bool IsPlatformHeadsetConnected
    {
        get
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return isDemo || bAndroidHeadsetConnected;
#elif UNITY_IOS && !UNITY_EDITOR
            return isDemo || bIOSHeadsetConnected;
#else
            return isDemo;
#endif
        }
    }

    public void Awake()
    {
        Debug.unityLogger.logEnabled = true;
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        if (receiveScanDevice == null)
        {
            receiveScanDevice = new ReceiveScanDevice();
        }
        signal_battery.SetActive(false);
    }
    
    public void Scan()
    {
        Debug.Log("unity===scan");
        string whiteList = "BrainLink_Pro,BrainLink_Lite,BrainLink,BrainLink_Lite_P,Brainlink_Lite,ROYWOS,BrainLink_Pink";
#if UNITY_IOS && !UNITY_EDITOR
        UnityThinkGear.SetWhiteList(whiteList);
        UnityThinkGear.Scan();
#elif UNITY_ANDROID && !UNITY_EDITOR
        UnityThinkGear.setWhiteList(whiteList);
        UnityThinkGear.SetBLLinstenner("ThinkGearManager");
        UnityThinkGear.start();
#else
        Debug.Log("BrainLink scan is only available on an Android or iOS build.");
#endif
    }

//     public void DisConnect()
//     {
// #if UNITY_IOS && !UNITY_EDITOR
//         UnityThinkGear.DisConnect();
// #elif UNITY_ANDROID && !UNITY_EDITOR
//         UnityThinkGear.close();
// #else
//         bAndroidHeadsetConnected = false;
//         bIOSHeadsetConnected = false;
// #endif
//     }

    
    public void connectDevice(string identifierOrAddress)
    {
        Debug.Log("unity===connectDevice:====" +identifierOrAddress);
#if UNITY_IOS && !UNITY_EDITOR
        UnityThinkGear.ConnectDevice(identifierOrAddress);
#elif UNITY_ANDROID && !UNITY_EDITOR
        UnityThinkGear.connectDevice(identifierOrAddress);
#else
        Debug.Log("BrainLink connection is only available on an Android or iOS build.");
#endif
    }

    public void DeviceFound(string nameIdentifierOrAddressRiss)
    {
        Debug.Log("unity===nameAddressRiss:====" + nameIdentifierOrAddressRiss);
        receiveScanDevice.Invoke(nameIdentifierOrAddressRiss);
    }

    void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        toggle_ap.isOn = false;
        toggle_circle.isOn = false;
        StartCoroutine("ShowBatteryCapacity");
        StartCoroutine("UpdateSignal");
    }

    void OnGUI()
    {
        if(!isDemo) { return; }
        
        PoorSignal = 0;
#if UNITY_IPHONE
        bIOSHeadsetConnected = true;
#elif UNITY_ANDROID
		bAndroidHeadsetConnected = true;
#endif
        if (GUILayout.Button("0"))
        {
            Attention = 0;
            Meditation = 0;
        }
        if (GUILayout.Button("10"))
        {
            Attention = 10;
            Meditation = 10;
        }
        if (GUILayout.Button("20"))
        {
            Attention = 20;
            Meditation = 20;
        }
        if (GUILayout.Button("30"))
        {
            Attention = 30;
            Meditation = 30;
        }
        if (GUILayout.Button("40"))
        {
            Attention = 40;
            Meditation = 40;
        }
        if (GUILayout.Button("50"))
        {
            Attention = 50;
            Meditation = 50;
        }
        if (GUILayout.Button("60"))
        {
            Attention = 60;
            Meditation = 60;
        }
        if (GUILayout.Button("70"))
        {
            Attention = 70;
            Meditation = 70;
        }
        if (GUILayout.Button("80"))
        {
            Attention = 80;
            Meditation = 80;
        }
        if (GUILayout.Button("90"))
        {
            Attention = 90;
            Meditation = 90;
        }
        if (GUILayout.Button("100"))
        {
            Attention = 100;
            Meditation = 100;
        }
        Raw = demo_raw;
    }

    public void OnApplicationQuit()
    {
        GC.Collect();
        instance = null;
    }

    IEnumerator UpdateSignal()
    {
        while (true)
        {
            if (!IsPlatformHeadsetConnected)
            {
                signal_battery.SetActive(false);
                image_signal.sprite = signal0;
            }
            else if (!isDemo)
            {
                if (PoorSignal == 200)
                {
                    image_signal.sprite = signal1;
                }
                else if (PoorSignal >= 100)
                {
                    image_signal.sprite = signal2;
                }
                else if (PoorSignal > 0)
                {
                    image_signal.sprite = signal3;
                }
                else
                {
                    image_signal.sprite = signal4;
                }
                signal_battery.SetActive(BatteryCapacity4_0 > 0);
            }
            else
            {
                signal_battery.SetActive(isShowBattery);
                image_signal.sprite = signal4;
            }
            yield return null;
        }
    }

    IEnumerator ShowBatteryCapacity()
    {
        while (true)
        {
            if (IsPlatformHeadsetConnected)
            {
                filled_battary.fillAmount = GetBatteryCapacity() / 100.0f;
                if (GetBatteryCapacity() > 20)
                {
                    filled_battary.color = new Color(167 / 255f, 167 / 255f, 167 / 255f);
                }
                else
                {
                    filled_battary.color = new Color(237 / 255f, 22 / 255f, 80 / 255f);
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }

    /// <summary>
    /// Signal value:
    /// 0 = Bluetooth connected and headset correctly worn.
    /// 1-200 = Bluetooth connected but headset not correctly worn.
    /// 200 = Bluetooth not connected.
    /// </summary>
	public int GetWave_quality()
    {
        if(isDemo) { return 0; }
        return IsPlatformHeadsetConnected ? PoorSignal : 200;
    }

    public int GetAttention()
    {
        return IsPlatformHeadsetConnected ? Attention : 0;
    }

    public int GetMeditation()
    {
        return IsPlatformHeadsetConnected ? Meditation : 0;
    }

    public int GetBlink()
    {
        return IsPlatformHeadsetConnected ? Blink : 0;
    }

    public int GetRaw()
    {
        return IsPlatformHeadsetConnected ? Raw : 0;
    }

    public float GetDelta()
    {
        return IsPlatformHeadsetConnected ? Delta : 0f;
    }

    public float GetTheta()
    {
        return IsPlatformHeadsetConnected ? Theta : 0f;
    }

    public float GetHighAlpha()
    {
        return IsPlatformHeadsetConnected ? HighAlpha : 0f;
    }

    public float GetHighBeta()
    {
        return IsPlatformHeadsetConnected ? HighBeta : 0f;
    }

    public float GetHighGamma()
    {
        return IsPlatformHeadsetConnected ? HighGamma : 0f;
    }

    public float GetLowAlpha()
    {
        return IsPlatformHeadsetConnected ? LowAlpha : 0f;
    }

    public float GetLowBeta()
    {
        return IsPlatformHeadsetConnected ? LowBeta : 0f;
    }

    public float GetLowGamma()
    {
        return IsPlatformHeadsetConnected ? LowGamma : 0f;
    }
    /// <summary>
    /// tg.IsConnect2
    /// </summary>
	public bool IsHeadsetConnected()
    {
        return IsPlatformHeadsetConnected;
    }

    public string GetHardwareversion4_0()
    {
        return IsPlatformHeadsetConnected ? Hardwareversion4_0 : string.Empty;
    }

    public float Getxvalue4_0()
    {
        return IsPlatformHeadsetConnected ? xvalue4_0 : 0f;
    }
    public float Getyvalue4_0()
    {
        return IsPlatformHeadsetConnected ? yvalue4_0 : 0f;
    }

    public float Getzvalue4_0()
    {
        return IsPlatformHeadsetConnected ? zvalue4_0 : 0f;
    }

    public int GetAp4_0()
    {
        return IsPlatformHeadsetConnected ? ap : -1;
    }

    public float GetGrind4_0()
    {
        return IsPlatformHeadsetConnected ? grind : -1f;
    }

    public int GetBatteryCapacity()
    {
        if (isDemo) { return batteryCapacity4_0;  }
        return IsPlatformHeadsetConnected ? BatteryCapacity4_0 : 0;
    }

    public int GetHeartRate()
    {
        return IsPlatformHeadsetConnected ? heartRate : 0;
    }

    public float GetTemperature()
    {
        return IsPlatformHeadsetConnected ? temperature : 0f;
    }

    public string GetHRV()
    {
        return IsPlatformHeadsetConnected ? HRV : string.Empty;
    }
    

    // Accessory connect state...
    void ReceiveContentState(string data)
    {
        Debug.Log("ReceiveContentState   data = " + data);
        bool connected = string.Equals(data, "yes", StringComparison.OrdinalIgnoreCase);

#if UNITY_ANDROID && !UNITY_EDITOR
        bAndroidHeadsetConnected = connected;
#elif UNITY_IOS && !UNITY_EDITOR
        bIOSHeadsetConnected = connected;
#else
        bAndroidHeadsetConnected = connected;
        bIOSHeadsetConnected = connected;
#endif

        if (connected)
        {
            PoorSignal = 200;
            return;
        }

        isSendFirst = false;
        isApOn = 0;
        isCircleOn = 0;
        toggle_ap.isOn = false;
        toggle_circle.isOn = false;
    }

    void ReceiveRawdata(string data)
    {
        Raw = int.Parse(data);
    }

    void SendSettings(int isApOnValue, int isCircleOnValue)
    {
        string send = "B01:" + isApOnValue + "11" + isCircleOnValue + ";";
        string repeatedSend = send + send + send + send + send;

        toggle_ap.isOn = isApOnValue == 1;
        toggle_circle.isOn = isCircleOnValue == 1;

#if UNITY_IOS && !UNITY_EDITOR
        UnityThinkGear.SendSettings(repeatedSend);
#elif UNITY_ANDROID && !UNITY_EDITOR
         UnityThinkGear.sendSettings(repeatedSend);
#endif
    }

    void ReceivePoorSignal(string data)
    {
        PoorSignal = int.Parse(data);
        if (!isSendFirst)
        {
            isApOn = 0;
            isCircleOn = 1;
            SendSettings(isApOn, isCircleOn);
            isSendFirst = true;
        }

    }
    
    
    void ReceiveAttention(string data)
    {
        Attention = int.Parse(data);
    }
    void ReceiveMeditation(string data)
    {
        Meditation = int.Parse(data);
    }
    void ReceiveBatteryCapacity(string data)
    {
        BatteryCapacity4_0 = int.Parse(data);
    }
    //Delta
    void ReceiveDelta(string data)
    {
        Delta = int.Parse(data);
    }
    //Theta
    void ReceiveTheta(string data)
    {
        Theta = int.Parse(data);
    }

    //LowAlpha
    void ReceiveLowAlpha(string data)
    {
        LowAlpha = int.Parse(data);
    }
    //HighAlpha
    void ReceiveHighAlpha(string data)
    {
        HighAlpha = int.Parse(data);
    }

    //LowBeta
    void ReceiveLowBeta(string data)
    {
        LowBeta = int.Parse(data);
    }

    //HighBeta
    void ReceiveHighBeta(string data)
    {
        HighBeta = int.Parse(data);
    }

    //LowGamma
    void ReceiveLowGamma(string data)
    {
        LowGamma = int.Parse(data);
    }

    //HighGamma
    void ReceiveHighGamma(string data)
    {
        HighGamma = int.Parse(data);
    }

    void ReceiveXValue(string data)
    {
        xvalue4_0 = float.Parse(data);
    }
    void ReceiveYValue(string data)
    {
        yvalue4_0 = float.Parse(data);
    }
    void ReceiveZValue(string data)
    {
        zvalue4_0 = float.Parse(data);
    }
    //眨眼
    private void ReceiveBlink(string data)
    {
        Blink = int.Parse(data);
    }
    //咬牙
    void ReceiveGrind4_0(string value)
    {
        grind = int.Parse(value);
    }
    //喜好度
    void ReceiveAp4_0(string value)
    {
        ap = int.Parse(value);
    }

    void ReceiveHardwareversion4_0(string value)
    {
        Hardwareversion4_0 = value;
    }
    //心率
    void ReceiveHeaetRate(string value)
    {
        heartRate = int.Parse(value);
    }
    //额温
    void ReceiveTemperature(string value)
    {
        temperature = float.Parse(value);
    }
    
    //
    void ReceiveHRV(string value)
    {
        HRV = value;
    }

    public void OnValueChangeAp(bool isOn)
    {
        isApOn = isOn ? 1 : 0;
        Debug.Log(isOn ? "AP enabled" : "AP disabled");
        SendSettings(isApOn, isCircleOn);
    }
    
    public void OnValueChangeCircle(bool isOn)
    {
        isCircleOn = isOn ? 1 : 0;
        Debug.Log(isOn ? "Circle enabled" : "Circle disabled");
        SendSettings(isApOn, isCircleOn);
    }

}
