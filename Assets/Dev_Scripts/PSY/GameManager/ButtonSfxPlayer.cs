using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonSfxPlayer : MonoBehaviour
{
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        if (button == null)
            button = GetComponent<Button>();

        button.onClick.AddListener(PlayClickSfx);
    }

    private void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(PlayClickSfx);
    }

    private void PlayClickSfx()
    {
        if (GameAudioManager.Instance != null)
            GameAudioManager.Instance.PlayDefaultButtonSfx();
    }
}
