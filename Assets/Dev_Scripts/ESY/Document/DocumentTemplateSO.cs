using UnityEngine;

[CreateAssetMenu(
    fileName = "NewDocumentTemplate",
    menuName = "Refugees/Document Template"
)]
public class DocumentTemplateSO : ScriptableObject
{
    [Header("Document")]
    [Tooltip("이 템플릿을 어느 문서 타입에 쓸지 표시하는 값입니다.")]
    public DocumentType documentType = DocumentType.Passport;

    [Tooltip("문서 상단 제목입니다. 예: PASSPORT")]
    public string title = "PASSPORT";

    [Tooltip("문서 상단 또는 본문 첫 줄에 표시할 발급 주체 문구입니다.")]
    public string issuer = "REPUBLIC OF KOREA";

    [Header("Passport Labels")]
    [Tooltip("여권번호 라벨입니다.")]
    public string passportNumberLabel = "Passport No.";

    [Tooltip("영문 성 라벨입니다.")]
    public string surnameLabel = "Surname";

    [Tooltip("영문 이름 라벨입니다.")]
    public string givenNamesLabel = "Given Names";

    [Tooltip("한글 성명 라벨입니다.")]
    public string koreanNameLabel = "Name in Korean";

    [Tooltip("이름 라벨입니다. 입국허가서 같은 기타 서류에서 사용합니다.")]
    public string nameLabel = "Name";

    [Tooltip("국적 라벨입니다.")]
    public string nationalityLabel = "Nationality";

    [Tooltip("성별 라벨입니다.")]
    public string genderLabel = "Sex";

    [Tooltip("생년월일 라벨입니다.")]
    public string birthDateLabel = "Date of Birth";

    [Tooltip("발급일 라벨입니다.")]
    public string issueDateLabel = "Date of Issue";

    [Tooltip("만료일 라벨입니다.")]
    public string expiryDateLabel = "Date of Expiry";

    [Tooltip("발급기관 라벨입니다.")]
    public string authorityLabel = "Authority";

    [Header("Permit Labels")]
    [Tooltip("입국허가서/기타 서류 번호 라벨입니다.")]
    public string documentNumberLabel = "Document No.";

    [Tooltip("직업 라벨입니다.")]
    public string occupationLabel = "Occupation";

    [Tooltip("거주지 라벨입니다.")]
    public string residenceLabel = "Residence";

    [Tooltip("가족관계 라벨입니다.")]
    public string familyLabel = "Family";

    [Tooltip("전과 기록 라벨입니다.")]
    public string criminalRecordLabel = "Criminal Record";

    [Tooltip("의료/정신병력 기록 라벨입니다.")]
    public string medicalNoteLabel = "Medical Note";

    [Header("Default Values")]
    [Tooltip("전과나 기록이 없을 때 표시할 문구입니다.")]
    public string noRecordText = "None";
}
