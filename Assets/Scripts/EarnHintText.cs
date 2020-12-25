using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EarnHintText : MonoBehaviour {

    private void Start()
    {
        var textUI = GetComponent<Text>();
        textUI.text = string.Format(textUI.text, GameConfig.instance.rewardedVideoAmount);
    }
}
