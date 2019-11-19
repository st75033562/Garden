using UnityEditor;

public class AndroidSigning
{
 
    [InitializeOnLoadMethod]
    public static void Setup()
    {
        PlayerSettings.Android.keystorePass = "123456";
        PlayerSettings.Android.keyaliasName = "active";
        PlayerSettings.Android.keyaliasPass = "123456";
    }
}
