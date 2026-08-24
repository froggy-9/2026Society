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

    private RectTransform deskArea;

    private void Awake()
    {
        deskArea = GetComponent<RectTransform>();
    }

    // =========================
    // 드래그
    // =========================

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 currentPosition = deskContent.anchoredPosition;

        currentPosition += eventData.delta;

        deskContent.anchoredPosition =
            ClampPosition(currentPosition);
    }

    // =========================
    // 줌
    // =========================

    public void OnScroll(PointerEventData eventData)
    {
        float scroll = eventData.scrollDelta.y;

        float currentZoom = deskContent.localScale.x;

        float newZoom =
            currentZoom + scroll * zoomSpeed;

        newZoom = Mathf.Clamp(
            newZoom,
            minZoom,
            maxZoom
        );

        deskContent.localScale = new Vector3(
            newZoom,
            newZoom,
            1f
        );

        // 줌 후에도 영역 밖으로 나가지 않도록 위치 보정
        deskContent.anchoredPosition =
            ClampPosition(deskContent.anchoredPosition);
    }

    // =========================
    // 이동 범위 제한
    // =========================

    private Vector2 ClampPosition(Vector2 position)
    {
        Vector2 contentSize =
            Vector2.Scale(
                deskContent.rect.size,
                deskContent.localScale
            );

        Vector2 areaSize =
            deskArea.rect.size;

        float minX =
            (areaSize.x - contentSize.x) / 2f;

        float maxX =
            (contentSize.x - areaSize.x) / 2f;

        float minY =
            (areaSize.y - contentSize.y) / 2f;

        float maxY =
            (contentSize.y - areaSize.y) / 2f;

        position.x =
            Mathf.Clamp(
                position.x,
                minX,
                maxX
            );

        position.y =
            Mathf.Clamp(
                position.y,
                minY,
                maxY
            );

        return position;
    }
}
