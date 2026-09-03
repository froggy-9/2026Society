using UnityEngine;

public enum Gender
{
    Male,
    Female
}

[System.Serializable]
public class NPCData
{
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
    public string job;
    public string address;
    public string[] family;

    [Header("Codes")]
    public string documentCode;
    public string passportCode;

    [Header("Risk")]
    public bool hasCriminalRecord;
    public string criminalRecordDetails;

    [TextArea(2, 5)]
    public string psychiatricHistory;

    [Header("Documents")]
    public DocumentData passport;
    public DocumentData entryPermit;

    [Header("Plea")]
    public bool canPlead;

    [TextArea(2, 5)]
    public string pleaText;

    [TextArea(2, 5)]
    public string approvedFollowUpNews;

    [TextArea(2, 5)]
    public string rejectedFollowUpNews;
}
