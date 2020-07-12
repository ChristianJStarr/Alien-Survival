using UnityEngine;
using UnityEngine.Advertisements;

public class MainMenuAdsListener : MonoBehaviour, IUnityAdsListener
{
    private string gameId = "3507995";
    private string myPlacementId = "rewardedVideo";
    private bool testMode = true;
    private string storedIp;
    private ushort storedPort;

    void Start()
    {
        Advertisement.AddListener(this);
        Advertisement.Initialize(gameId, testMode);
    }
    public void ShowAd(string serverIp, ushort serverPort)
    {
        storedIp = serverIp;
        storedPort = serverPort;
        Advertisement.Show(myPlacementId);
    }
    public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
    {
        if (showResult == ShowResult.Finished)
        {
            ServerConnect serverConnect = FindObjectOfType<ServerConnect>();
            if (serverConnect != null)
            {
                serverConnect.ConnectToServer(storedIp, storedPort);
            }
        }
        else if (showResult == ShowResult.Skipped)
        {
            ServerConnect serverConnect = FindObjectOfType<ServerConnect>();
            if (serverConnect != null)
            {
                serverConnect.ConnectToServer(storedIp, storedPort);
            }
        }
        else if (showResult == ShowResult.Failed)
        {
            ServerConnect serverConnect = FindObjectOfType<ServerConnect>();
            if (serverConnect != null)
            {
                serverConnect.ConnectToServer(storedIp, storedPort);
            }
        }
    }

    public void OnUnityAdsReady(string placementId)
    {
        // If the ready Placement is rewarded, show the ad:
        if (placementId == myPlacementId)
        {
            //Advertisement.Show(myPlacementId);
        }
    }

    public void OnUnityAdsDidError(string message)
    {
        // Log the error.
    }

    public void OnUnityAdsDidStart(string placementId)
    {
        // Optional actions to take when the end-users triggers an ad.
    }
}