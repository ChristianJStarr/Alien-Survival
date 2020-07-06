
using UnityEngine;

public class DeepLink : MonoBehaviour
{
    /// Name of your scheme
    public string Scheme = "aliensurvival";
    /// Your unique URL path that is after "scheme://"
    public string UniquePath = "callback/oauth/";

    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void configure(string gameObjName, string methodName);

    private void Start()
    {
        // Configure bridge to callback to this game object and the OnDeepLinkURLOpened method
        configure(this.gameObject.name, "OnDeepLinkURLOpened");
    }

    public void OnDeepLinkURLOpened(string deepLinkUrl)
    {
        // Callback method for recieveing Deep Link on iOS
        if (string.IsNullOrEmpty(deepLinkUrl))
            return;

        if (deepLinkUrl.Contains(Scheme + "://" + UniquePath))
        {
            // This string is your deep link url
            // Code goes here to handle it
        }
    }
}
