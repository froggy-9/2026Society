using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Refugees/Day Data")]
public class DayDataSO : ScriptableObject
{
    public int day;

    public int quota;

    public float dayTime = 180f;

    public NewsSO news;

    public List<RuleSO> rules;
}