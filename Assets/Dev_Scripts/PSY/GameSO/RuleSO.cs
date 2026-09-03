using UnityEngine;
using UnityEngine.Serialization;

public enum RuleCheckType
{
    None,
    PassportRequired,
    EntryPermitRequired,
    PortraitMatch,
    NameMatch,
    GenderMatch,
    AgeMatch,
    OccupationMatch,
    ResidenceMatch,
    DocumentCodeMatch,
    PassportCodeMatch,
    PassportNotExpired,
    NoCriminalRecord,
    NoPsychiatricHistory
}

[CreateAssetMenu(menuName = "Refugees/Rule")]
public class RuleSO : ScriptableObject
{
    [Tooltip("규칙 구분용 ID입니다. 화면 출력에는 필수로 쓰이지 않습니다.")]
    public int ruleID;

    [Tooltip("규칙 이름입니다. 판정 로그나 디버그용으로 쓰입니다.")]
    public string ruleName;

    [Tooltip("규칙서 본문에 그대로 출력할 문장입니다.")]
    [TextArea]
    public string description;

    [Header("Inspection")]
    [Tooltip("이 규칙에서 검사할 항목들입니다. 여러 개 넣으면 모두 통과해야 승인 대상입니다.")]
    public RuleCheckType[] checkTypes;

    [HideInInspector]
    [FormerlySerializedAs("checkType")]
    public RuleCheckType checkType = RuleCheckType.None;

    public RuleCheckType[] GetCheckTypes()
    {
        if (checkTypes != null && checkTypes.Length > 0)
            return checkTypes;

        return new[] { checkType };
    }
}
