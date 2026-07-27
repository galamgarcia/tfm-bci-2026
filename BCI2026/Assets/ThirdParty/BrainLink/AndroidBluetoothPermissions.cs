using System.Collections;
using UnityEngine;

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

public class AndroidBluetoothPermissions : MonoBehaviour
{
    private IEnumerator Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        yield return RequestPermission(Permission.FineLocation);

        if (AndroidVersionIsAtLeast(12))
        {
            yield return RequestPermission("android.permission.BLUETOOTH_SCAN");
            yield return RequestPermission("android.permission.BLUETOOTH_CONNECT");
        }
#endif
        yield break;
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private static IEnumerator RequestPermission(string permission)
    {
        if (!Permission.HasUserAuthorizedPermission(permission))
        {
            Permission.RequestUserPermission(permission);

            while (!Permission.HasUserAuthorizedPermission(permission))
            {
                yield return null;
            }
        }
    }

    private static bool AndroidVersionIsAtLeast(int version)
    {
        using var versionClass = new AndroidJavaClass("android.os.Build$VERSION");
        int sdkVersion = versionClass.GetStatic<int>("SDK_INT");

        return sdkVersion >= version;
    }
#endif
}
