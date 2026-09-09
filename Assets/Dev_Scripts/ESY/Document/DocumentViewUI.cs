using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

public class DocumentViewUI : MonoBehaviour
{
    [Header("Passport")]
    [Tooltip("여권 큰 문서 루트입니다. NPC가 여권을 제출했을 때만 켜집니다.")]
    [SerializeField] private GameObject passportRoot;

    [Tooltip("책상 위 작은 여권 루트입니다. 없으면 비워둡니다.")]
    [SerializeField] private GameObject passportCompactRoot;

    [SerializeField] private TMP_Text passportNumberText;
    [SerializeField] private TMP_Text passportSurnameText;
    [SerializeField] private TMP_Text passportGivenNameText;
    [SerializeField] private TMP_Text passportNationalityText;
    [SerializeField] private TMP_Text passportBirthDateText;
    [SerializeField] private TMP_Text passportIssueDateText;
    [SerializeField] private TMP_Text passportExpiryDateText;
    [SerializeField] private Image passportPortraitImage;


    [Header("Entry Permit")]
    [Tooltip("입국 신고서 큰 문서 루트입니다. NPC가 입국 신고서를 제출했을 때만 켜집니다.")]
    [SerializeField] private GameObject entryPermitRoot;

    [Tooltip("책상 위 작은 입국 신고서 루트입니다. 없으면 비워둡니다.")]
    [SerializeField] private GameObject entryPermitCompactRoot;

    [SerializeField] private TMP_Text entrySurnameText;
    [SerializeField] private TMP_Text entryGivenNameText;
    [FormerlySerializedAs("entryAgeText")]
    [SerializeField] private TMP_Text entryBirthDateText;
    [SerializeField] private TMP_Text entryGenderText;
    [SerializeField] private TMP_Text entryOccupationText;
    [SerializeField] private TMP_Text entryResidenceText;
    [FormerlySerializedAs("entryDocumentCodeText")]
    [SerializeField] private TMP_Text entryPassportNumberText;
    [SerializeField] private TMP_Text entryFamilyRelationshipText;
    [SerializeField] private TMP_Text entryMedicalHistoryText;


    [Header("Medical Certificate")]
    [Tooltip("진단서 큰 문서 루트입니다. NPC가 진단서를 제출했을 때만 켜집니다.")]
    [SerializeField] private GameObject medicalCertificateRoot;

    [Tooltip("책상 위 작은 진단서 루트입니다. 없으면 비워둡니다.")]
    [SerializeField] private GameObject medicalCertificateCompactRoot;

    [SerializeField] private TMP_Text medicalRegistrationNumberText;
    [SerializeField] private TMP_Text medicalNameText;
    [SerializeField] private TMP_Text medicalDiagnosisText;
    [SerializeField] private TMP_Text medicalCertificateDateText;
    [SerializeField] private TMP_Text medicalValidUntilText;


    public void ShowSubmittedDocuments(NPCData npc)
    {
        if (npc == null)
        {
            Close();
            return;
        }

        SetRootActive(passportRoot, false);
        SetRootActive(passportCompactRoot, npc.passport != null);
        SetRootActive(entryPermitRoot, false);
        SetRootActive(entryPermitCompactRoot, npc.entryPermit != null);
        SetRootActive(medicalCertificateRoot, false);
        SetRootActive(medicalCertificateCompactRoot, npc.medicalCertificate != null);

        Show(npc.passport);
        Show(npc.entryPermit);
        Show(npc.medicalCertificate);
    }

    public void Show(DocumentData document)
    {
        if (document == null)
            return;

        switch (document.documentType)
        {
            case DocumentType.Passport:
                ShowPassport(document);
                break;

            case DocumentType.EntryPermit:
                ShowEntryPermit(document);
                break;

            case DocumentType.MedicalCertificate:
                ShowMedicalCertificate(document);
                break;
        }
    }

    public void Close()
    {
        SetRootActive(passportRoot, false);
        SetRootActive(passportCompactRoot, false);
        SetRootActive(entryPermitRoot, false);
        SetRootActive(entryPermitCompactRoot, false);
        SetRootActive(medicalCertificateRoot, false);
        SetRootActive(medicalCertificateCompactRoot, false);
    }


    private void ShowPassport(DocumentData document)
    {
        SetText(passportNumberText, document.passportCode);
        SetText(passportSurnameText, document.englishSurname);
        SetText(passportGivenNameText, document.englishGivenNames);
        SetText(passportNationalityText, document.nationality);
        SetText(passportBirthDateText, document.dateOfBirth);
        SetText(passportIssueDateText, document.issueDate);
        SetText(passportExpiryDateText, document.passportExpiryDate);

        if (passportPortraitImage != null)
        {
            passportPortraitImage.sprite = document.portrait;
            passportPortraitImage.enabled = document.portrait != null;
        }
    }


    private void ShowEntryPermit(DocumentData document)
    {
        SetText(entrySurnameText, document.englishSurname);
        SetText(entryGivenNameText, document.englishGivenNames);

        SetText(entryBirthDateText, document.dateOfBirth);

        SetText(entryGenderText, document.gender.ToString());
        SetText(entryOccupationText, document.occupation);
        SetText(entryResidenceText, document.residence);
        SetText(entryPassportNumberText, document.passportCode);
        SetText(entryFamilyRelationshipText, document.familyRelationship);
        SetText(entryMedicalHistoryText, document.psychiatricHistory);
    }


    private void ShowMedicalCertificate(DocumentData document)
    {
        SetText(
            medicalRegistrationNumberText,
            document.registrationNumber
        );

        string fullName =
            $"{document.englishSurname} {document.englishGivenNames}".Trim();

        SetText(medicalNameText, fullName);
        SetText(medicalDiagnosisText, document.medicalDiagnosis);
        SetText(
            medicalCertificateDateText,
            document.medicalCertificateDate
        );
        SetText(
            medicalValidUntilText,
            document.medicalCertificateValidUntil
        );
    }


    private static void SetText(TMP_Text target, string value)
    {
        if (target != null)
            target.text = value ?? string.Empty;
    }

    private static void SetRootActive(GameObject root, bool active)
    {
        if (root != null)
            root.SetActive(active);
    }
}
