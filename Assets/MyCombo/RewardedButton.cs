using UnityEngine;

public class RewardedButton : MonoBehaviour
{
    private const string ACTION_NAME = "rewarded_video";

    private void Start()
    {

    }

    public void OnClick()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (IsAvailableToShow())
            AdmobController.instance.ShowRewardBasedVideo();
        else if (!IsAdAvailable())
            Toast.instance.ShowMessage("Ad is not available now");
        else
        {
            int remainTime = (int)(GameConfig.instance.rewardedVideoPeriod - CUtils.GetActionDeltaTime(ACTION_NAME));
            Toast.instance.ShowMessage("Please wait " + remainTime + "seconds for the next ad");
        }
#endif
    }

    public bool IsAvailableToShow()
    {
        return IsActionAvailable() && IsAdAvailable();
    }

    private bool IsActionAvailable()
    {
        return CUtils.IsActionAvailable(ACTION_NAME, GameConfig.instance.rewardedVideoPeriod);
    }

    private bool IsAdAvailable()
    {
        if (AdmobController.instance.rewardBasedVideo == null) return false;
        bool isLoaded = AdmobController.instance.rewardBasedVideo.IsLoaded();
        return isLoaded;
    }
}
