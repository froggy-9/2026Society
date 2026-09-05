using UnityEngine;

public class RefugeesGameManager : MonoBehaviour
{
    public static RefugeesGameManager Instance;

    public event System.Action<GameState> StateChanged;
    public event System.Action<int> DayStarted;
    public event System.Action JudgementSubmitted;

    [Header("Managers")]
    [SerializeField] private DayManager dayManager;
    [SerializeField] private EvaluationManager evaluationManager;

    [Header("Game")]
    [SerializeField] private int maxDay = 4;

    public int CurrentDay { get; private set; }

    public GameState CurrentState { get; private set; } = GameState.None;

    public float RemainingTime { get; private set; }

    public int InspectedNpcCount { get; private set; }

    private GameState previousState;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        if (dayManager == null)
            dayManager = FindFirstObjectByType<DayManager>();

        if (evaluationManager == null)
            evaluationManager = FindFirstObjectByType<EvaluationManager>();
    }

    private void Update()
    {
        if (CurrentState != GameState.Inspection)
            return;

        RemainingTime -= Time.deltaTime;

        if (RemainingTime <= 0f)
        {
            RemainingTime = 0f;
            EndDay();
        }
    }

    public void GameStart()
    {
        PleaResultLog.Clear();

        int slot = SaveCardMenu.GetSelectedSlot();
        bool canContinue = !SaveCardMenu.ShouldStartNewGame() && SaveCardMenu.HasSave(slot);

        if (canContinue)
        {
            evaluationManager?.LoadGame(
                SaveCardMenu.GetSavedScore(slot),
                SaveCardMenu.GetSavedCorrectCount(slot),
                SaveCardMenu.GetSavedWrongAcceptCount(slot),
                SaveCardMenu.GetSavedWrongRejectCount(slot),
                SaveCardMenu.GetSavedCumulativePerformanceScore(slot),
                SaveCardMenu.GetSavedMaxCumulativePerformanceScore(slot)
            );
            CurrentDay = SaveCardMenu.GetSavedDay(slot);
        }
        else
        {
            evaluationManager?.ResetGame();
            CurrentDay = 1;
        }

        StartDay();
    }

    private void StartDay()
    {
        if (dayManager == null)
        {
            GoToGameOver();
            return;
        }

        if (!dayManager.LoadDay(CurrentDay))
        {
            GoToGameOver();
            return;
        }

        evaluationManager?.ResetDay();

        InspectedNpcCount = 0;
        RemainingTime = dayManager.DayTime;

        SetState(GameState.DayIntro);
        DayStarted?.Invoke(CurrentDay);
    }

    public void BeginDayNews()
    {
        if (CurrentState != GameState.DayIntro)
            return;

        SetState(GameState.News);
    }

    public void StartInspection()
    {
        if (CurrentState != GameState.News)
            return;

        SetState(GameState.Inspection);
    }

    public void SubmitJudgement(bool playerApproved, bool npcShouldBeApproved, NPCData npc = null, string reason = "")
    {
        if (CurrentState != GameState.Inspection)
            return;

        if (evaluationManager == null)
            return;

        InspectedNpcCount++;

        bool hardPenalty = CurrentDay == 4;

        evaluationManager.SubmitJudgement(
            playerApproved,
            npcShouldBeApproved,
            hardPenalty,
            npc,
            reason
        );

        JudgementSubmitted?.Invoke();
    }

    public void EndDay()
    {
        if (CurrentState != GameState.Inspection)
            return;

        if (evaluationManager == null || dayManager == null)
        {
            GoToGameOver();
            return;
        }

        evaluationManager.CalculateResult(
            InspectedNpcCount,
            dayManager.Quota
        );

        SaveCardMenu.SaveProgress(
            CurrentDay,
            evaluationManager.TotalScore,
            evaluationManager.CumulativeCorrectCount,
            evaluationManager.CumulativeWrongAcceptCount,
            evaluationManager.CumulativeWrongRejectCount,
            evaluationManager.CumulativePerformanceScore,
            evaluationManager.MaxCumulativePerformanceScore
        );

        if (evaluationManager.IsGameOver())
        {
            GoToGameOver();
            return;
        }

        SetState(GameState.Result);
    }

    public void NextDay()
    {
        if (CurrentState != GameState.Result)
            return;

        if (CurrentDay >= maxDay)
        {
            ShowEnding();
            return;
        }

        CurrentDay++;

        StartDay();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        GameStart();
    }

    public void PauseGame()
    {
        if (CurrentState == GameState.Pause)
        {
            SetState(previousState);
            Time.timeScale = 1f;
            return;
        }

        previousState = CurrentState;
        SetState(GameState.Pause);
        Time.timeScale = 0f;
    }

    public bool CanSpawnNpc()
    {
        return CurrentState == GameState.Inspection;
    }

    public NewsSO GetCurrentNews()
    {
        return dayManager != null ? dayManager.CurrentNews : null;
    }

    public System.Collections.Generic.List<RuleSO> GetCurrentRules()
    {
        return dayManager != null ? dayManager.CurrentRules : null;
    }

    public RuleSO GetTodayRule()
    {
        return dayManager != null ? dayManager.TodayRule : null;
    }

    public string GetCurrentRuleDescription()
    {
        return dayManager != null ? dayManager.CurrentRuleDescription : string.Empty;
    }

    public DayDataSO GetCurrentDayData()
    {
        return dayManager != null ? dayManager.CurrentDayData : null;
    }

    public EvaluationManager GetEvaluation()
    {
        return evaluationManager;
    }

    private void SetState(GameState nextState)
    {
        if (CurrentState == nextState)
            return;

        CurrentState = nextState;
        StateChanged?.Invoke(CurrentState);
    }

    private void GoToGameOver()
    {
        SetState(GameState.GameOver);
    }

    private void ShowEnding()
    {
        SetState(GameState.Ending);
    }
}
