using UnityEngine;

public enum DocumentType
{
    Passport,
    EntryPermit
}

[System.Serializable]
public class DocumentData
{
    [Header("Document")]
    public DocumentType documentType;

    [Header("Person")]
    public string koreanName;
    public string englishSurname;
    public string englishGivenNames;
    public Gender gender;
    public int age;
    public Sprite portrait;
    public string nationality;
    public string dateOfBirth;

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
    public string issueDate;
    public string issuingAuthority;

    [Tooltip("Format: yyyy-MM-dd")]
    public string passportExpiryDate;
}
