using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DocumentDrag : MonoBehaviour, IBeginDragHandler,IDragHandler,IEndDragHandler
{
    [Header("UI")]
    [SerializeField] private GameObject smallPassportUI;
    [SerializeField] private GameObject detailPassportUI;

    [Header("Desk")]
    [SerializeField] private RectTransform deskArea;

    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private Vector2 startPosition;
    private Vector2 smallSize;

    [SerializeField] private Vector2 detailSize = new Vector2(500, 350);

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        startPosition = rectTransform.anchoredPosition;
        smallSize = rectTransform.sizeDelta;

        // 시작 상태
        smallPassportUI.SetActive(true);
        detailPassportUI.SetActive(false);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = false;

        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition +=
            eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        bool insideDesk =
            RectTransformUtility.RectangleContainsScreenPoint(
                deskArea,
                eventData.position,
                null
            );

        if (insideDesk)
        {
            // 상세 여권으로 변경
            smallPassportUI.SetActive(false);
            detailPassportUI.SetActive(true);

            rectTransform.sizeDelta = detailSize;
        }
        else
        {
            // NPC 영역으로 돌아감
            smallPassportUI.SetActive(true);
            detailPassportUI.SetActive(false);

            rectTransform.sizeDelta = smallSize;

            // 원래 제출 위치로 복귀
            rectTransform.anchoredPosition = startPosition;
        }
    }
}
