using System.Collections.Generic;
using UnityEngine;

public class DayManager : MonoBehaviour
{
    [SerializeField] private List<DayDataSO> dayDatas;
    private readonly List<RuleSO> currentDayRules = new List<RuleSO>();

    public DayDataSO CurrentDayData { get; private set; }

    public NewsSO CurrentNews => CurrentDayData != null ? CurrentDayData.news : null;

    public List<RuleSO> CurrentRules => currentDayRules;

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

        LoadCurrentDayRules();
        return true;
    }

    private void LoadCurrentDayRules()
    {
        currentDayRules.Clear();

        if (CurrentDayData == null || CurrentDayData.rule == null)
            return;

        currentDayRules.Add(CurrentDayData.rule);
    }
}
