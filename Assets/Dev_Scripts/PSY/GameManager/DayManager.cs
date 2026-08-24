using System.Collections.Generic;
using UnityEngine;

public class DayManager : MonoBehaviour
{
    [SerializeField] private List<DayDataSO> dayDatas;

    public DayDataSO CurrentDayData { get; private set; }

    public NewsSO CurrentNews => CurrentDayData.news;

    public List<RuleSO> CurrentRules => CurrentDayData.rules;

    public int Quota => CurrentDayData.quota;

    public float DayTime => CurrentDayData.dayTime;

    public bool LoadDay(int day)
    {
        CurrentDayData = dayDatas.Find(x => x.day == day);

        if (CurrentDayData == null)
        {
            Debug.LogError($"Day {day} 데이터를 찾을 수 없습니다.");
            return false;
        }

        return true;
    }
}