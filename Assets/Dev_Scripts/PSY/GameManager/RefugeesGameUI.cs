using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RefugeesGameUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject newsPanel;
    [SerializeField] private GameObject inspectionPanel;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private GameObject gameOverPanel;

    [Header("News UI")]
    [SerializeField] private NewsViewUI newsView;
    [SerializeField] private RuleListUI ruleListView;
    [SerializeField] private Button newsContinueButton;

    [Header("Inspection HUD")]
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text quotaText;
    [SerializeField] private TMP_Text scoreText;

    [Header("Result")]
    [SerializeField] private TMP_Text resultSummaryText;
    [SerializeField] private Button nextDayButton;
    [SerializeField] private Button restartButton;

    private RefugeesGameManager gameManager;

    private void Awake()
    {
        RegisterGameManager();
    }

    private void OnEnable()
    {
        RegisterGameManager();

        if (gameManager != null)
        {
            gameManager.StateChanged -= RefreshState;
            gameManager.StateChanged += RefreshState;
        }

        if (newsContinueButton != null)
            newsContinueButton.onClick.AddListener(ContinueFromNews);

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

        if (nextDayButton != null)
            nextDayButton.onClick.RemoveListener(NextDay);

        if (restartButton != null)
            restartButton.onClick.RemoveListener(RestartGame);
    }

    private void Start()
    {
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
}
