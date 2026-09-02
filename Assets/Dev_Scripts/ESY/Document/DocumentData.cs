using UnityEngine;

public enum DocumentType
{
    Passport,
    EntryPermit
}

public class DocumentData : ScriptableObject
{
    [Header("Document")]
    public DocumentType documentType;

    [Header("Person")]
    public string npcName;
    public Gender gender;
    public int age;
    public Sprite portrait;

    [Header("Profile")]
    public string occupation;
    public string residence;
    public string familyRelationship;

    [Header("Codes")]
    public string documentCode;
    public string passportCode;

    [Header("Risk")]
    public bool hasCriminalRecord;
    public string criminalRecordDetails;

    [TextArea(2, 5)]
    public string psychiatricHistory;

    [Header("Passport")]
    [Tooltip("Format: yyyy-MM-dd")]
    public string passportExpiryDate;
}
