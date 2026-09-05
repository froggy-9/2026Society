using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlidePopupTabButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Move")]
    [Tooltip("실제로 움직일 이미지 RectTransform입니다. 비워두면 이 오브젝트 자체가 움직입니다.")]
    [SerializeField] private RectTransform visualRoot;

    [Tooltip("마우스를 올렸을 때 이미지가 도착할 위치입니다. 버튼 중앙까지 나오게 하려면 (0, 0)으로 둡니다.")]
    [SerializeField] private Vector2 visibleAnchoredPosition = Vector2.zero;

    [Tooltip("이동 속도입니다.")]
    [SerializeField] private float moveSpeed = 3.5f;

    [Header("Click")]
    [Tooltip("누르면 열 팝업 종류입니다.")]
    [SerializeField] private PopupType popupType;

    private RectTransform rectTransform;
    private RefugeesGameUI gameUI;
    private RectTransform motionTarget;
    private Vector2 hiddenPosition;
    private bool hovering;

    private enum PopupType
    {
        News,
        Rule
    }

    private void Awake()
    {
        rectTransform = transform as RectTransform;
        gameUI = FindFirstObjectByType<RefugeesGameUI>();

        if (rectTransform != null)
        {
            motionTarget = visualRoot != null ? visualRoot : rectTransform;
            hiddenPosition = motionTarget.anchoredPosition;
        }

        Button button = GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(OpenPopup);
    }

    private void OnDestroy()
    {
        Button button = GetComponent<Button>();
        if (button != null)
            button.onClick.RemoveListener(OpenPopup);
    }

    private void Update()
    {
        if (motionTarget == null)
            return;

        Vector2 target = hovering ? visibleAnchoredPosition : hiddenPosition;
        motionTarget.anchoredPosition = Vector2.Lerp(
            motionTarget.anchoredPosition,
            target,
            1f - Mathf.Exp(-moveSpeed * Time.unscaledDeltaTime)
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
    }

    private void OpenPopup()
    {
        ResetVisualToHiddenPosition();

        if (gameUI == null)
            gameUI = FindFirstObjectByType<RefugeesGameUI>();

        if (popupType == PopupType.News)
            gameUI?.OpenNewsPopup();
        else
            gameUI?.OpenRulePopup();
    }

    private void ResetVisualToHiddenPosition()
    {
        hovering = false;

        if (motionTarget != null)
            motionTarget.anchoredPosition = hiddenPosition;
    }
}
