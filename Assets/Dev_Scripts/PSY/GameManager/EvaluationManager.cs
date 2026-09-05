using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InspectionRecord
{
    public string koreanName;
    public bool playerApproved;
    public bool shouldApprove;
    public string reason;
    public Sprite portrait;
}

[System.Serializable]
public struct DailyPerformanceResult
{
    public int inspectedCount;
    public int correctCount;
    public int wrongCount;
    public float accuracy;
    public int performanceScore;
    public int maxPerformanceScore;
    public string gradeLabel;
    public int bonusPay;
    public string comment;
}

public class EvaluationManager : MonoBehaviour
{
    [Header("Config")]
    [Tooltip("성과금 등급, 지급률, 엔딩 기준을 담은 설정 SO입니다.")]
    [SerializeField] private EvaluationConfigSO config;

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
    public DailyPerformanceResult LastDailyResult { get; private set; }

    public int CumulativeCorrectCount { get; private set; }
    public int CumulativeWrongAcceptCount { get; private set; }
    public int CumulativeWrongRejectCount { get; private set; }
    public int CumulativeInspectedCount => CumulativeCorrectCount + CumulativeWrongAcceptCount + CumulativeWrongRejectCount;
    public float CumulativeAccuracy => CumulativeInspectedCount == 0 ? 0f : (float)CumulativeCorrectCount / CumulativeInspectedCount;
    public int CumulativePerformanceScore { get; private set; }
    public int MaxCumulativePerformanceScore { get; private set; }
    public string CumulativeGradeLabel => GetGrade(CumulativeAccuracy).label;

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
        LastDailyResult = default;
        dayRecords.Clear();
    }

    public void ResetGame()
    {
        TotalScore = 0;
        CumulativeCorrectCount = 0;
        CumulativeWrongAcceptCount = 0;
        CumulativeWrongRejectCount = 0;
        CumulativePerformanceScore = 0;
        MaxCumulativePerformanceScore = 0;
        ResetDay();
    }

    public void LoadGame(int totalScore)
    {
        TotalScore = totalScore;
        CumulativeCorrectCount = 0;
        CumulativeWrongAcceptCount = 0;
        CumulativeWrongRejectCount = 0;
        CumulativePerformanceScore = 0;
        MaxCumulativePerformanceScore = 0;
        ResetDay();
    }

    public void LoadGame(
        int totalScore,
        int cumulativeCorrectCount,
        int cumulativeWrongAcceptCount,
        int cumulativeWrongRejectCount,
        int cumulativePerformanceScore,
        int maxCumulativePerformanceScore)
    {
        TotalScore = totalScore;
        CumulativeCorrectCount = cumulativeCorrectCount;
        CumulativeWrongAcceptCount = cumulativeWrongAcceptCount;
        CumulativeWrongRejectCount = cumulativeWrongRejectCount;
        CumulativePerformanceScore = cumulativePerformanceScore;
        MaxCumulativePerformanceScore = maxCumulativePerformanceScore;
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

        if (isCorrect)
            CumulativeCorrectCount++;

        if (isCorrect && playerApproved)
            CorrectApprovedCount++;
        else if (isCorrect)
            CorrectDeniedCount++;
        else if (playerApproved)
        {
            WrongApprovedCount++;
            CumulativeWrongAcceptCount++;
        }
        else
        {
            WrongDeniedCount++;
            CumulativeWrongRejectCount++;
        }

        DayScore += score;
        TotalScore += score;

        dayRecords.Add(new InspectionRecord
        {
            koreanName = npc != null ? npc.koreanName : string.Empty,
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
        PerformanceGrade grade = GetGrade(Accuracy);
        int performanceScore = Mathf.RoundToInt(Accuracy * 100f);
        int maxPerformanceScore = 100;
        int bonusPay = Mathf.RoundToInt(GetBaseBonus() * grade.payRate);

        CumulativePerformanceScore += performanceScore;
        MaxCumulativePerformanceScore += maxPerformanceScore;

        LastDailyResult = new DailyPerformanceResult
        {
            inspectedCount = inspectedCount,
            correctCount = CorrectCount,
            wrongCount = WrongCount,
            accuracy = Accuracy,
            performanceScore = performanceScore,
            maxPerformanceScore = maxPerformanceScore,
            gradeLabel = grade.label,
            bonusPay = bonusPay,
            comment = grade.comment
        };
    }

    public bool IsGameOver()
    {
        return false;
    }

    public RefugeesEndingType GetEndingType()
    {
        float minimumAccuracy = config != null ? config.preservationEndingMinimumAccuracy : 0.8f;

        if (CumulativeAccuracy >= minimumAccuracy)
            return RefugeesEndingType.Preservation;

        if (CumulativeWrongAcceptCount > CumulativeWrongRejectCount)
            return RefugeesEndingType.FollowUpCare;

        if (CumulativeWrongRejectCount > CumulativeWrongAcceptCount)
            return RefugeesEndingType.Closure;

        return config != null ? config.tieEndingType : RefugeesEndingType.Closure;
    }

    public EndingNewsContent GetEndingNewsContent()
    {
        RefugeesEndingType endingType = GetEndingType();
        return config != null ? config.GetEndingNewsContent(endingType) : null;
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

    private PerformanceGrade GetGrade(float accuracy)
    {
        return config != null ? config.GetGrade(accuracy) : new PerformanceGrade { label = "보통", minimumAccuracy = 0f, payRate = 0f };
    }

    private int GetBaseBonus()
    {
        return config != null ? config.baseBonus : 1000;
    }
}
