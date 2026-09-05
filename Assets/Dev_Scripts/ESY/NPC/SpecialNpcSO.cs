using UnityEngine;

[CreateAssetMenu(
    fileName = "NewSpecialNpc",
    menuName = "Refugees/Special NPC"
)]
public class SpecialNpcSO : ScriptableObject
{
    [Header("Schedule")]
    [Tooltip("이 특수 NPC가 등장할 일차입니다. 해당 일차 안에서 등장 순서는 랜덤입니다.")]
    public int[] appearDays;

    [Header("Person")]
    [Tooltip("한글 성명입니다.")]
    public string koreanName;

    [Tooltip("영문 성입니다.")]
    public string englishSurname;

    [Tooltip("영문 이름입니다.")]
    public string englishGivenNames;

    [Tooltip("성별입니다.")]
    public Gender gender;

    [Tooltip("나이입니다.")]
    public int age;

    [Tooltip("화면에 서 있는 NPC 얼굴/전신 스프라이트입니다.")]
    public Sprite portrait;

    [Tooltip("국적입니다.")]
    public string nationality;

    [Tooltip("생년월일입니다. yyyy-MM-dd 형식을 권장합니다.")]
    public string dateOfBirth;

    [Header("Profile")]
    [Tooltip("직업입니다.")]
    public string job;

    [Tooltip("주소 또는 체류 예정지입니다.")]
    public string address;

    [Tooltip("가족관계입니다.")]
    public string[] family;

    [Header("Documents")]
    [Tooltip("특수 NPC가 가지고 올 여권 데이터입니다. 없으면 여권 미제출로 처리됩니다.")]
    public DocumentData passport;

    [Tooltip("특수 NPC가 가지고 올 입국허가서 데이터입니다. 없으면 입국허가서 미제출로 처리됩니다.")]
    public DocumentData entryPermit;

    [Header("Judgement")]
    [Tooltip("켜면 규칙 판정 대신 아래 수동 정답을 사용합니다.")]
    public bool useManualDecision = true;

    [Tooltip("수동 정답입니다. 입국 가능이면 켜고, 입국 불가능이면 끕니다.")]
    public bool shouldApprove = true;

    [Tooltip("수동 정답을 쓸 때 결과 기록에 남길 사유입니다.")]
    public string decisionReason = "Special NPC";

    [Header("Dialogue")]
    [Tooltip("NPC 대사입니다. 입력한 리스트 순서대로 말풍선에 표시됩니다.")]
    [TextArea(2, 5)]
    public string[] dialogueLines;

    [Tooltip("최종 승인했을 때 다음 뉴스에 추가할 후속 기사입니다. 비워두면 추가되지 않습니다.")]
    [TextArea(2, 5)]
    public string approvedFollowUpNews;

    [Tooltip("최종 거절했을 때 다음 뉴스에 추가할 후속 기사입니다. 비워두면 추가되지 않습니다.")]
    [TextArea(2, 5)]
    public string rejectedFollowUpNews;

    public bool CanAppearOnDay(int day)
    {
        if (appearDays == null || appearDays.Length == 0)
            return false;

        for (int i = 0; i < appearDays.Length; i++)
        {
            if (appearDays[i] == day)
                return true;
        }

        return false;
    }

    public NPCData CreateNpc()
    {
        NPCData npc = new NPCData
        {
            koreanName = koreanName,
            englishSurname = englishSurname,
            englishGivenNames = englishGivenNames,
            gender = gender,
            age = age,
            portrait = portrait,
            nationality = nationality,
            dateOfBirth = dateOfBirth,
            job = job,
            address = address,
            family = family,
            passport = CloneDocument(passport),
            entryPermit = CloneDocument(entryPermit),
            useManualDecision = useManualDecision,
            manualShouldApprove = shouldApprove,
            manualDecisionReason = decisionReason,
            dialogueLines = dialogueLines,
            canPlead = dialogueLines != null && dialogueLines.Length > 0,
            approvedFollowUpNews = approvedFollowUpNews,
            rejectedFollowUpNews = rejectedFollowUpNews
        };

        FillDocumentFromNpc(npc.passport, npc);
        FillDocumentFromNpc(npc.entryPermit, npc);

        return npc;
    }

    private static DocumentData CloneDocument(DocumentData source)
    {
        if (source == null)
            return null;

        return new DocumentData
        {
            documentType = source.documentType,
            koreanName = source.koreanName,
            englishSurname = source.englishSurname,
            englishGivenNames = source.englishGivenNames,
            gender = source.gender,
            age = source.age,
            portrait = source.portrait,
            nationality = source.nationality,
            dateOfBirth = source.dateOfBirth,
            occupation = source.occupation,
            residence = source.residence,
            familyRelationship = source.familyRelationship,
            documentCode = source.documentCode,
            passportCode = source.passportCode,
            hasCriminalRecord = source.hasCriminalRecord,
            criminalRecordDetails = source.criminalRecordDetails,
            psychiatricHistory = source.psychiatricHistory,
            issueDate = source.issueDate,
            issuingAuthority = source.issuingAuthority,
            passportExpiryDate = source.passportExpiryDate
        };
    }

    private static void FillDocumentFromNpc(DocumentData document, NPCData npc)
    {
        if (document == null || npc == null)
            return;

        if (string.IsNullOrWhiteSpace(document.koreanName))
            document.koreanName = npc.koreanName;

        if (string.IsNullOrWhiteSpace(document.englishSurname))
            document.englishSurname = npc.englishSurname;

        if (string.IsNullOrWhiteSpace(document.englishGivenNames))
            document.englishGivenNames = npc.englishGivenNames;

        if (document.portrait == null)
            document.portrait = npc.portrait;

        if (string.IsNullOrWhiteSpace(document.nationality))
            document.nationality = npc.nationality;

        if (string.IsNullOrWhiteSpace(document.dateOfBirth))
            document.dateOfBirth = npc.dateOfBirth;
    }
}
