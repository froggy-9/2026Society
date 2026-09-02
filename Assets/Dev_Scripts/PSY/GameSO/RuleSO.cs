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
    public int ruleID;

    public string ruleName;

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
