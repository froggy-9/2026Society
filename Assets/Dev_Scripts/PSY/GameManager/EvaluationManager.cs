using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InspectionRecord
{
    public string npcName;
    public bool playerApproved;
    public bool shouldApprove;
    public string reason;
    public Sprite portrait;
}

public class EvaluationManager : MonoBehaviour
{
    public int TotalScore { get; private set; }
    public int DayScore { get; private set; }

    public int ApprovedCount { get; private set; }
    public int DeniedCount { get; private set; }

    public int CorrectCount { get; private set; }
    public int WrongCount { get; private set; }
    public int CorrectApprovedCount { get; private set; }
    public int CorrectDeniedCount { get; private set; }
    public int WrongApprovedCount { get; private set; }
    public int WrongDeniedCount { get; private set; }

    public int CorrectCombo { get; private set; }
    public int WrongCombo { get; private set; }

    public int MissedQuotaCount { get; private set; }
    public float Accuracy { get; private set; }
    public IReadOnlyList<InspectionRecord> DayRecords => dayRecords;

    private readonly List<InspectionRecord> dayRecords = new List<InspectionRecord>();

    public void ResetDay()
    {
        DayScore = 0;
        ApprovedCount = 0;
        DeniedCount = 0;
        CorrectCount = 0;
        WrongCount = 0;
        CorrectApprovedCount = 0;
        CorrectDeniedCount = 0;
        WrongApprovedCount = 0;
        WrongDeniedCount = 0;
        CorrectCombo = 0;
        WrongCombo = 0;
        MissedQuotaCount = 0;
        Accuracy = 0f;
        dayRecords.Clear();
    }

    public void ResetGame()
    {
        TotalScore = 0;
        ResetDay();
    }

    public void LoadGame(int totalScore)
    {
        TotalScore = totalScore;
        ResetDay();
    }

    public void SubmitJudgement(
        bool playerApproved,
        bool npcShouldBeApproved,
        bool hardPenalty,
        NPCData npc = null,
        string reason = ""
    )
    {
        if (playerApproved)
            ApprovedCount++;
        else
            DeniedCount++;

        bool isCorrect = playerApproved == npcShouldBeApproved;
        int score = CalculateScore(isCorrect, hardPenalty);

        if (isCorrect && playerApproved)
            CorrectApprovedCount++;
        else if (isCorrect)
            CorrectDeniedCount++;
        else if (playerApproved)
            WrongApprovedCount++;
        else
            WrongDeniedCount++;

        DayScore += score;
        TotalScore += score;

        dayRecords.Add(new InspectionRecord
        {
            npcName = npc != null ? npc.npcName : string.Empty,
            playerApproved = playerApproved,
            shouldApprove = npcShouldBeApproved,
            reason = reason,
            portrait = npc != null ? npc.portrait : null
        });
    }

    public void CalculateResult(int inspectedCount, int quota)
    {
        MissedQuotaCount = Mathf.Max(0, quota - inspectedCount);
        Accuracy = inspectedCount == 0 ? 0f : (float)CorrectCount / inspectedCount;
    }

    public bool IsGameOver()
    {
        return TotalScore < 0;
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
