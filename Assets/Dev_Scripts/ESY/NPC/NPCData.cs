using UnityEngine;

public enum Gender
{
    Male,
    Female
}

[CreateAssetMenu(
    fileName = "NewNPCData",
    menuName = "Refugees/NPC Data"
)]
public class NPCData : ScriptableObject
{
    [Header("기본 정보")]
    public string npcName;
    public Gender gender;
    public int age;

    [Header("신상 정보")]
    public string occupation;
    public string residence;

    [Header("문서 정보")]
    public string documentCode;
    public string passportCode;

    [Header("신원 정보")]
    public bool hasCriminalRecord;
    public string criminalRecordDetails;

    public string familyRelationship;

    [TextArea(2, 5)]
    public string psychiatricHistory;

    [Header("여권")]
    public string passportExpiryDate;

    [Header("대화")]
    [TextArea(3, 5)]
    public string[] answers;

    [Header("연결된 서류")]
    public DocumentData passport;
    public DocumentData entryPermit;
}