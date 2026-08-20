using UnityEngine;

public class RefugeesGameManager : MonoBehaviour
{
    public static RefugeesGameManager Instance;

    [Header("Managers")]
    [SerializeField] private DayManager dayManager;
    [SerializeField] private EvaluationManager evaluationManager;

    [Header("Game")]
    [SerializeField] private int maxDay = 7;

    public int CurrentDay { get; private set; }

    public GameState CurrentState { get; private set; }

    public float RemainingTime { get; private set; }

    public int InspectedNpcCount { get; private set; }

    private GameState previousState;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
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
        evaluationManager.ResetGame();

        CurrentDay = 1;

        StartDay();
    }

    private void StartDay()
    {
        if (!dayManager.LoadDay(CurrentDay))
        {
            CurrentState = GameState.GameOver;
            return;
        }

        evaluationManager.ResetDay();

        InspectedNpcCount = 0;
        RemainingTime = dayManager.DayTime;

        CurrentState = GameState.News;
    }

    public void StartInspection()
    {
        if (CurrentState != GameState.News)
            return;

        CurrentState = GameState.Inspection;
    }

    public void SubmitJudgement(bool playerApproved, bool npcShouldBeApproved)
    {
        if (CurrentState != GameState.Inspection)
            return;

        InspectedNpcCount++;

        bool hardPenalty = CurrentDay == 4;

        evaluationManager.SubmitJudgement(
            playerApproved,
            npcShouldBeApproved,
            hardPenalty
        );
    }

    public void EndDay()
    {
        if (CurrentState != GameState.Inspection)
            return;

        evaluationManager.CalculateResult(
            InspectedNpcCount,
            dayManager.Quota
        );

        if (evaluationManager.IsGameOver())
        {
            CurrentState = GameState.GameOver;
            return;
        }

        CurrentState = GameState.Result;
    }

    public void NextDay()
    {
        if (CurrentState != GameState.Result)
            return;

        if (CurrentDay >= maxDay)
        {
            CurrentState = GameState.GameOver;
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
            CurrentState = previousState;
            Time.timeScale = 1f;
            return;
        }

        previousState = CurrentState;
        CurrentState = GameState.Pause;
        Time.timeScale = 0f;
    }

    public bool CanSpawnNpc()
    {
        return CurrentState == GameState.Inspection;
    }

    public NewsSO GetCurrentNews()
    {
        return dayManager.CurrentNews;
    }

    public System.Collections.Generic.List<RuleSO> GetCurrentRules()
    {
        return dayManager.CurrentRules;
    }

    public DayDataSO GetCurrentDayData()
    {
        return dayManager.CurrentDayData;
    }

    public EvaluationManager GetEvaluation()
    {
        return evaluationManager;
    }
}