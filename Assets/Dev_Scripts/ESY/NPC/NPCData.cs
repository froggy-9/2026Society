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
    [System.NonSerialized] public bool hasViewedDocument;

    public int DocumentCount => (passport != null ? 1 : 0)
        + (entryPermit != null ? 1 : 0) + (medicalCertificate != null ? 1 : 0);

    [Header("Person")]
    [HideInInspector]
    [FormerlySerializedAs("koreanName")]
    public string koreanName;

    [Tooltip("영문 성입니다. 예: KIM")]
    public string englishSurname;

    [Tooltip("영문 이름입니다. 예: MINJI")]
    public string englishGivenNames;

    public Gender gender;
    public Sprite portrait;

    [Tooltip("화면의 NPC와 여권 사진이 같은 인물인지 나타냅니다. 오류가 있는 일반 NPC는 false가 됩니다.")]
    public bool passportPhotoMatchesNpc = true;
    public string nationality;
    public string dateOfBirth;

    [Header("Profile")]
    public string job;
    public string address;
    public static bool AddressMatchesNationality(string country, string residence)
    {
        if (string.IsNullOrWhiteSpace(country) || string.IsNullOrWhiteSpace(residence))
            return false;

        string prefix = country.Trim() + " ";
        string value = residence.Trim();
        return value.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(value.Substring(prefix.Length));
    }

    public string[] family;

    [Header("Codes")]
    public string passportCode;

    [Header("Risk")]

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
