using UnityEngine;
using UnityEngine.Serialization;

public enum Gender
{
    Male,
    Female
}

[System.Serializable]
public class NPCData
{
    [Header("Person")]
    [HideInInspector]
    [FormerlySerializedAs("koreanName")]
    public string koreanName;

    [Tooltip("영문 성입니다. 예: KIM")]
    public string englishSurname;

    [Tooltip("영문 이름입니다. 예: MINJI")]
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

    public bool HasMedicalHistory => HasMedicalRecord(psychiatricHistory);

    public static bool HasMedicalRecord(string history)
    {
        if (string.IsNullOrWhiteSpace(history))
            return false;

        string value = history.Trim();
        return value != "없음"
            && !string.Equals(value, "None", System.StringComparison.OrdinalIgnoreCase);
    }

    [Header("Documents")]
    public DocumentData passport;
    public DocumentData entryPermit;
    public DocumentData medicalCertificate;

    [Header("Manual Decision")]
    public bool useManualDecision;
    public bool manualShouldApprove = true;
    public string manualDecisionReason;

    [Header("Dialogue")]
    [TextArea(2, 5)]
    public string[] dialogueLines;

    [Header("Plea")]
    public bool canPlead;

    [TextArea(2, 5)]
    public string pleaText;

    [TextArea(2, 5)]
    public string approvedFollowUpNews;

    [TextArea(2, 5)]
    public string rejectedFollowUpNews;
}
