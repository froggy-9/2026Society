using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(
    fileName = "NewDayData",
    menuName = "Refugees/Day Data"
)]
public class DayDataSO : ScriptableObject
{
    [Header("Day")]
    [Tooltip("몇 일차 데이터인지 적습니다. RefugeesGameManager의 CurrentDay와 같은 값을 찾습니다.")]
    public int day;

    [Tooltip("Format: yyyy-MM-dd")]
    public string currentDate;

    [FormerlySerializedAs("quota")]
    [Tooltip("하루 결과 평가용 목표 심사 수입니다. 하루 종료 조건은 dayTime입니다.")]
    public int targetInspectionCount;

    [Tooltip("이 하루의 실제 플레이 제한시간입니다. 초 단위입니다.")]
    public float dayTime = 180f;

    [Header("Content")]
    [Tooltip("이 날짜 시작 전에 보여줄 뉴스 SO입니다.")]
    public NewsSO news;

    [Header("Rule Description")]
    [FormerlySerializedAs("ruleNotice")]
    [TextArea(2, 5)]
    [Tooltip("규칙서 본문 위에 추가로 보여줄 그날 안내 문장입니다. 필요 없으면 비워둡니다.")]
    public string ruleDescription;

    [Tooltip("이 날짜에 사용할 규칙 SO입니다. 화면에는 RuleSO.description이 나오고, 판정에는 RuleSO.checkTypes가 쓰입니다.")]
    public RuleSO rule;

    [Header("Random NPC")]
    [Tooltip("이 날짜에 사용할 NPC 랜덤 재료표입니다.")]
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
