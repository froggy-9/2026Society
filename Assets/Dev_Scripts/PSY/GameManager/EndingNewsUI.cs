using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndingNewsUI : MonoBehaviour
{
    [Header("Root")]
    [Tooltip("엔딩 뉴스 전체 패널입니다. 검은 배경까지 포함한 최상단 UI를 넣습니다.")]
    [SerializeField] private GameObject panelRoot;

    [Tooltip("실제로 아래에서 올라올 신문/문서 RectTransform입니다.")]
    [SerializeField] private RectTransform newspaperRoot;

    [Header("Text Slots")]
    [Tooltip("엔딩 신문 상단 날짜 텍스트 칸입니다.")]
    [SerializeField] private TMP_Text dateText;

    [Tooltip("엔딩 신문 상단 일차/최종 표기 텍스트 칸입니다.")]
    [SerializeField] private TMP_Text dayText;

    [Tooltip("엔딩 신문 기자/발행처 텍스트 칸입니다.")]
    [SerializeField] private TMP_Text reporterText;

    [Tooltip("엔딩 신문 분류/표지 텍스트 칸입니다.")]
    [SerializeField] private TMP_Text articleLabelText;

    [Tooltip("기존 데이터 호환용 작은 상단 텍스트입니다. 새 UI에서는 Article Label Text와 같은 값을 넣어도 됩니다.")]
    [SerializeField] private TMP_Text metaText;

    [Tooltip("엔딩 뉴스 헤드라인 텍스트입니다.")]
    [SerializeField] private TMP_Text headlineText;

    [Tooltip("엔딩 뉴스 본문 텍스트입니다.")]
    [SerializeField] private TMP_Text bodyText;

    [Header("Image Slots")]
    [Tooltip("엔딩 뉴스 이미지가 들어갈 칸들입니다. Ending News의 images 순서대로 채워집니다.")]
    [SerializeField] private Image[] imageSlots;

    [Header("Newspaper Scroll")]
    [Tooltip("엔딩 신문 본문 Scroll View의 ScrollRect입니다.")]
    [SerializeField] private ScrollRect newspaperScrollRect;

    [Tooltip("엔딩 신문 본문 Scroll View 안의 Content RectTransform입니다.")]
    [SerializeField] private RectTransform newspaperContent;

    [Tooltip("Content가 Viewport보다 작을 때 유지할 최소 높이입니다. 0이면 Viewport 높이를 기준으로 합니다.")]
    [SerializeField] private float minimumContentHeight;

    [Tooltip("본문/이미지 아래에 남길 여백입니다.")]
    [SerializeField] private float scrollBottomPadding = 48f;

    [Header("Title Button")]
    [Tooltip("누르면 타이틀 화면으로 돌아갈 버튼입니다.")]
    [SerializeField] private Button titleButton;

    [Tooltip("타이틀 씬 이름입니다. Build Settings에 등록된 씬 이름과 같아야 합니다.")]
    [SerializeField] private string titleSceneName = "TitleScene";

    [Header("Motion")]
    [Tooltip("등장 시작 위치 오프셋입니다.")]
    [SerializeField] private Vector2 startOffset = new Vector2(0f, -180f);

    [Tooltip("엔딩 뉴스 등장 시간입니다.")]
    [SerializeField] private float openDuration = 1.05f;

    private Coroutine motionRoutine;
    private Vector2 basePosition;
    private bool hasBasePosition;
    private bool isShowing;
    private readonly System.Collections.Generic.Dictionary<RectTransform, float> textBaseHeights = new System.Collections.Generic.Dictionary<RectTransform, float>();

    private void Awake()
    {
        if (panelRoot == null)
            panelRoot = gameObject;

        if (newspaperRoot == null)
            newspaperRoot = transform as RectTransform;

        if (!isShowing)
            Hide();
    }

    private void OnEnable()
    {
        if (titleButton != null)
            titleButton.onClick.AddListener(GoToTitle);
    }

    private void OnDisable()
    {
        if (titleButton != null)
            titleButton.onClick.RemoveListener(GoToTitle);
    }

    public void Show(RefugeesEndingType endingType, EndingNewsContent content)
    {
        // Set before activation: the first activation invokes Awake synchronously.
        isShowing = true;

        if (panelRoot != null)
            panelRoot.SetActive(true);

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        if (newspaperRoot != null && !hasBasePosition)
        {
            basePosition = newspaperRoot.anchoredPosition;
            hasBasePosition = true;
        }

        string labelText = content != null && !string.IsNullOrWhiteSpace(content.GetArticleLabelText())
            ? content.GetArticleLabelText()
            : GetMetaText(endingType);

        SetText(dateText, content != null ? content.dateText : string.Empty);
        SetText(dayText, content != null ? content.dayText : string.Empty);
        SetText(reporterText, content != null ? content.reporterText : string.Empty);
        SetText(articleLabelText, labelText);
        SetText(metaText, labelText);
        SetText(headlineText, content != null ? content.headline : GetFallbackHeadline(endingType));
        SetText(bodyText, content != null ? content.body : string.Empty);
        SetImages(content != null ? content.images : null);
        RefreshNewspaperScroll();

        if (motionRoutine != null)
            StopCoroutine(motionRoutine);

        motionRoutine = StartCoroutine(PlayOpenMotion());
    }

    public void Hide()
    {
        isShowing = false;

        if (motionRoutine != null)
        {
            StopCoroutine(motionRoutine);
            motionRoutine = null;
        }

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    public void GoToTitle()
    {
        if (string.IsNullOrWhiteSpace(titleSceneName))
            return;

        Time.timeScale = 1f;
        SceneManager.LoadScene(titleSceneName);
    }

    private IEnumerator PlayOpenMotion()
    {
        if (newspaperRoot == null)
            yield break;

        CanvasGroup canvasGroup = newspaperRoot.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = newspaperRoot.gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;

        Vector2 startPosition = basePosition + startOffset;
        newspaperRoot.anchoredPosition = startPosition;

        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, openDuration);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = Mathf.SmoothStep(0f, 1f, t);

            newspaperRoot.anchoredPosition = Vector2.LerpUnclamped(startPosition, basePosition, eased);

            canvasGroup.alpha = eased;

            yield return null;
        }

        newspaperRoot.anchoredPosition = basePosition;

        canvasGroup.alpha = 1f;

        motionRoutine = null;
    }

    private void SetImages(Sprite[] images)
    {
        if (imageSlots == null)
            return;

        for (int i = 0; i < imageSlots.Length; i++)
        {
            Image slot = imageSlots[i];

            if (slot == null)
                continue;

            Sprite sprite = images != null && i < images.Length ? images[i] : null;
            slot.sprite = sprite;
            slot.enabled = sprite != null;
            slot.preserveAspect = true;
        }
    }

    private void RefreshNewspaperScroll()
    {
        if (newspaperScrollRect == null && newspaperContent == null)
            ResolveScrollFromBodyText();

        RectTransform content = newspaperContent;

        if (content == null)
            return;

        Canvas.ForceUpdateCanvases();
        ResizeTextToPreferredHeight(bodyText);
        Canvas.ForceUpdateCanvases();

        RectTransform viewport = newspaperScrollRect != null ? newspaperScrollRect.viewport : null;
        float viewportHeight = viewport != null ? viewport.rect.height : 0f;
        float requiredHeight = Mathf.Max(minimumContentHeight, viewportHeight);

        for (int i = 0; i < content.childCount; i++)
        {
            RectTransform child = content.GetChild(i) as RectTransform;

            if (child == null || !child.gameObject.activeSelf)
                continue;

            float bottom = GetBottomDistanceFromContentTop(content, child);
            requiredHeight = Mathf.Max(requiredHeight, bottom + scrollBottomPadding);
        }

        Vector2 size = content.sizeDelta;
        size.y = requiredHeight;
        content.sizeDelta = size;
        content.anchoredPosition = new Vector2(content.anchoredPosition.x, 0f);

        if (newspaperScrollRect != null)
            newspaperScrollRect.verticalNormalizedPosition = 1f;
    }

    private void ResolveScrollFromBodyText()
    {
        if (bodyText == null)
            return;

        newspaperScrollRect = bodyText.GetComponentInParent<ScrollRect>();
        if (newspaperScrollRect != null)
            newspaperContent = newspaperScrollRect.content;
    }

    private void ResizeTextToPreferredHeight(TMP_Text text)
    {
        if (text == null)
            return;

        RectTransform rectTransform = text.rectTransform;

        if (!textBaseHeights.ContainsKey(rectTransform))
            textBaseHeights.Add(rectTransform, rectTransform.sizeDelta.y);

        float baseHeight = textBaseHeights[rectTransform];
        float preferredHeight = text.GetPreferredValues(text.text, rectTransform.rect.width, 0f).y;
        Vector2 size = rectTransform.sizeDelta;
        size.y = Mathf.Max(baseHeight, preferredHeight);
        rectTransform.sizeDelta = size;
        text.ForceMeshUpdate();
    }

    private static float GetBottomDistanceFromContentTop(RectTransform content, RectTransform child)
    {
        Vector3[] childCorners = new Vector3[4];
        child.GetWorldCorners(childCorners);

        float minY = float.MaxValue;

        for (int i = 0; i < childCorners.Length; i++)
        {
            Vector3 localCorner = content.InverseTransformPoint(childCorners[i]);
            minY = Mathf.Min(minY, localCorner.y);
        }

        return content.rect.yMax - minY;
    }

    private static string GetMetaText(RefugeesEndingType endingType)
    {
        switch (endingType)
        {
            case RefugeesEndingType.Preservation:
                return "최종 보도 · 존손";

            case RefugeesEndingType.FollowUpCare:
                return "최종 보도 · 사후관리";

            case RefugeesEndingType.Closure:
                return "최종 보도 · 폐쇄조치";

            default:
                return "최종 보도";
        }
    }

    private static string GetFallbackHeadline(RefugeesEndingType endingType)
    {
        switch (endingType)
        {
            case RefugeesEndingType.Preservation:
                return "국경 심사 체계 존손 결정";

            case RefugeesEndingType.FollowUpCare:
                return "난민 수용 사후관리 체계 가동";

            case RefugeesEndingType.Closure:
                return "국경 관리소 폐쇄조치 발표";

            default:
                return "국경관리국 최종 보도";
        }
    }

    private static void SetText(TMP_Text text, string value)
    {
        if (text != null)
            text.text = value;
    }
}
