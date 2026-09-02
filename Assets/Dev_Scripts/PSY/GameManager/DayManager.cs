using System.Collections.Generic;
using UnityEngine;

public class DayManager : MonoBehaviour
{
    [SerializeField] private List<DayDataSO> dayDatas;
    private readonly List<RuleSO> currentRules = new List<RuleSO>();

    public DayDataSO CurrentDayData { get; private set; }

    public NewsSO CurrentNews => CurrentDayData != null ? CurrentDayData.news : null;

    public List<RuleSO> CurrentRules => currentRules;

    public RuleSO TodayRule => CurrentDayData != null ? CurrentDayData.rule : null;

    public string CurrentRuleDescription => CurrentDayData != null ? CurrentDayData.ruleDescription : string.Empty;

    public int Quota => CurrentDayData != null ? CurrentDayData.targetInspectionCount : 0;

    public float DayTime => CurrentDayData != null ? CurrentDayData.dayTime : 0f;

    public bool LoadDay(int day)
    {
        CurrentDayData = dayDatas != null
            ? dayDatas.Find(x => x != null && x.day == day)
            : null;

        if (CurrentDayData == null)
        {
            Debug.LogError($"Day {day} data is missing.");
            return false;
        }

        RebuildRules(day);
        return true;
    }

    private void RebuildRules(int day)
    {
        currentRules.Clear();

        if (dayDatas == null)
            return;

        for (int i = 0; i < dayDatas.Count; i++)
        {
            DayDataSO dayData = dayDatas[i];

            if (dayData == null || dayData.day > day || dayData.rule == null)
                continue;

            if (!currentRules.Contains(dayData.rule))
                currentRules.Add(dayData.rule);
        }
    }
}
