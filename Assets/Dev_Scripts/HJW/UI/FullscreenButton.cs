using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FullscreenButton : MonoBehaviour
{
    public TMP_Text buttonText;
    public Image icon;

    public Sprite fullscreenIcon;
    public Sprite windowIcon;

    [SerializeField, Min(16)] private int windowedWidth = 1600;

    private FullScreenMode displayedMode;

    void OnEnable()
    {
        UpdateButton();
    }

    void Update()
    {
        // Screen mode changes are applied after the current frame.
        if (displayedMode != Screen.fullScreenMode)
            UpdateButton();
    }

    public void ToggleFullscreen()
    {
        if (Screen.fullScreenMode == FullScreenMode.Windowed)
        {
            // 창 모드 → 전체 화면
            Resolution desktopResolution = Screen.currentResolution;
            Screen.SetResolution(desktopResolution.width, desktopResolution.height,
                FullScreenMode.FullScreenWindow);
        }
        else
        {
            // 전체 화면 → 창 모드
            Resolution desktopResolution = Screen.currentResolution;
            // Leave room for window borders while keeping an exact 16:9 ratio.
            float maxWidth = Mathf.Min(windowedWidth, desktopResolution.width * 0.9f,
                desktopResolution.height * 0.9f * 16f / 9f);
            int sizeUnit = Mathf.Max(1, Mathf.FloorToInt(maxWidth / 16f));
            Screen.SetResolution(sizeUnit * 16, sizeUnit * 9, FullScreenMode.Windowed);
        }
    }

    void UpdateButton()
    {
        displayedMode = Screen.fullScreenMode;
        if (displayedMode == FullScreenMode.Windowed)
        {
            buttonText.text = "Windowed";
            icon.sprite = windowIcon;
        }
        else
        {
            buttonText.text = "Full Screen";
            icon.sprite = fullscreenIcon;
        }
    }
}
