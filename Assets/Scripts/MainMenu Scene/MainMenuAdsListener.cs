using UnityEngine;
using UnityEngine.Advertisements;

public class MainMenuAdsListener : MonoBehaviour, IUnityAdsListener
{
    private CoinManager coinManager;
    private string gameId = "3507995";
    private string myPlacementId = "rewardedVideo";
    private bool testMode = true;

    void Start()
    {
        coinManager = GetComponent<CoinManager>();
        Advertisement.AddListener(this);
        Advertisement.Initialize(gameId, testMode);
    }
    public void ShowRewardedVideo()
    {
        Advertisement.Show(myPlacementId);
    }
    public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
    {
        if (showResult == ShowResult.Finished)
        {
            Debug.Log("Ads - Finished AD");
            coinManager.AddCoin(25);
        }
        else if (showResult == ShowResult.Skipped)
        {
            Debug.Log("Ads - Skipped AD");
        }
        else if (showResult == ShowResult.Failed)
        {
            Debug.LogWarning("Ads - The ad did not finish due to an error.");
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