using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RefugeesGameUI : MonoBehaviour
{
    [Header("Panels")]
    [Tooltip("뉴스 화면 전체 패널입니다. 뉴스/규칙 UI가 들어있는 Canvas 또는 Panel을 넣습니다.")]
    [SerializeField] private GameObject newsPanel;

    [Tooltip("심사 화면 전체 패널입니다.")]
    [SerializeField] private GameObject inspectionPanel;

    [Tooltip("하루 결과 화면 전체 패널입니다.")]
    [SerializeField] private GameObject resultPanel;

    [Tooltip("게임오버 화면 전체 패널입니다.")]
    [SerializeField] private GameObject gameOverPanel;

    [Header("News UI")]
    [Tooltip("뉴스 제목/본문/이미지 슬롯을 제어하는 NewsViewUI입니다.")]
    [SerializeField] private NewsViewUI newsView;

    [Tooltip("근무 규칙 제목/본문을 제어하는 RuleListUI입니다.")]
    [SerializeField] private RuleListUI ruleListView;

    [Tooltip("뉴스 화면에서 심사 화면으로 넘어가는 버튼입니다. Enter 키도 지원합니다.")]
    [SerializeField] private Button newsContinueButton;

    [Header("Inspection HUD")]
    [Tooltip("현재 일차를 표시할 TMP 텍스트입니다.")]
    [SerializeField] private TMP_Text dayText;

    [Tooltip("남은 제한시간을 표시할 TMP 텍스트입니다.")]
    [SerializeField] private TMP_Text timerText;

    [Tooltip("현재 심사 수 / 목표 심사 수를 표시할 TMP 텍스트입니다.")]
    [SerializeField] private TMP_Text quotaText;

    [Tooltip("현재 점수를 표시할 TMP 텍스트입니다.")]
    [SerializeField] private TMP_Text scoreText;

    [Header("Result")]
    [Tooltip("하루 결과 요약을 표시할 TMP 텍스트입니다.")]
    [SerializeField] private TMP_Text resultSummaryText;

    [Tooltip("다음 날로 넘어가는 버튼입니다.")]
    [SerializeField] private Button nextDayButton;

    [Tooltip("게임을 처음부터 다시 시작하는 버튼입니다.")]
    [SerializeField] private Button restartButton;

    private RefugeesGameManager gameManager;

    private void Awake()
    {
        ResolveSceneReferences();
        RegisterGameManager();
    }

    private void OnEnable()
    {
        ResolveSceneReferences();
        RegisterGameManager();

        if (gameManager != null)
        {
            gameManager.StateChanged -= RefreshState;
            gameManager.StateChanged += RefreshState;
        }

        if (newsContinueButton != null)
            newsContinueButton.onClick.AddListener(ContinueFromNews);

        if (newsView != null)
            newsView.ContinueRequested += ContinueFromNews;

        if (nextDayButton != null)
            nextDayButton.onClick.AddListener(NextDay);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
    }

    private void OnDisable()
    {
        if (gameManager != null)
            gameManager.StateChanged -= RefreshState;

        if (newsContinueButton != null)
            newsContinueButton.onClick.RemoveListener(ContinueFromNews);

        if (newsView != null)
            newsView.ContinueRequested -= ContinueFromNews;

        if (nextDayButton != null)
            nextDayButton.onClick.RemoveListener(NextDay);

        if (restartButton != null)
            restartButton.onClick.RemoveListener(RestartGame);
    }

    private void Start()
    {
        ResolveSceneReferences();
        RegisterGameManager();

        if (gameManager != null && gameManager.CurrentState == GameState.None)
            gameManager.GameStart();

        RefreshAll();
    }

    private void Update()
    {
        if (gameManager == null)
            return;

        if (gameManager.CurrentState == GameState.News && WasContinueKeyPressed())
            ContinueFromNews();

        RefreshHud();
    }

    public void ContinueFromNews()
    {
        gameManager?.StartInspection();
        PleaResultLog.Clear();
    }

    public void NextDay()
    {
        gameManager?.NextDay();
    }

    public void RestartGame()
    {
        gameManager?.RestartGame();
    }

    private void RefreshState(GameState state)
    {
        SetActive(newsPanel, state == GameState.News);
        SetActive(inspectionPanel, state == GameState.Inspection);
        SetActive(resultPanel, state == GameState.Result);
        SetActive(gameOverPanel, state == GameState.GameOver);

        RefreshAll();
    }

    private void RefreshAll()
    {
        RefreshNews();
        RefreshHud();
        RefreshResult();
    }

    private void RefreshNews()
    {
        if (gameManager == null)
            return;

        newsView?.Show(gameManager.GetCurrentNews(), PleaResultLog.PendingNews);
        ruleListView?.Show(gameManager.GetCurrentRules(), gameManager.GetCurrentRuleDescription());
    }

    private void RefreshHud()
    {
        if (gameManager == null)
            return;

        EvaluationManager evaluation = gameManager.GetEvaluation();
        DayDataSO dayData = gameManager.GetCurrentDayData();

        SetText(dayText, $"DAY {gameManager.CurrentDay}");
        SetText(timerText, FormatTime(gameManager.RemainingTime));
        SetText(scoreText, evaluation != null ? evaluation.TotalScore.ToString() : "0");

        if (dayData != null)
            SetText(quotaText, $"{gameManager.InspectedNpcCount} / {dayData.targetInspectionCount}");
    }

    private void RefreshResult()
    {
        if (gameManager == null)
            return;

        EvaluationManager evaluation = gameManager.GetEvaluation();

        if (evaluation == null)
            return;

        string summary =
            $"Day {gameManager.CurrentDay} 종료\n\n" +
            $"올바르게 선별한 난민 수: {evaluation.CorrectApprovedCount}\n" +
            $"올바르게 거절한 난민 수: {evaluation.CorrectDeniedCount}\n\n" +
            $"잘못 선별한 난민 수: {evaluation.WrongApprovedCount}\n" +
            $"잘못 거절한 난민 수: {evaluation.WrongDeniedCount}\n\n" +
            $"정확도: {evaluation.Accuracy:P0}\n" +
            $"총점: {evaluation.TotalScore}";

        SetText(resultSummaryText, summary);
    }

    private static string FormatTime(float seconds)
    {
        int totalSeconds = Mathf.CeilToInt(seconds);
        int minutes = totalSeconds / 60;
        int remain = totalSeconds % 60;
        return $"{minutes:00}:{remain:00}";
    }

    private static void SetActive(GameObject target, bool active)
    {
        if (target != null)
            target.SetActive(active);
    }

    private static void SetText(TMP_Text text, string value)
    {
        if (text != null)
            text.text = value;
    }

    private static bool WasContinueKeyPressed()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return false;

        return keyboard.enterKey.wasPressedThisFrame
            || keyboard.numpadEnterKey.wasPressedThisFrame;
    }

    private void RegisterGameManager()
    {
        RefugeesGameManager nextManager = RefugeesGameManager.Instance;

        if (nextManager == null || gameManager == nextManager)
            return;

        if (gameManager != null)
            gameManager.StateChanged -= RefreshState;

        gameManager = nextManager;
        gameManager.StateChanged += RefreshState;
    }

    private void ResolveSceneReferences()
    {
        if (newsView == null)
            newsView = GetComponent<NewsViewUI>();

        if (ruleListView == null)
            ruleListView = GetComponent<RuleListUI>();
    }
}
