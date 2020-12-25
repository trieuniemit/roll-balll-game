using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Prefs
{
    public static Action onHintCountChanged;
    public static int hintCount
    {
        get { return PlayerPrefs.GetInt("Hints", 5); }
        set {
            PlayerPrefs.SetInt("Hints", value);
            if (onHintCountChanged != null) onHintCountChanged();
        }
    }

    public static bool soundEnabled
    {
        get { return PlayerPrefs.GetInt("SoundSettings", 1) == 1; }
        set { PlayerPrefs.SetInt("SoundSettings", value ? 1 : 0); }
    }
}
