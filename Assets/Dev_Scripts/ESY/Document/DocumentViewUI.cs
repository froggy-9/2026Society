using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DocumentViewUI : MonoBehaviour
{
    [Tooltip("이 문서 UI에 사용할 문서 양식 SO입니다. 여권이면 Passport 템플릿을 넣습니다.")]
    [SerializeField] private DocumentTemplateSO template;

    [Tooltip("문서 제목이 들어갈 TMP 텍스트입니다.")]
    [SerializeField] private TMP_Text titleText;

    [Tooltip("문서 내용이 들어갈 TMP 텍스트입니다.")]
    [SerializeField] private TMP_Text bodyText;

    [Tooltip("문서 사진이 들어갈 UI Image입니다. 여권 사진을 보여줄 칸입니다.")]
    [SerializeField] private Image portraitImage;

    [Tooltip("문서를 닫는 X 버튼입니다.")]
    [SerializeField] private Button closeButton;

    private void OnEnable()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
    }

    private void OnDisable()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(Close);
    }

    public void Show(DocumentData document)
    {
        if (document == null)
        {
            Close();
            return;
        }

        gameObject.SetActive(true);

        if (titleText != null)
            titleText.text = !string.IsNullOrWhiteSpace(template?.title) ? template.title : document.documentType.ToString();

        if (bodyText != null)
            bodyText.text = BuildBody(document, template);

        if (portraitImage != null)
        {
            portraitImage.sprite = document.portrait;
            portraitImage.enabled = document.portrait != null;
        }
    }

    public void Close()
    {
        if (portraitImage != null)
        {
            portraitImage.sprite = null;
            portraitImage.enabled = false;
        }

        gameObject.SetActive(false);
    }

    private static string BuildBody(DocumentData document, DocumentTemplateSO template)
    {
        StringBuilder builder = new StringBuilder();

        if (template != null && !string.IsNullOrWhiteSpace(template.issuer))
            builder.AppendLine(template.issuer);

        if (document.documentType == DocumentType.Passport)
        {
            AddLine(builder, Label(template, template?.passportNumberLabel, "Passport No."), document.passportCode);
            AddLine(builder, Label(template, template?.surnameLabel, "Surname"), document.englishSurname);
            AddLine(builder, Label(template, template?.givenNamesLabel, "Given Names"), document.englishGivenNames);
            AddLine(builder, Label(template, template?.koreanNameLabel, "Name in Korean"), document.koreanName);
            AddLine(builder, Label(template, template?.nationalityLabel, "Nationality"), document.nationality);
            AddLine(builder, Label(template, template?.genderLabel, "Sex"), document.gender.ToString());
            AddLine(builder, Label(template, template?.birthDateLabel, "Date of Birth"), document.dateOfBirth);
            AddLine(builder, Label(template, template?.issueDateLabel, "Date of Issue"), document.issueDate);
            AddLine(builder, Label(template, template?.expiryDateLabel, "Date of Expiry"), document.passportExpiryDate);
            AddLine(builder, Label(template, template?.authorityLabel, "Authority"), document.issuingAuthority);
        }
        else
        {
            AddLine(builder, Label(template, template?.documentNumberLabel, "Document No."), document.documentCode);
            AddLine(builder, Label(template, template?.nameLabel, "Name"), document.koreanName);
            AddLine(builder, Label(template, template?.genderLabel, "Sex"), document.gender.ToString());
            AddLine(builder, "Age", document.age > 0 ? document.age.ToString() : string.Empty);
            AddLine(builder, Label(template, template?.occupationLabel, "Occupation"), document.occupation);
            AddLine(builder, Label(template, template?.residenceLabel, "Residence"), document.residence);
            AddLine(builder, Label(template, template?.familyLabel, "Family"), document.familyRelationship);
            AddLine(builder, Label(template, template?.criminalRecordLabel, "Criminal Record"), document.hasCriminalRecord ? document.criminalRecordDetails : NoRecordText(template));
            AddLine(builder, Label(template, template?.medicalNoteLabel, "Medical Note"), document.psychiatricHistory);
        }

        return builder.ToString();
    }

    private static string Label(DocumentTemplateSO template, string label, string fallback)
    {
        return !string.IsNullOrWhiteSpace(label) ? label : fallback;
    }

    private static string NoRecordText(DocumentTemplateSO template)
    {
        if (template != null && !string.IsNullOrWhiteSpace(template.noRecordText))
            return template.noRecordText;

        return "None";
    }

    private static void AddLine(StringBuilder builder, string label, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        builder.Append(label);
        builder.Append(": ");
        builder.AppendLine(value);
    }
}
