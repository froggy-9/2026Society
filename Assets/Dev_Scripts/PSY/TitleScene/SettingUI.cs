using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using DG.Tweening;

public class SettingsUI : MonoBehaviour
{
    [Header("Settings Panel")]
    [SerializeField] private RectTransform settingsPanel;

    [Header("Game Logo")]
    [SerializeField] private RectTransform gameLogo;

    [Header("Settings Texts")]
    [SerializeField] private TMP_Text[] settingsTexts;

    [Header("Title Objects")]
    [SerializeField] private GameObject[] titleObjects;

    [Header("Other Objects To Disable")]
    [SerializeField] private GameObject[] disableObjects;

    [Header("Logo Target Position")]
    [SerializeField] private Vector3 logoTargetPosition =
        new Vector3(545f, 333f, 0f);

    [Header("Animation")]
    [SerializeField] private float panelDuration = 0.6f;
    [SerializeField] private float logoDuration = 0.6f;
    [SerializeField] private float textFadeDuration = 0.4f;

    private Vector2 panelOriginalPosition;
    private Vector3 logoOriginalPosition;

    private bool isOpen = false;
    private bool isAnimating = false;

    private void Awake()
    {
        // 원래 위치 저장
        panelOriginalPosition = settingsPanel.anchoredPosition;
        logoOriginalPosition = gameLogo.anchoredPosition3D;

        // 패널을 화면 아래로 이동
        settingsPanel.anchoredPosition =
            panelOriginalPosition + new Vector2(0f, -Screen.height);

        // Settings 텍스트 숨김
        foreach (TMP_Text text in settingsTexts)
        {
            if (text != null)
                text.alpha = 0f;
        }
    }

    private void Update()
    {
        if (!isOpen)
            return;

        // ESC로만 닫기
        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseSettings();
        }
    }

    // ========================================
    // SETTINGS OPEN
    // ========================================

    public void OpenSettings()
    {
        if (isOpen || isAnimating)
            return;

        isOpen = true;
        isAnimating = true;

        settingsPanel.DOKill();
        gameLogo.DOKill();

        // ------------------------------------
        // 타이틀 오브젝트 비활성화
        // ------------------------------------

        foreach (GameObject obj in titleObjects)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        // ------------------------------------
        // 기타 오브젝트 비활성화
        // ------------------------------------

        foreach (GameObject obj in disableObjects)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        // ------------------------------------
        // 패널 아래 → 원래 위치
        // ------------------------------------

        settingsPanel
            .DOAnchorPos(
                panelOriginalPosition,
                panelDuration
            )
            .SetEase(Ease.OutCubic);

        // ------------------------------------
        // 로고 이동
        // ------------------------------------

        gameLogo
            .DOAnchorPos3D(
                logoTargetPosition,
                logoDuration
            )
            .SetEase(Ease.OutCubic);

        // ------------------------------------
        // Settings 텍스트 Fade In
        // ------------------------------------

        foreach (TMP_Text text in settingsTexts)
        {
            if (text != null)
            {
                text.DOKill();
                text.DOFade(
                    1f,
                    textFadeDuration
                );
            }
        }

        DOVirtual.DelayedCall(
            Mathf.Max(
                panelDuration,
                logoDuration,
                textFadeDuration
            ),
            () =>
            {
                isAnimating = false;
            }
        );
    }

    // ========================================
    // SETTINGS CLOSE
    // ========================================

    public void CloseSettings()
    {
        if (!isOpen || isAnimating)
            return;

        isAnimating = true;

        settingsPanel.DOKill();
        gameLogo.DOKill();

        // ------------------------------------
        // Settings 텍스트 Fade Out
        // ------------------------------------

        foreach (TMP_Text text in settingsTexts)
        {
            if (text != null)
            {
                text.DOKill();

                text.DOFade(
                    0f,
                    textFadeDuration
                );
            }
        }

        // ------------------------------------
        // 로고 원래 위치
        // ------------------------------------

        gameLogo
            .DOAnchorPos3D(
                logoOriginalPosition,
                logoDuration
            )
            .SetEase(Ease.InCubic);

        // ------------------------------------
        // 패널 아래로 이동
        // ------------------------------------

        settingsPanel
            .DOAnchorPos(
                panelOriginalPosition +
                new Vector2(0f, -Screen.height),
                panelDuration
            )
            .SetEase(Ease.InCubic)
            .OnComplete(() =>
            {
                // --------------------------------
                // 타이틀 오브젝트 다시 활성화
                // --------------------------------

                foreach (GameObject obj in titleObjects)
                {
                    if (obj != null)
                        obj.SetActive(true);
                }

                // --------------------------------
                // 기타 오브젝트 다시 활성화
                // --------------------------------

                foreach (GameObject obj in disableObjects)
                {
                    if (obj != null)
                        obj.SetActive(true);
                }

                isOpen = false;
                isAnimating = false;
            });
    }
}