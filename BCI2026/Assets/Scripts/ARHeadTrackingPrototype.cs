using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Management;

public sealed class ARHeadTrackingPrototype : MonoBehaviour
{
    private enum HeadDirection
    {
        Searching,
        Center,
        Left,
        Right,
    }

    [SerializeField] private ARFaceManager faceManager;
    [SerializeField] private float yawThresholdDegrees = 12f;
    [SerializeField] private float directionHoldSeconds = 0.12f;
    [SerializeField] private bool invertHorizontal;

    private Text directionText;
    private Text statusText;
    private float neutralYaw;
    private float smoothedYaw;
    private float calibrationEndsAt;
    private float calibrationSin;
    private float calibrationCos;
    private int calibrationSamples;
    private bool isCalibrating;
    private HeadDirection currentDirection = HeadDirection.Searching;
    private HeadDirection pendingDirection = HeadDirection.Searching;
    private float pendingSince;

    private void Awake()
    {
        faceManager ??= GetComponent<ARFaceManager>();
        CreateInterface();
    }

    private void Start()
    {
        statusText.text = "Esperando la camara AR y una cara.";
        StartCoroutine(CheckFaceTrackingSupport());
    }

    private IEnumerator CheckFaceTrackingSupport()
    {
        yield return ARSession.CheckAvailability();
        if (ARSession.state == ARSessionState.Unsupported)
        {
#if UNITY_IOS
            string loaderName = XRGeneralSettings.Instance?.Manager?.activeLoader?.GetType().Name ?? "ninguno";
            statusText.text = $"ARKit no se inicio. Estado: {ARSession.state}. Loader: {loaderName}.";
#elif UNITY_ANDROID
            statusText.text = "ARCore no es compatible con este dispositivo.";
#else
            statusText.text = "El seguimiento AR no es compatible con este dispositivo.";
#endif
        }
    }

    private void Update()
    {
        ARFace face = FindFace();
        if (face == null || face.trackingState != UnityEngine.XR.ARSubsystems.TrackingState.Tracking)
        {
            SetDirection(HeadDirection.Searching);
            return;
        }

        float yaw = face.transform.localEulerAngles.y;
        smoothedYaw = Mathf.LerpAngle(smoothedYaw, yaw, 0.25f);

        if (isCalibrating)
        {
            calibrationSin += Mathf.Sin(smoothedYaw * Mathf.Deg2Rad);
            calibrationCos += Mathf.Cos(smoothedYaw * Mathf.Deg2Rad);
            calibrationSamples++;
            if (Time.unscaledTime >= calibrationEndsAt)
            {
                isCalibrating = false;
                neutralYaw = Mathf.Atan2(calibrationSin, calibrationCos) * Mathf.Rad2Deg;
                statusText.text = "Centro calibrado. Gira la cabeza a izquierda o derecha.";
            }
        }

        float offset = Mathf.DeltaAngle(neutralYaw, smoothedYaw);
        if (invertHorizontal)
        {
            offset = -offset;
        }
        SetDirection(Classify(offset));
    }

    public void StartCalibration()
    {
        calibrationSin = 0f;
        calibrationCos = 0f;
        calibrationSamples = 0;
        calibrationEndsAt = Time.unscaledTime + 1f;
        isCalibrating = true;
        statusText.text = "Calibrando. Mantiene la cabeza centrada durante un segundo.";
    }

    private ARFace FindFace()
    {
        if (faceManager == null)
        {
            return null;
        }

        foreach (ARFace face in faceManager.trackables)
        {
            return face;
        }
        return null;
    }

    private HeadDirection Classify(float offset)
    {
        if (offset <= -yawThresholdDegrees)
        {
            return HeadDirection.Left;
        }
        if (offset >= yawThresholdDegrees)
        {
            return HeadDirection.Right;
        }
        return HeadDirection.Center;
    }

    private void SetDirection(HeadDirection candidate)
    {
        if (candidate != currentDirection)
        {
            if (candidate != pendingDirection)
            {
                pendingDirection = candidate;
                pendingSince = Time.unscaledTime;
            }
            else if (Time.unscaledTime - pendingSince >= directionHoldSeconds)
            {
                currentDirection = candidate;
            }
        }

        directionText.text = currentDirection switch
        {
            HeadDirection.Left => "IZQUIERDA",
            HeadDirection.Right => "DERECHA",
            HeadDirection.Center => "CENTRO",
            _ => "BUSCANDO CARA",
        };
    }

    private void CreateInterface()
    {
        var canvasObject = new GameObject("Head Tracking UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);

        var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup));
        content.transform.SetParent(canvas.transform, false);
        var contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.pivot = new Vector2(0.5f, 0.5f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(920, 700);
        var layout = content.GetComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 42;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        directionText = CreateText(content.transform, 72, new Vector2(880, 140));
        statusText = CreateText(content.transform, 30, new Vector2(880, 130));
        CreateButton(content.transform);
        SetDirection(HeadDirection.Searching);
    }

    private static Text CreateText(Transform parent, int fontSize, Vector2 size)
    {
        var gameObject = new GameObject("Text", typeof(Text), typeof(LayoutElement));
        gameObject.transform.SetParent(parent, false);
        var text = gameObject.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        var layout = gameObject.GetComponent<LayoutElement>();
        layout.preferredWidth = size.x;
        layout.preferredHeight = size.y;
        return text;
    }

    private void CreateButton(Transform parent)
    {
        var gameObject = new GameObject("Calibrar centro", typeof(Image), typeof(Button), typeof(LayoutElement));
        gameObject.transform.SetParent(parent, false);
        var image = gameObject.GetComponent<Image>();
        image.color = new Color(0.15f, 0.45f, 0.8f);
        var button = gameObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(StartCalibration);
        var layout = gameObject.GetComponent<LayoutElement>();
        layout.preferredWidth = 440;
        layout.preferredHeight = 100;

        var label = CreateText(gameObject.transform, 34, new Vector2(440, 100));
        label.text = "Calibrar centro";
        var labelRect = label.rectTransform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
    }
}
