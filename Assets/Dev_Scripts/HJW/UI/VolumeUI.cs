using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VolumeUI : MonoBehaviour
{
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private TMP_Text bgmText;

    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TMP_Text sfxText;

    private GameAudioManager audioManager;

    private void Awake()
    {
        audioManager = GameAudioManager.GetOrCreate();
    }

    private void Start()
    {
        if (bgmSlider != null)
        {
            bgmSlider.SetValueWithoutNotify(audioManager.BgmVolume);
            bgmSlider.onValueChanged.AddListener(ChangeBGM);
        }

        if (sfxSlider != null)
        {
            sfxSlider.SetValueWithoutNotify(audioManager.SfxVolume);
            sfxSlider.onValueChanged.AddListener(ChangeSFX);
        }

        ChangeBGM(audioManager.BgmVolume);
        ChangeSFX(audioManager.SfxVolume);
    }

    private void OnDestroy()
    {
        if (bgmSlider != null)
            bgmSlider.onValueChanged.RemoveListener(ChangeBGM);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(ChangeSFX);
    }

    private void ChangeBGM(float value)
    {
        audioManager.SetBgmVolume(value);

        if (bgmText != null)
            bgmText.text = Mathf.RoundToInt(value * 100) + "%";
    }

    private void ChangeSFX(float value)
    {
        audioManager.SetSfxVolume(value);

        if (sfxText != null)
            sfxText.text = Mathf.RoundToInt(value * 100) + "%";
    }
}
