using UnityEngine;
using UnityEngine.Serialization;

public enum RuleCheckType
{
    // Preserve the IDs stored in existing Rule assets.
    None = 0,
    PassportRequired = 1,
    EntryPermitRequired = 2,
    PortraitMatch = 4,
    NameMatch = 5,
    GenderMatch = 6,
    AgeMatch = 7,
    BirthDateMatch = 8,
    OccupationMatch = 9,
    DocumentCodeMatch = 13,
    PassportCodeMatch = 14,
    PassportNotExpired = 15,
    NoCriminalRecord = 17,
    NationalityAllowed = 19,
    NationalityMatch = 20,
    PassportEntryDataMatch = 21
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

    [HideInInspector]
    public RuleCheckType[] checkTypes;

    [Tooltip("입국 제한 국가입니다. DaySO의 Banned Nationality 항목에서 사용합니다.")]
    public string[] bannedNationalities;

    [Tooltip("직업 정보 대조 대상입니다. 비워두면 모든 직업을 검사합니다.")]
    public string[] inspectedOccupations;

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
