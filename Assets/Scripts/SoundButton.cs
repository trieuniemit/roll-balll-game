using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SoundButton : MonoBehaviour
{
    public Sprite soundButtonEnabled, soundButtonDisabled;

    void Start()
    {
        if (Prefs.soundEnabled)
            GetComponent<Image>().sprite = soundButtonEnabled;
        else
            GetComponent<Image>().sprite = soundButtonDisabled;
    }

    public void ToggleSoundButton()
    {
        Prefs.soundEnabled = !Prefs.soundEnabled;

        if (Prefs.soundEnabled)
            SoundManager.instance.PlaySound(SoundManager.instance.music);
        else
            SoundManager.instance.music.Stop();

        LoadSoundButtonImage();
    }

    void LoadSoundButtonImage()
    {
        if (Prefs.soundEnabled)
            GetComponent<Image>().sprite = soundButtonEnabled;
        else
            GetComponent<Image>().sprite = soundButtonDisabled;
    }
}
