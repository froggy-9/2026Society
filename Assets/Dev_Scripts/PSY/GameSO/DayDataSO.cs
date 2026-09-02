using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(
    fileName = "NewDayData",
    menuName = "Refugees/Day Data"
)]
public class DayDataSO : ScriptableObject
{
    [Header("Day")]
    public int day;

    [Tooltip("Format: yyyy-MM-dd")]
    public string currentDate;

    [FormerlySerializedAs("quota")]
    [Tooltip("하루 결과 평가용 목표 심사 수입니다. 하루 종료 조건은 dayTime입니다.")]
    public int targetInspectionCount;
    public float dayTime = 180f;

    [Header("Content")]
    public NewsSO news;

    [Header("Rule Description")]
    [FormerlySerializedAs("ruleNotice")]
    [TextArea(2, 5)]
    public string ruleDescription;

    [Tooltip("오늘 새로 추가되는 규칙입니다. 이전 일차 규칙은 자동으로 유지됩니다.")]
    public RuleSO rule;

    [Header("NPC Cases")]
    [FormerlySerializedAs("inspectionProfiles")]
    [HideInInspector]
    public List<NpcCase> npcs;

    [Header("Random NPC")]
    public NpcTableSO npcTable;

    [FormerlySerializedAs("randomNpcCount")]
    [Tooltip("오늘 생성할 NPC 수입니다. 0이면 targetInspectionCount 수만큼 생성합니다.")]
    public int npcCount;

    [Tooltip("오늘 정답이 불허가인 NPC 수입니다.")]
    public int rejectNpcCount;

    [Tooltip("오늘 등장할 간청 NPC 수입니다.")]
    public int pleaNpcCount;

    [Tooltip("불허가 NPC를 만들 때 사용할 사유입니다. 비워두면 기본 사유에서 랜덤 선택합니다.")]
    public NpcFailReason[] rejectReasons;
}
