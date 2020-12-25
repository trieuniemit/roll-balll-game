using UnityEngine;
using System.Collections;

public class LevelLoader : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        CUtils.ShowInterstitialAd();
        GameManager.instance.LoadLevel();
    }

    void Update()
    {
        GameManager.instance.UpdateLevel();
    }
}
