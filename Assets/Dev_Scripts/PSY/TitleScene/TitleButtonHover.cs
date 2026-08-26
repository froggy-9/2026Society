using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

public class TitleButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Text")]
    [SerializeField] private TMP_Text text;

    [Header("Exit Button")]
    [SerializeField] private bool isExitButton = false;

    [Header("Normal")]
    [SerializeField] private Color normalColor = new Color32(225, 225, 225, 255);

    [Header("Normal Button Hover")]
    [SerializeField] private Color hoverColor = Color.white;
    [SerializeField] private float hoverScale = 1.03f;
    [SerializeField] private float duration = 0.2f;

    [Header("Exit Hover")]
    [SerializeField] private Color exitHoverColor = new Color32(220, 60, 60, 255);

    private string originalText;

    private void Awake()
    {
        originalText = text.text;

        text.color = normalColor;
        transform.localScale = Vector3.one;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        text.DOKill();
        transform.DOKill();

        // ========================================
        // EXIT BUTTON
        // ========================================

        if (isExitButton)
        {
            // 텍스트 변경 없음
            text.text = originalText;

            // 크기 변화 없음
            transform.localScale = Vector3.one;

            // 빨간색으로 변경
            text.DOColor(
                exitHoverColor,
                duration
            ).SetEase(Ease.OutQuad);

            return;
        }


        // ========================================
        // NORMAL BUTTON
        // ========================================

        text.text = "> " + originalText;

        transform.DOScale(
            hoverScale,
            duration
        ).SetEase(Ease.OutQuad);

        text.DOColor(
            hoverColor,
            duration
        ).SetEase(Ease.OutQuad);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        text.DOKill();
        transform.DOKill();

        // ========================================
        // EXIT BUTTON
        // ========================================

        if (isExitButton)
        {
            text.text = originalText;

            transform.localScale = Vector3.one;

            text.DOColor(
                normalColor,
                duration
            ).SetEase(Ease.OutQuad);

            return;
        }


        // ========================================
        // NORMAL BUTTON
        // ========================================

        text.text = originalText;

        transform.DOScale(
            1f,
            duration
        ).SetEase(Ease.OutQuad);

        text.DOColor(
            normalColor,
            duration
        ).SetEase(Ease.OutQuad);
    }
}