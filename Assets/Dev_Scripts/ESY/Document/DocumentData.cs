using UnityEngine;
using UnityEngine.Serialization;

public enum DocumentType
{
    Passport,
    EntryPermit,
    MedicalCertificate
}

[System.Serializable]
public class DocumentData
{
    [Header("Document")]
    public DocumentType documentType;

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
    public string nationality;
    public string dateOfBirth;

    [Header("Profile")]
    public string occupation;
    public string residence;
    public string familyRelationship;

    [Header("Codes")]
    public string passportCode;
    public string registrationNumber;

    [Header("Risk")]

    [TextArea(2, 5)]
    public string psychiatricHistory;

    [Tooltip("진단서에 표시할 병명 또는 병원 기록입니다.")]
    public string medicalDiagnosis;

    [Header("Passport")]
    public string issueDate;

    [Tooltip("Format: yyyy-MM-dd")]
    public string passportExpiryDate;

    [Header("Medical Certificate")]
    [Tooltip("진단서 작성일입니다. 형식은 yyyy-MM-dd를 권장합니다.")]
    public string medicalCertificateDate;

    [Tooltip("진단서 유효기간입니다. 형식은 yyyy-MM-dd를 권장합니다.")]
    public string medicalCertificateValidUntil;
}
