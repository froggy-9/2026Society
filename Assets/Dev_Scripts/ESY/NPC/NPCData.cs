using UnityEngine;

public enum Gender
{
    Male,
    Female
}

public class NPCData : ScriptableObject
{
    [Header("Person")]
    public string npcName;
    public Gender gender;
    public int age;
    public Sprite portrait;

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
}
