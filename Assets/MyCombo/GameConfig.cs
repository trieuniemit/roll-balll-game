using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameConfig : MonoBehaviour
{
    public string androidPackageName;
    public string iosAppID;

    public int rewardedVideoPeriod;
    public int rewardedVideoAmount;

    public int adPeriod;

    public bool unlockAllLevelForTesting = false;

    public static GameConfig instance;
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
}
