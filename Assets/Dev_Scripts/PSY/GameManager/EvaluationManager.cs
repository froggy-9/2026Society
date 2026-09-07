using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InspectionRecord
{
    public string englishName;
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
    public int judgementPerformanceMoney;
    public int rentCost;
    public int foodCost;
    public int heatingCost;
    public int livingCost;
    public int netChange;
    public int ownedPerformanceMoney;
    public string gradeLabel;
    public string comment;
    public bool isDaySettled;
}

public class EvaluationManager : MonoBehaviour
{
    [Header("Config")]
    [Tooltip("성과금 등급, 지급률, 엔딩 기준을 담은 설정 SO입니다.")]
    [SerializeField] private EvaluationConfigSO config;

    public int OwnedPerformanceMoney { get; private set; }
    public int TodayJudgementPerformanceMoney { get; private set; }
    public int TodayNetChange { get; private set; }
    public int TodayLivingCost { get; private set; }
    public bool IsDaySettled { get; private set; }

    public int TotalScore => OwnedPerformanceMoney;
    public int DayScore => TodayJudgementPerformanceMoney;
    public int FinalScore => Mathf.Max(0, OwnedPerformanceMoney);

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

    public float Accuracy { get; private set; }
    public IReadOnlyList<InspectionRecord> DayRecords => dayRecords;
    public DailyPerformanceResult LastDailyResult { get; private set; }

    public int CumulativeCorrectCount { get; private set; }
    public int CumulativeWrongAcceptCount { get; private set; }
    public int CumulativeWrongRejectCount { get; private set; }
    public int CumulativeInspectedCount => CumulativeCorrectCount + CumulativeWrongAcceptCount + CumulativeWrongRejectCount;
    public float CumulativeAccuracy => CumulativeInspectedCount == 0 ? 0f : (float)CumulativeCorrectCount / CumulativeInspectedCount;

    private readonly List<InspectionRecord> dayRecords = new List<InspectionRecord>();

    public void ResetDay()
    {
        TodayJudgementPerformanceMoney = 0;
        TodayNetChange = 0;
        TodayLivingCost = GetLivingCostTotal();
        IsDaySettled = false;
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
        Accuracy = 0f;
        LastDailyResult = default;
        dayRecords.Clear();
    }

    public void ResetGame()
    {
        OwnedPerformanceMoney = config != null ? Mathf.Max(0, config.startingPerformanceMoney) : 60;
        CumulativeCorrectCount = 0;
        CumulativeWrongAcceptCount = 0;
        CumulativeWrongRejectCount = 0;
        ResetDay();
    }

    public void SubmitJudgement(
        bool playerApproved,
        bool npcShouldBeApproved,
        NPCData npc = null,
        string reason = "",
        int currentDay = 1
    )
    {
        if (playerApproved)
            ApprovedCount++;
        else
            DeniedCount++;

        bool isCorrect = playerApproved == npcShouldBeApproved;
        int performanceMoney = CalculateJudgementPerformanceMoney(isCorrect, currentDay);

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

        TodayJudgementPerformanceMoney += performanceMoney;

        dayRecords.Add(new InspectionRecord
        {
            englishName = npc != null ? $"{npc.englishSurname} {npc.englishGivenNames}".Trim() : string.Empty,
            playerApproved = playerApproved,
            shouldApprove = npcShouldBeApproved,
            reason = reason,
            portrait = npc != null ? npc.portrait : null
        });
    }

    public void CalculateResult(int inspectedCount)
    {
        Accuracy = inspectedCount == 0 ? 0f : (float)CorrectCount / inspectedCount;

        SettleDayOnce();
        SettlementGrade grade = GetSettlementGrade(OwnedPerformanceMoney);
        string comment = WrongCount > 0
            ? "일부 심사 과정에서 규정 위반 사항이 확인되었습니다."
            : grade.comment;

        LastDailyResult = new DailyPerformanceResult
        {
            inspectedCount = inspectedCount,
            correctCount = CorrectCount,
            wrongCount = WrongCount,
            accuracy = Accuracy,
            judgementPerformanceMoney = TodayJudgementPerformanceMoney,
            rentCost = GetRentCost(),
            foodCost = GetFoodCost(),
            heatingCost = GetHeatingCost(),
            livingCost = TodayLivingCost,
            netChange = TodayNetChange,
            ownedPerformanceMoney = OwnedPerformanceMoney,
            gradeLabel = grade.label,
            comment = comment,
            isDaySettled = IsDaySettled
        };
    }

    public void SettleDayOnce()
    {
        if (IsDaySettled)
            return;

        TodayLivingCost = GetLivingCostTotal();
        TodayNetChange = TodayJudgementPerformanceMoney - TodayLivingCost;
        OwnedPerformanceMoney += TodayNetChange;
        IsDaySettled = true;
    }

    public bool IsGameOver()
    {
        return IsDaySettled && OwnedPerformanceMoney < 0;
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

    private int CalculateJudgementPerformanceMoney(bool isCorrect, int currentDay)
    {
        if (isCorrect)
        {
            CorrectCount++;
            CorrectCombo++;
            WrongCombo = 0;
            return GetCorrectBaseBonus() + (CorrectCombo - 1) * GetCorrectStreakBonusStep();
        }

        WrongCount++;
        WrongCombo++;
        CorrectCombo = 0;

        int penalty = GetWrongBasePenalty() + (WrongCombo - 1) * GetWrongStreakPenaltyStep();

        if (config != null && currentDay == config.finalDayNumber)
            penalty += config.finalDayWrongSelectionPenalty;

        return -penalty;
    }

    private SettlementGrade GetSettlementGrade(int ownedPerformanceMoney)
    {
        return config != null
            ? config.GetSettlementGrade(ownedPerformanceMoney)
            : new SettlementGrade { label = ownedPerformanceMoney < 0 ? "미흡" : "보통", minimumOwnedPerformanceMoney = 0 };
    }

    private int GetCorrectBaseBonus()
    {
        return config != null ? config.correctBaseBonus : 3;
    }

    private int GetCorrectStreakBonusStep()
    {
        return config != null ? config.correctStreakBonusStep : 2;
    }

    private int GetWrongBasePenalty()
    {
        return config != null ? config.wrongBasePenalty : 4;
    }

    private int GetWrongStreakPenaltyStep()
    {
        return config != null ? config.wrongStreakPenaltyStep : 1;
    }

    private int GetRentCost()
    {
        return config != null ? config.rentCost : 40;
    }

    private int GetFoodCost()
    {
        return config != null ? config.foodCost : 15;
    }

    private int GetHeatingCost()
    {
        return config != null ? config.heatingCost : 5;
    }

    private int GetLivingCostTotal()
    {
        return config != null ? config.GetLivingCostTotal() : 60;
    }

}
