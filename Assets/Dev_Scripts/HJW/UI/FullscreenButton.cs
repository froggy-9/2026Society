using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FullscreenButton : MonoBehaviour
{
    public TMP_Text buttonText;
    public Image icon;

    public Sprite fullscreenIcon;
    public Sprite windowIcon;

    void Start()
    {
        UpdateButton();
    }

    public void ToggleFullscreen()
    {
        if (Screen.fullScreenMode == FullScreenMode.Windowed)
        {
            // 창 모드 → 전체 화면
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        }
        else
        {
            // 전체 화면 → 창 모드
            Screen.fullScreenMode = FullScreenMode.Windowed;
        }

        // 실제 변경된 상태를 다시 확인
        UpdateButton();

        Debug.Log("현재 화면 모드 : " + Screen.fullScreenMode);
    }

    void UpdateButton()
    {
        if (Screen.fullScreenMode == FullScreenMode.Windowed)
        {
            // 현재 창 모드 → 버튼은 전체 화면으로 바꾸는 기능
            buttonText.text = "Full Screen";
            icon.sprite = fullscreenIcon;
        }
        else
        {
            // 현재 전체 화면 → 버튼은 창 모드로 바꾸는 기능
            buttonText.text = "Windowed";
            icon.sprite = windowIcon;
        }
    }
}
