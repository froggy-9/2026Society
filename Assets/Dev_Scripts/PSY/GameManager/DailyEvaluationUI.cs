using System.Collections;
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
    [Tooltip("금일 성과점수 숫자 텍스트입니다.")]
    [SerializeField] private TMP_Text dailyScoreText;
    [Tooltip("평가 도장 텍스트입니다.")]
    [SerializeField] private TMP_Text gradeStampText;

    [Header("Cumulative")]
    [Tooltip("누적 성과점수 값 텍스트입니다.")]
    [SerializeField] private TMP_Text cumulativeScoreText;
    [Tooltip("평균 정확도 값 텍스트입니다.")]
    [SerializeField] private TMP_Text averageAccuracyText;
    [Tooltip("종합평가 값 텍스트입니다.")]
    [SerializeField] private TMP_Text cumulativeGradeText;
    [Tooltip("누적 정확도 진행 바 Fill 이미지입니다.")]
    [SerializeField] private Image cumulativeProgressFill;

    [Header("Warning")]
    [Tooltip("기관 평가/주의 문구 본문 텍스트입니다.")]
    [SerializeField] private TMP_Text warningBodyText;

    [Header("Button")]
    [Tooltip("확인 / 다음 날 버튼입니다.")]
    [SerializeField] private Button confirmButton;

    [Header("Motion")]
    [Tooltip("결산 문서가 켜질 때 항목들이 순서대로 나타나는 시간 간격입니다.")]
    [SerializeField] private float revealInterval = 0.07f;
    [Tooltip("각 항목 페이드 시간입니다.")]
    [SerializeField] private float revealFadeDuration = 0.45f;

    private RefugeesGameManager gameManager;
    private Coroutine revealRoutine;
    private Graphic[] revealGraphics;
    private float[] revealTargetAlphas;

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
        SetText(dayText, $"DAY {gameManager.CurrentDay:00}");
        SetText(categoryText, "근무평정  ·  심사관 근무 결산");
        SetText(titleText, $"제{gameManager.CurrentDay}일차 업무평가");
        SetText(subtitleText, "국경관리국 심사관 근무평정 보고서");
        SetText(totalValueText, result.inspectedCount.ToString());
        SetText(correctValueText, result.correctCount.ToString());
        SetText(wrongValueText, result.wrongCount.ToString());
        SetText(accuracyValueText, accuracy.ToString());
        SetText(dailyScoreText, result.performanceScore.ToString());
        SetText(gradeStampText, $"평가 {result.gradeLabel}");
        SetText(cumulativeScoreText, $"{evaluation.CumulativePerformanceScore} / {evaluation.MaxCumulativePerformanceScore}");
        SetText(averageAccuracyText, $"{evaluation.CumulativeAccuracy:P1}");
        SetText(cumulativeGradeText, evaluation.CumulativeGradeLabel);
        SetText(warningBodyText, result.comment);
        SetStampRotation();

        if (cumulativeProgressFill != null)
        {
            cumulativeProgressFill.type = Image.Type.Filled;
            cumulativeProgressFill.fillMethod = Image.FillMethod.Horizontal;
            cumulativeProgressFill.fillOrigin = 0;
            cumulativeProgressFill.fillAmount = Mathf.Clamp01(evaluation.CumulativeAccuracy);
        }

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
        if (documentRoot == null)
            return;

        revealGraphics = documentRoot.GetComponentsInChildren<Graphic>(true);
        revealTargetAlphas = new float[revealGraphics.Length];

        if (revealRoutine != null)
            StopCoroutine(revealRoutine);

        revealRoutine = StartCoroutine(RevealSequentially());
    }

    private IEnumerator RevealSequentially()
    {
        for (int i = 0; i < revealGraphics.Length; i++)
        {
            Graphic graphic = revealGraphics[i];

            if (graphic == null || graphic.transform == documentRoot)
                continue;

            Color color = graphic.color;
            revealTargetAlphas[i] = color.a;
            color.a = 0f;
            graphic.color = color;
        }

        for (int i = 0; i < revealGraphics.Length; i++)
        {
            Graphic graphic = revealGraphics[i];

            if (graphic == null || graphic.transform == documentRoot)
                continue;

            float elapsed = 0f;
            Color startColor = graphic.color;
            Color endColor = startColor;
            endColor.a = revealTargetAlphas[i];
            Vector3 endPosition = graphic.transform.localPosition;
            Vector3 startPosition = endPosition + new Vector3(0f, -8f, 0f);
            graphic.transform.localPosition = startPosition;

            while (elapsed < revealFadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, revealFadeDuration));
                float eased = Mathf.SmoothStep(0f, 1f, t);
                graphic.color = Color.Lerp(startColor, endColor, eased);
                graphic.transform.localPosition = Vector3.LerpUnclamped(startPosition, endPosition, eased);
                yield return null;
            }

            graphic.color = endColor;
            graphic.transform.localPosition = endPosition;
            yield return new WaitForSecondsRealtime(revealInterval);
        }

        revealRoutine = null;
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
        if (cumulativeProgressFill == null) cumulativeProgressFill = FindImage("ProgressFill");
    }

    private TMP_Text FindText(string childName)
    {
        return documentRoot != null ? documentRoot.Find(childName)?.GetComponent<TMP_Text>() : null;
    }

    private Image FindImage(string childName)
    {
        return documentRoot != null ? documentRoot.Find(childName)?.GetComponent<Image>() : null;
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

    private static void SetText(TMP_Text text, string value)
    {
        if (text != null)
            text.text = value;
    }
}
