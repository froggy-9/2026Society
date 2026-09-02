using UnityEngine;

public class DocumentFollowDesk : MonoBehaviour
{
    [Header("Desk")]
    [SerializeField] private RectTransform deskContent;

    [Header("Document")]
    [SerializeField] private Vector2 documentPosition;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void LateUpdate()
    {
        if (deskContent == null)
            return;

        float zoom = deskContent.localScale.x;

        rectTransform.anchoredPosition = deskContent.anchoredPosition + documentPosition * zoom;
        rectTransform.localScale = Vector3.one * zoom;
    }
}