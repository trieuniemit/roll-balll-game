using UnityEngine;
using System;
using GoogleMobileAds.Api;

public class AdmobController : MonoBehaviour
{
    private BannerView bannerView;
    public InterstitialAd interstitial;
    public RewardBasedVideoAd rewardBasedVideo;

    [Header("Interstitial")]
    public string androidInterstitial;
    public string iosInterstitial;

    [Header("Banner")]
    public string androidBanner;
    public string iosBanner;

    [Header("Rewarded Video")]
    public string androidRewardedVideo;
    public string iosRewardedVideo;

    public static AdmobController instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        RequestBanner();
        RequestInterstitial();

        InitRewardedVideo();
        RequestRewardBasedVideo();
    }

    private void InitRewardedVideo()
    {
        // Get singleton reward based video ad reference.
        this.rewardBasedVideo = RewardBasedVideoAd.Instance;

        // RewardBasedVideoAd is a singleton, so rs should only be registered once.
        this.rewardBasedVideo.OnAdLoaded += this.RewardBasedVideoLoaded;
        this.rewardBasedVideo.OnAdFailedToLoad += this.RewardBasedVideoFailedToLoad;
        this.rewardBasedVideo.OnAdOpening += this.RewardBasedVideoOpened;
        this.rewardBasedVideo.OnAdStarted += this.RewardBasedVideoStarted;
        this.rewardBasedVideo.OnAdRewarded += this.RewardBasedVideoRewarded;
        this.rewardBasedVideo.OnAdClosed += this.RewardBasedVideoClosed;
        this.rewardBasedVideo.OnAdLeavingApplication += this.RewardBasedVideoLeftApplication;
    }

    public void RequestBanner()
    {
        // These ad units are configured to always serve test ads.
        #if UNITY_EDITOR
            string adUnitId = "ca-app-pub-3940256099942544/6300978111"; // test
        #elif UNITY_ANDROID
            string adUnitId = androidBanner.Trim();
        #elif UNITY_IPHONE
            string adUnitId = iosBanner.Trim();
        #else
            string adUnitId = "unexpected_platform";
        #endif

        // Create a 320x50 banner at the top of the screen.
        this.bannerView = new BannerView(adUnitId, AdSize.SmartBanner, AdPosition.Bottom);

        // Register for ad events.
        this.bannerView.OnAdLoaded += this.AdLoaded;
        this.bannerView.OnAdFailedToLoad += this.AdFailedToLoad;
        this.bannerView.OnAdOpening += this.AdOpened;
        this.bannerView.OnAdClosed += this.AdClosed;
        this.bannerView.OnAdLeavingApplication += this.AdLeftApplication;
        // Load a banner ad.
        this.bannerView.LoadAd(this.CreateAdRequest());
    }

    public void RequestInterstitial()
    {
        // These ad units are configured to always serve test ads.
        #if UNITY_EDITOR
            string adUnitId = "ca-app-pub-3940256099942544/1033173712"; // test
        #elif UNITY_ANDROID
            string adUnitId = androidInterstitial.Trim();
        #elif UNITY_IPHONE
            string adUnitId = iosInterstitial.Trim();
        #else
            string adUnitId = "unexpected_platform";
        #endif

        // Create an interstitial.
        this.interstitial = new InterstitialAd(adUnitId);

        // Register for ad events.
        this.interstitial.OnAdLoaded += this.InterstitialLoaded;
        this.interstitial.OnAdFailedToLoad += this.InterstitialFailedToLoad;
        this.interstitial.OnAdOpening += this.InterstitialOpened;
        this.interstitial.OnAdClosed += this.InterstitialClosed;
        this.interstitial.OnAdLeavingApplication += this.InterstitialLeftApplication;

        // Load an interstitial ad.
        this.interstitial.LoadAd(this.CreateAdRequest());
    }

    public void RequestRewardBasedVideo()
    {
        #if UNITY_EDITOR
            string adUnitId = "ca-app-pub-3940256099942544/5224354917"; //test
        #elif UNITY_ANDROID
            string adUnitId = androidRewardedVideo.Trim();
        #elif UNITY_IPHONE
            string adUnitId = iosRewardedVideo.Trim();
        #else
            string adUnitId = "unexpected_platform";
        #endif

        this.rewardBasedVideo.LoadAd(this.CreateAdRequest(), adUnitId);
    }

    // Returns an ad request with custom ad targeting.
    private AdRequest CreateAdRequest()
    {
        return new AdRequest.Builder()
                .AddTestDevice(AdRequest.TestDeviceSimulator)
                .AddTestDevice("04BF0AFEC9ABA620692118919F24A681")
                .AddTestDevice("F0330A41B8F8D4153EACE15A7091C81A")
                .AddKeyword("game")
                .TagForChildDirectedTreatment(false)
                .AddExtra("color_bg", "9B30FF")
                .Build();
    }

    public void ShowInterstitial(InterstitialAd ad)
    {
        if (ad != null && ad.IsLoaded())
        {
            ad.Show();
        }
    }

    public void ShowBanner()
    {
        if (bannerView != null)
        {
            bannerView.Show();
        }
    }

    public void HideBanner()
    {
        if (bannerView != null)
        {
            bannerView.Hide();
        }
    }

    public bool ShowInterstitial()
    {
        if (interstitial != null && interstitial.IsLoaded())
        {
            interstitial.Show();
            return true;
        }
        return false;
    }

    public void ShowRewardBasedVideo()
    {
        if (this.rewardBasedVideo.IsLoaded())
        {
            this.rewardBasedVideo.Show();
        }
        else
        {
            Debug.Log("Ads: Reward based video ad is not ready yet");
        }
    }

    #region Banner callback rs

    public void AdLoaded(object sender, EventArgs args)
    {
        //HideBanner();
        Debug.Log("Ads: AdLoaded event received.");
    }

    public void AdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        Debug.Log("Ads: FailedToReceiveAd event received with message: " + args.Message);
    }

    public void AdOpened(object sender, EventArgs args)
    {
        Debug.Log("Ads: AdOpened event received");
    }

    public void AdClosed(object sender, EventArgs args)
    {
        Debug.Log("Ads: AdClosed event received");
    }

    public void AdLeftApplication(object sender, EventArgs args)
    {
        Debug.Log("Ads: AdLeftApplication event received");
    }

    #endregion

    #region Interstitial callback rs

    public void InterstitialLoaded(object sender, EventArgs args)
    {
        Debug.Log("Ads: InterstitialLoaded event received.");
    }

    public void InterstitialFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        Debug.Log("Ads: InterstitialFailedToLoad event received with message: " + args.Message);
    }

    public void InterstitialOpened(object sender, EventArgs args)
    {
        Debug.Log("Ads: InterstitialOpened event received");
    }

    public void InterstitialClosed(object sender, EventArgs args)
    {
        Debug.Log("Ads: InterstitialClosed event received");
        RequestInterstitial();
    }

    public void InterstitialLeftApplication(object sender, EventArgs args)
    {
        Debug.Log("Ads: InterstitialLeftApplication event received ---------");
    }

    #endregion

    #region RewardBasedVideo callback rs

    public void RewardBasedVideoLoaded(object sender, EventArgs args)
    {
        Debug.Log("Ads: RewardBasedVideoLoaded event received");
    }

    public void RewardBasedVideoFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        Debug.Log("Ads: RewardBasedVideoFailedToLoad event received with message: " + args.Message);
    }

    public void RewardBasedVideoOpened(object sender, EventArgs args)
    {
        Debug.Log("Ads: RewardBasedVideoOpened event received");
    }

    public void RewardBasedVideoStarted(object sender, EventArgs args)
    {
        Debug.Log("Ads: RewardBasedVideoStarted event received");
    }

    public void RewardBasedVideoClosed(object sender, EventArgs args)
    {
        RequestRewardBasedVideo();
        Debug.Log("Ads: RewardBasedVideoClosed event received");
    }

    public void RewardBasedVideoRewarded(object sender, Reward args)
    {
        string type = args.Type;
        double amount = args.Amount;
        Debug.Log("Ads: RewardBasedVideoRewarded event received for " + amount.ToString() + " " + type);
    }

    public void RewardBasedVideoLeftApplication(object sender, EventArgs args)
    {
        Debug.Log("Ads: RewardBasedVideoLeftApplication event received");
    }

    #endregion
}
