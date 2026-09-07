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
    [Tooltip("몇 일차 데이터인지 적습니다. RefugeesGameManager의 CurrentDay와 같은 값을 찾습니다.")]
    public int day;

    [Tooltip("Format: yyyy-MM-dd")]
    public string currentDate;

    [Min(1)]
    [Tooltip("하루 최대 심사 인원입니다. 승인과 거절을 합산하며 한도에 도달하면 즉시 하루 결과를 표시합니다.")]
    public int maxInspectionCount = 10;

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

    [Tooltip("규칙서 설명, 제한 국가, 직업 확인 대상을 설정합니다. 실제 검사 항목은 아래 Inspection Items에서 선택합니다.")]
    public RuleSO rule;

    [Header("Random NPC")]
    [Tooltip("이 날짜에 사용할 NPC 랜덤 재료표입니다.")]
    public NpcTableSO npcTable;

    [FormerlySerializedAs("randomNpcCount")]
    [Tooltip("오늘 생성할 일반 NPC 후보 수입니다. 0이면 Max Inspection Count만큼 생성합니다.")]
    public int npcCount;

    [HideInInspector]
    [Tooltip("예전 고정 불허 NPC 수입니다. 현재는 NpcTableSO의 Invalid Npc Chance를 사용합니다.")]
    public int rejectNpcCount;

    [InspectorName("Inspection Items")]
    [Tooltip("오늘 심사할 불허 사유입니다. 선택한 항목만 검사하며 NPC 오류 생성에도 사용합니다. 이전 날짜의 항목도 계속 검사하려면 포함하세요. 비우면 규칙 검사를 하지 않습니다.")]
    public NpcFailReason[] rejectReasons;

    [Header("Special NPC")]
    [Tooltip("특수 NPC 목록입니다. 각 SO의 등장 일차에 현재 일차가 들어 있으면 오늘 목록에 섞입니다.")]
    public SpecialNpcSO[] specialNpcs;

    public RuleCheckType[] GetInspectionChecks()
    {
        var checks = new List<RuleCheckType>();
        if (rejectReasons == null)
            return checks.ToArray();

        foreach (NpcFailReason reason in rejectReasons)
        {
            RuleCheckType check;
            switch (reason)
            {
                case NpcFailReason.MissingPassport: check = RuleCheckType.PassportRequired; break;
                case NpcFailReason.MissingEntryPermit: check = RuleCheckType.EntryPermitRequired; break;
                case NpcFailReason.PortraitMismatch: check = RuleCheckType.PortraitMatch; break;
                case NpcFailReason.NationalityMismatch:
                case NpcFailReason.NameMismatch:
                case NpcFailReason.GenderMismatch:
                case NpcFailReason.AgeMismatch:
                case NpcFailReason.BirthDateMismatch:
                case NpcFailReason.PassportCodeMismatch:
                    check = day == 1 ? RuleCheckType.PassportEntryDataMatch : GetExistingMismatchCheck(reason);
                    break;
                case NpcFailReason.PassportExpired: check = RuleCheckType.PassportNotExpired; break;
                case NpcFailReason.BannedNationality: check = RuleCheckType.NationalityAllowed; break;
                case NpcFailReason.OccupationMismatch: check = RuleCheckType.OccupationMatch; break;
                case NpcFailReason.CriminalRecord: check = RuleCheckType.NoCriminalRecord; break;
                case NpcFailReason.DocumentCodeMismatch: check = RuleCheckType.DocumentCodeMatch; break;
                default: continue;
            }

            if (!checks.Contains(check))
                checks.Add(check);
        }

        return checks.ToArray();
    }

    private static RuleCheckType GetExistingMismatchCheck(NpcFailReason reason)
    {
        return reason switch
        {
            NpcFailReason.NationalityMismatch => RuleCheckType.NationalityMatch,
            NpcFailReason.NameMismatch => RuleCheckType.NameMatch,
            NpcFailReason.GenderMismatch => RuleCheckType.GenderMatch,
            NpcFailReason.AgeMismatch => RuleCheckType.AgeMatch,
            NpcFailReason.BirthDateMismatch => RuleCheckType.BirthDateMatch,
            NpcFailReason.PassportCodeMismatch => RuleCheckType.PassportCodeMatch,
            _ => RuleCheckType.None
        };
    }
}
