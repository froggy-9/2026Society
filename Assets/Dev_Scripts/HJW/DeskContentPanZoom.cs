using UnityEngine;
using UnityEngine.EventSystems;

public class DeskContentPanZoom : MonoBehaviour, IDragHandler, IScrollHandler
{
    [Header("Desk Content")]
    [SerializeField] private RectTransform deskContent;

    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 0.1f;
    [SerializeField] private float minZoom = 1f;
    [SerializeField] private float maxZoom = 2.5f;

    [Header("Move")]
    [SerializeField] private float dragSensitivity = 0.9f;

    [Header("Smooth")]
    [SerializeField] private float moveSmooth = 22f;
    [SerializeField] private float zoomSmooth = 18f;

    private RectTransform deskArea;
    private Vector2 targetPosition;
    private float targetZoom;
    private bool initialized;

    private void Awake()
    {
        deskArea = GetComponent<RectTransform>();
        ResetTarget();
    }

    private void OnEnable()
    {
        ResetTarget();
    }

    private void Update()
    {
        if (deskContent == null || deskArea == null || !initialized)
            return;

        float moveT = 1f - Mathf.Exp(-moveSmooth * Time.deltaTime);
        float zoomT = 1f - Mathf.Exp(-zoomSmooth * Time.deltaTime);

        float zoom = Mathf.Lerp(deskContent.localScale.x, targetZoom, zoomT);
        Vector2 position = Vector2.Lerp(
            deskContent.anchoredPosition,
            ClampPosition(targetPosition, zoom),
            moveT
        );

        if (Mathf.Abs(zoom - targetZoom) < 0.001f)
            zoom = targetZoom;

        if (Vector2.Distance(position, targetPosition) < 0.01f)
            position = targetPosition;

        deskContent.localScale = new Vector3(zoom, zoom, 1f);
        deskContent.anchoredPosition = position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (deskContent == null || deskArea == null)
            return;

        targetPosition += eventData.delta * dragSensitivity;
        targetPosition = ClampPosition(targetPosition, targetZoom);
    }

    public void OnScroll(PointerEventData eventData)
    {
        if (deskContent == null || deskArea == null)
            return;

        float scroll = eventData.scrollDelta.y;
        targetZoom = Mathf.Clamp(targetZoom + scroll * zoomSpeed, minZoom, maxZoom);
        targetPosition = ClampPosition(targetPosition, targetZoom);
    }

    private void ResetTarget()
    {
        if (deskContent == null)
            return;

        targetPosition = deskContent.anchoredPosition;
        targetZoom = Mathf.Clamp(deskContent.localScale.x, minZoom, maxZoom);
        initialized = true;
    }

    private Vector2 ClampPosition(Vector2 position, float zoom)
    {
        Vector2 contentSize = deskContent.rect.size * zoom;
        Vector2 areaSize = deskArea.rect.size;

        float maxX = Mathf.Max(0f, (contentSize.x - areaSize.x) / 2f);
        float maxY = Mathf.Max(0f, (contentSize.y - areaSize.y) / 2f);

        position.x = Mathf.Clamp(position.x, -maxX, maxX);
        position.y = Mathf.Clamp(position.y, -maxY, maxY);

        return position;
    }
}
