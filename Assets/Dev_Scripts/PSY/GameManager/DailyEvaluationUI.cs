using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyEvaluationUI : MonoBehaviour
{
    [Header("Root")]
    [Tooltip("일일 업무평가 화면 전체 패널입니다.")]
    [SerializeField] private GameObject panelRoot;

    [Header("Document Root")]
    [Tooltip("실제 결산 문서 패널입니다. 보통 ResultPanel을 넣습니다.")]
    [SerializeField] private RectTransform documentRoot;

    [Header("Header")]
    [Tooltip("상단 왼쪽 기관/단말 텍스트입니다.")]
    [SerializeField] private TMP_Text topMetaText;
    [Tooltip("상단 오른쪽 DAY 텍스트입니다.")]
    [SerializeField] private TMP_Text dayText;
    [Tooltip("문서 분류 텍스트입니다. 예: 근무평정 · 심사관 근무 결산")]
    [SerializeField] private TMP_Text categoryText;
    [Tooltip("큰 제목 텍스트입니다. 예: 제4일차 업무평가")]
    [SerializeField] private TMP_Text titleText;
    [Tooltip("부제 텍스트입니다. 예: 국경관리국 심사관 근무평정 보고서")]
    [SerializeField] private TMP_Text subtitleText;

    [Header("Daily Metrics")]
    [Tooltip("총 심사 인원 숫자 텍스트입니다.")]
    [SerializeField] private TMP_Text totalValueText;
    [Tooltip("정상 처리 숫자 텍스트입니다.")]
    [SerializeField] private TMP_Text correctValueText;
    [Tooltip("심사 오류 숫자 텍스트입니다.")]
    [SerializeField] private TMP_Text wrongValueText;
    [Tooltip("심사 정확도 숫자 텍스트입니다.")]
    [SerializeField] private TMP_Text accuracyValueText;

    [Header("Score")]
    [Tooltip("보유 성과금 숫자 텍스트입니다.")]
    [SerializeField] private TMP_Text dailyScoreText;
    [Tooltip("평가 도장 텍스트입니다.")]
    [SerializeField] private TMP_Text gradeStampText;

    [Header("Settlement")]
    [Tooltip("판정 성과금 값 텍스트입니다.")]
    [SerializeField] private TMP_Text cumulativeScoreText;
    [Tooltip("총 생활비 값 텍스트입니다.")]
    [SerializeField] private TMP_Text averageAccuracyText;
    [Tooltip("금일 순증감 값 텍스트입니다.")]
    [SerializeField] private TMP_Text cumulativeGradeText;

    [Header("Warning")]
    [Tooltip("규정 위반/정상 근무 안내 박스입니다. 잘못된 선별이 없을 때 숨길 수 있습니다.")]
    [SerializeField] private GameObject warningRoot;
    [Tooltip("기관 평가/주의 문구 본문 텍스트입니다.")]
    [SerializeField] private TMP_Text warningBodyText;

    [Header("Button")]
    [Tooltip("확인 / 다음 날 버튼입니다.")]
    [SerializeField] private Button confirmButton;

    [Header("Motion")]
    [Tooltip("움직이는 값 항목 사이의 간격입니다.")]
    [SerializeField] private float revealInterval = 0.08f;
    [Tooltip("각 값 항목이 올라오며 나타나는 시간입니다.")]
    [SerializeField] private float revealFadeDuration = 0.38f;
    [Tooltip("값 항목이 시작할 때 아래에서 올라오는 거리입니다.")]
    [SerializeField] private float revealOffset = 12f;

    private RefugeesGameManager gameManager;
    private Coroutine revealRoutine;
    private static readonly Color NormalValueColor = new Color(0.82f, 0.79f, 0.66f, 1f);
    private static readonly Color PositiveMoneyColor = new Color(0.78f, 0.9f, 0.88f, 1f);
    private static readonly Color NegativeMoneyColor = new Color(0.66f, 0.22f, 0.18f, 1f);
    private static readonly Color ExcellentStampColor = new Color(0.78f, 0.65f, 0.27f, 1f);
    private static readonly Color GoodStampColor = new Color(0.60f, 0.69f, 0.39f, 1f);
    private static readonly Color FairStampColor = new Color(0.39f, 0.65f, 0.58f, 1f);
    private static readonly Color AverageStampColor = new Color(0.67f, 0.67f, 0.62f, 1f);
    private static readonly Color PoorStampColor = new Color(0.72f, 0.31f, 0.24f, 1f);

    private void Awake()
    {
        if (panelRoot == null)
            panelRoot = gameObject;

        if (documentRoot == null && titleText != null && titleText.transform.parent is RectTransform titleParent)
            documentRoot = titleParent;

        if (documentRoot == null)
            documentRoot = transform as RectTransform;

        ResolveMissingReferences();
        gameManager = RefugeesGameManager.Instance;
    }

    private void OnEnable()
    {
        if (confirmButton != null)
            confirmButton.onClick.AddListener(Confirm);
    }

    private void OnDisable()
    {
        if (confirmButton != null)
            confirmButton.onClick.RemoveListener(Confirm);

        if (revealRoutine != null)
            StopCoroutine(revealRoutine);
    }

    public void Show(RefugeesGameManager manager)
    {
        gameManager = manager;

        if (panelRoot != null)
            panelRoot.SetActive(true);

        ResolveMissingReferences();

        if (gameManager == null || gameManager.GetEvaluation() == null)
            return;

        EvaluationManager evaluation = gameManager.GetEvaluation();
        DailyPerformanceResult result = evaluation.LastDailyResult;
        int accuracy = Mathf.RoundToInt(result.accuracy * 100f);

        SetText(topMetaText, "국경관리국  ·  심사관 단말");
        SetText(dayText, string.Empty);
        SetText(categoryText, $"DAY {gameManager.CurrentDay:00}  ·  성과금 정산");
        SetText(titleText, $"제{gameManager.CurrentDay}일차 근무 결산");
        SetText(subtitleText, "국경관리국 심사관 성과금 정산 보고서");
        SetText(totalValueText, $"{result.inspectedCount}<size=55%>건</size>");
        SetText(correctValueText, $"{result.correctCount}<size=55%>건</size>");
        SetText(wrongValueText, $"{result.wrongCount}<size=55%>건</size>");
        SetText(accuracyValueText, $"{accuracy}<size=55%>%</size>");
        SetText(dailyScoreText, $"{result.ownedPerformanceMoney}");
        SetText(gradeStampText, result.gradeLabel);
        SetText(cumulativeScoreText, FormatSignedMoney(result.judgementPerformanceMoney));
        SetText(averageAccuracyText, FormatSignedMoney(-result.livingCost));
        SetText(cumulativeGradeText, FormatSignedMoney(result.netChange));
        SetText(warningBodyText, result.comment);
        SetColor(totalValueText, NormalValueColor);
        SetColor(correctValueText, NormalValueColor);
        SetColor(wrongValueText, result.wrongCount > 0 ? NegativeMoneyColor : NormalValueColor);
        SetColor(accuracyValueText, NormalValueColor);
        SetColor(dailyScoreText, NormalValueColor);
        SetColor(cumulativeScoreText, GetMoneyColor(result.judgementPerformanceMoney));
        SetColor(averageAccuracyText, GetMoneyColor(-result.livingCost));
        SetColor(cumulativeGradeText, GetMoneyColor(result.netChange));
        SetStampColor(result.gradeLabel);

        if (warningRoot != null)
            warningRoot.SetActive(result.wrongCount > 0);

        SetStampRotation();

        PlayRevealMotion();
    }

    public void Hide()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    private void Confirm()
    {
        gameManager?.NextDay();
    }

    private void PlayRevealMotion()
    {
        if (revealRoutine != null)
            StopCoroutine(revealRoutine);

        revealRoutine = StartCoroutine(RevealSequentially());
    }

    private IEnumerator RevealSequentially()
    {
        List<RectTransform> revealRoots = GetRevealRoots();

        if (confirmButton != null)
            confirmButton.interactable = false;

        for (int i = 0; i < revealRoots.Count; i++)
        {
            yield return RevealRoot(revealRoots[i]);
            yield return new WaitForSecondsRealtime(revealInterval);
        }

        if (confirmButton != null)
            confirmButton.interactable = true;

        revealRoutine = null;
    }

    private IEnumerator RevealRoot(RectTransform root)
    {
        if (root == null)
            yield break;

        CanvasGroup canvasGroup = root.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = root.gameObject.AddComponent<CanvasGroup>();

        Vector2 endPosition = root.anchoredPosition;
        Vector2 startPosition = endPosition + Vector2.down * revealOffset;
        canvasGroup.alpha = 0f;
        root.anchoredPosition = startPosition;

        float elapsed = 0f;

        while (elapsed < revealFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, revealFadeDuration));
            float eased = Mathf.SmoothStep(0f, 1f, t);
            canvasGroup.alpha = eased;
            root.anchoredPosition = Vector2.LerpUnclamped(startPosition, endPosition, eased);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        root.anchoredPosition = endPosition;
    }

    private List<RectTransform> GetRevealRoots()
    {
        var roots = new List<RectTransform>
        {
            totalValueText != null ? totalValueText.rectTransform : null,
            correctValueText != null ? correctValueText.rectTransform : null,
            wrongValueText != null ? wrongValueText.rectTransform : null,
            accuracyValueText != null ? accuracyValueText.rectTransform : null,
            dailyScoreText != null ? dailyScoreText.rectTransform : null,
            gradeStampText != null ? gradeStampText.rectTransform.parent as RectTransform : null,
            cumulativeScoreText != null ? cumulativeScoreText.rectTransform : null,
            averageAccuracyText != null ? averageAccuracyText.rectTransform : null,
            cumulativeGradeText != null ? cumulativeGradeText.rectTransform : null,
            confirmButton != null ? confirmButton.transform as RectTransform : null
        };

        roots.RemoveAll(root => root == null);
        return roots;
    }

    private void ResolveMissingReferences()
    {
        if (documentRoot == null && titleText != null && titleText.transform.parent is RectTransform titleParent)
            documentRoot = titleParent;

        if (documentRoot == null)
            documentRoot = transform as RectTransform;

        if (topMetaText == null) topMetaText = FindText("ReportTopMetaText");
        if (dayText == null) dayText = FindText("ReportDayText");
        if (categoryText == null) categoryText = FindText("ReportCategoryText");
        if (titleText == null) titleText = FindText("ReportMainTitleText");
        if (subtitleText == null) subtitleText = FindText("ReportSubtitleText");
        if (totalValueText == null) totalValueText = FindText("MetricTotalValue");
        if (correctValueText == null) correctValueText = FindText("MetricCorrectValue");
        if (wrongValueText == null) wrongValueText = FindText("MetricWrongValue");
        if (accuracyValueText == null) accuracyValueText = FindText("MetricAccuracyValue");
        if (dailyScoreText == null) dailyScoreText = FindText("DailyScoreValue");
        if (gradeStampText == null) gradeStampText = FindText("GradeStampText");
        if (cumulativeScoreText == null) cumulativeScoreText = FindText("CumulativeScoreValue");
        if (averageAccuracyText == null) averageAccuracyText = FindText("AverageAccuracyValue");
        if (cumulativeGradeText == null) cumulativeGradeText = FindText("CumulativeGradeValue");
        if (warningBodyText == null) warningBodyText = FindText("WarningBodyText");
        if (warningRoot == null && warningBodyText != null) warningRoot = warningBodyText.transform.parent.gameObject;
    }

    private TMP_Text FindText(string childName)
    {
        return documentRoot != null ? documentRoot.Find(childName)?.GetComponent<TMP_Text>() : null;
    }

    private void SetStampRotation()
    {
        SetChildRotation("GradeStampBox", 6f);
        SetChildRotation("GradeStampBox_Top", 6f);
        SetChildRotation("GradeStampBox_Bottom", 6f);
        SetChildRotation("GradeStampBox_Left", 6f);
        SetChildRotation("GradeStampBox_Right", 6f);

        if (gradeStampText != null)
            gradeStampText.rectTransform.localEulerAngles = new Vector3(0f, 0f, 6f);
    }

    private void SetChildRotation(string childName, float angle)
    {
        RectTransform rectTransform = documentRoot != null ? documentRoot.Find(childName) as RectTransform : null;

        if (rectTransform != null)
            rectTransform.localEulerAngles = new Vector3(0f, 0f, angle);
    }

    private void SetStampColor(string gradeLabel)
    {
        Color stampColor = gradeLabel switch
        {
            "탁월" => ExcellentStampColor,
            "우수" => GoodStampColor,
            "양호" => FairStampColor,
            "보통" => AverageStampColor,
            _ => PoorStampColor
        };

        RectTransform stampRoot = gradeStampText != null
            ? gradeStampText.rectTransform.parent as RectTransform
            : null;

        if (stampRoot == null)
            return;

        Image colorBackground = stampRoot.Find("ColorImage")?.GetComponent<Image>();

        if (colorBackground != null)
        {
            colorBackground.transform.SetAsFirstSibling();
            colorBackground.color = new Color(stampColor.r, stampColor.g, stampColor.b, 0.16f);
        }

        if (gradeStampText != null)
        {
            gradeStampText.transform.SetAsLastSibling();
            gradeStampText.color = stampColor;
        }
    }

    private static void SetText(TMP_Text text, string value)
    {
        if (text != null)
            text.text = value;
    }

    private static void SetColor(TMP_Text text, Color color)
    {
        if (text != null)
            text.color = color;
    }

    private static Color GetMoneyColor(int value)
    {
        return value < 0 ? NegativeMoneyColor : PositiveMoneyColor;
    }

    private static string FormatSignedMoney(int value)
    {
        return value > 0 ? $"+{value}" : value.ToString();
    }
}
