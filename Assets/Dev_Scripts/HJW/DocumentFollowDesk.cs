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
        float zoom = deskContent.localScale.x;

        // DeskContent의 이동 + 확대 비율 적용
        rectTransform.anchoredPosition =
            deskContent.anchoredPosition +
            documentPosition * zoom;

        // 서류 자체도 같은 배율로 확대
        rectTransform.localScale =
            Vector3.one * zoom;
    }
}
