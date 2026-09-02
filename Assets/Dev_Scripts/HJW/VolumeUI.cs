using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VolumeUI : MonoBehaviour
{
    public Slider bgmSlider;
    public TMP_Text bgmText;

    public Slider sfxSlider;
    public TMP_Text sfxText;

    void Start()
    {
        bgmSlider.onValueChanged.AddListener(ChangeBGM);
        sfxSlider.onValueChanged.AddListener(ChangeSFX);

        ChangeBGM(bgmSlider.value);
        ChangeSFX(sfxSlider.value);
    }

    void ChangeBGM(float value)
    {
        bgmText.text = Mathf.RoundToInt(value * 100) + "%";
    }

    void ChangeSFX(float value)
    {
        sfxText.text = Mathf.RoundToInt(value * 100) + "%";
    }
}
