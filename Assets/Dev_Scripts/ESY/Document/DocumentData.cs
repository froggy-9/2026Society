using UnityEngine;

public enum DocumentType
{
    Passport,
    EntryPermit
}

[CreateAssetMenu(
    fileName = "NewDocumentData",
    menuName = "Refugees/Document Data"
)]
public class DocumentData : ScriptableObject
{
    [Header("문서 종류")]
    public DocumentType documentType;

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
}