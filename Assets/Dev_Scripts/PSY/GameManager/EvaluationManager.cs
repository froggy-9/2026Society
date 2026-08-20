using UnityEngine;

public class EvaluationManager : MonoBehaviour
{
    public int TotalScore { get; private set; }
    public int DayScore { get; private set; }

    public int ApprovedCount { get; private set; }
    public int DeniedCount { get; private set; }

    public int CorrectCount { get; private set; }
    public int WrongCount { get; private set; }

    public int CorrectCombo { get; private set; }
    public int WrongCombo { get; private set; }

    public int MissedQuotaCount { get; private set; }
    public float Accuracy { get; private set; }

    /// <summary>
    /// 하루 평가 초기화
    /// </summary>
    public void ResetDay()
    {
        DayScore = 0;

        ApprovedCount = 0;
        DeniedCount = 0;

        CorrectCount = 0;
        WrongCount = 0;

        CorrectCombo = 0;
        WrongCombo = 0;

        MissedQuotaCount = 0;
        Accuracy = 0f;
    }

    /// <summary>
    /// 게임 전체 초기화
    /// </summary>
    public void ResetGame()
    {
        TotalScore = 0;
        ResetDay();
    }

    /// <summary>
    /// 심사 결과 제출
    /// </summary>
    public void SubmitJudgement(bool playerApproved, bool npcShouldBeApproved, bool hardPenalty)
    {
        if (playerApproved)
            ApprovedCount++;
        else
            DeniedCount++;

        bool isCorrect = playerApproved == npcShouldBeApproved;

        int score = CalculateScore(isCorrect, hardPenalty);

        DayScore += score;
        TotalScore += score;
    }

    /// <summary>
    /// 하루 종료 계산
    /// </summary>
    public void CalculateResult(int inspectedCount, int quota)
    {
        MissedQuotaCount = Mathf.Max(0, quota - inspectedCount);

        Accuracy = inspectedCount == 0
            ? 0f
            : (float)CorrectCount / inspectedCount;
    }

    /// <summary>
    /// 게임오버 조건
    /// </summary>
    public bool IsGameOver()
    {
        return TotalScore < 0 || MissedQuotaCount > 0;
    }

    private int CalculateScore(bool isCorrect, bool hardPenalty)
    {
        if (isCorrect)
        {
            CorrectCount++;

            CorrectCombo++;
            WrongCombo = 0;

            return 30 + (CorrectCombo - 1) * 10;
        }

        WrongCount++;

        WrongCombo++;
        CorrectCombo = 0;

        int penalty = hardPenalty ? -200 : -100;

        return penalty - (WrongCombo - 1) * 10;
    }
}