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
    [Tooltip("엔딩 이름 또는 DAY END 같은 작은 상단 텍스트입니다.")]
    [SerializeField] private TMP_Text metaText;

    [Tooltip("엔딩 뉴스 헤드라인 텍스트입니다.")]
    [SerializeField] private TMP_Text headlineText;

    [Tooltip("엔딩 뉴스 본문 텍스트입니다.")]
    [SerializeField] private TMP_Text bodyText;

    [Header("Image Slots")]
    [Tooltip("엔딩 뉴스 이미지가 들어갈 칸들입니다. Ending News의 images 순서대로 채워집니다.")]
    [SerializeField] private Image[] imageSlots;

    [Header("Title Button")]
    [Tooltip("누르면 타이틀 화면으로 돌아갈 버튼입니다.")]
    [SerializeField] private Button titleButton;

    [Tooltip("타이틀 씬 이름입니다. Build Settings에 등록된 씬 이름과 같아야 합니다.")]
    [SerializeField] private string titleSceneName = "TitleScene";

    [Header("Motion")]
    [Tooltip("등장 시작 위치 오프셋입니다.")]
    [SerializeField] private Vector2 startOffset = new Vector2(0f, -180f);

    [Tooltip("엔딩 뉴스 등장 시간입니다.")]
    [SerializeField] private float openDuration = 0.85f;

    private Coroutine motionRoutine;
    private Vector2 basePosition;
    private bool hasBasePosition;

    private void Awake()
    {
        if (panelRoot == null)
            panelRoot = gameObject;

        if (newspaperRoot == null)
            newspaperRoot = transform as RectTransform;

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
        if (panelRoot != null)
            panelRoot.SetActive(true);

        if (newspaperRoot != null && !hasBasePosition)
        {
            basePosition = newspaperRoot.anchoredPosition;
            hasBasePosition = true;
        }

        SetText(metaText, content != null && !string.IsNullOrWhiteSpace(content.metaText)
            ? content.metaText
            : GetMetaText(endingType));
        SetText(headlineText, content != null ? content.headline : GetFallbackHeadline(endingType));
        SetText(bodyText, content != null ? content.body : string.Empty);
        SetImages(content != null ? content.images : null);

        if (motionRoutine != null)
            StopCoroutine(motionRoutine);

        motionRoutine = StartCoroutine(PlayOpenMotion());
    }

    public void Hide()
    {
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

        CanvasGroup canvasGroup = panelRoot != null ? panelRoot.GetComponent<CanvasGroup>() : null;

        if (canvasGroup == null && panelRoot != null)
            canvasGroup = panelRoot.AddComponent<CanvasGroup>();

        if (canvasGroup != null)
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

            if (canvasGroup != null)
                canvasGroup.alpha = eased;

            yield return null;
        }

        newspaperRoot.anchoredPosition = basePosition;

        if (canvasGroup != null)
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
