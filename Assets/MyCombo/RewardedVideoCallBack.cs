using GoogleMobileAds.Api;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RewardedVideoCallBack : MonoBehaviour {

    private void Start()
    {
        #if UNITY_ANDROID || UNITY_IOS
            if (AdmobController.instance.rewardBasedVideo != null)
            {
                AdmobController.instance.rewardBasedVideo.OnAdRewarded += HandleRewardBasedVideoRewarded;
            }
        #endif
    }

    private const string ACTION_NAME = "rewarded_video";
    public void HandleRewardBasedVideoRewarded(object sender, Reward args)
    {
        Prefs.hintCount += GameConfig.instance.rewardedVideoAmount;
        CUtils.SetActionTime(ACTION_NAME);
        Toast.instance.ShowMessage(string.Format("You have received {0} hints", GameConfig.instance.rewardedVideoAmount), 2.5f);
    }

    private void OnDestroy()
    {
        #if UNITY_ANDROID || UNITY_IOS
            if (AdmobController.instance.rewardBasedVideo != null)
            {
                AdmobController.instance.rewardBasedVideo.OnAdRewarded -= HandleRewardBasedVideoRewarded;
            }
        #endif
    }
}
