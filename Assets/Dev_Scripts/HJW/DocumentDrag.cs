using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DocumentDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI")]
    [SerializeField] private GameObject smallPassportUI;
    [SerializeField] private GameObject detailPassportUI;

    [Header("Desk")]
    [SerializeField] private RectTransform deskArea;
    [SerializeField] private RectTransform deskContent;

    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private Vector2 startPosition;
    private Vector2 smallSize;
    private Vector3 startScale;
    private Transform startParent;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        startPosition = rectTransform.anchoredPosition;
        smallSize = rectTransform.sizeDelta;
        startScale = rectTransform.localScale;
        startParent = transform.parent;

        smallPassportUI.SetActive(true);
        detailPassportUI.SetActive(false);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = false;

        if (canvas != null && transform.parent == deskContent)
            MoveToCanvas();

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
            MoveToDesk(eventData);

            smallPassportUI.SetActive(false);
            detailPassportUI.SetActive(true);

            ApplyDeskScale();
        }
        else
        {
            if (startParent != null && transform.parent != startParent)
                transform.SetParent(startParent, false);

            smallPassportUI.SetActive(true);
            detailPassportUI.SetActive(false);

            rectTransform.sizeDelta = smallSize;
            rectTransform.localScale = startScale;

            rectTransform.anchoredPosition = startPosition;
        }
    }

    private void MoveToCanvas()
    {
        RectTransform canvasRect = canvas.transform as RectTransform;

        if (canvasRect == null)
            return;

        Camera camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(camera, rectTransform.position);

        transform.SetParent(canvas.transform, false);

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, camera, out Vector2 localPosition))
            rectTransform.anchoredPosition = localPosition;

        rectTransform.localScale = startScale;
    }

    private void MoveToDesk(PointerEventData eventData)
    {
        if (deskContent == null)
            return;

        Camera camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera
            : null;

        transform.SetParent(deskContent, false);

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(deskContent, eventData.position, camera, out Vector2 localPosition))
            rectTransform.anchoredPosition = localPosition;

        ApplyDeskScale();
    }

    private void ApplyDeskScale()
    {
        if (deskContent == null)
        {
            rectTransform.localScale = startScale;
            return;
        }

        Vector3 parentScale = deskContent.lossyScale;

        if (Mathf.Approximately(parentScale.y, 0f))
        {
            rectTransform.localScale = startScale;
            return;
        }

        rectTransform.localScale = new Vector3(
            startScale.x,
            startScale.y * parentScale.x / parentScale.y,
            startScale.z
        );
    }
}
