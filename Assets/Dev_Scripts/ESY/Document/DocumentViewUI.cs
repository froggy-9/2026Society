using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DocumentViewUI : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private Image portraitImage;
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
            titleText.text = document.documentType.ToString();

        if (bodyText != null)
            bodyText.text = BuildBody(document);

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

    private static string BuildBody(DocumentData document)
    {
        StringBuilder builder = new StringBuilder();

        AddLine(builder, "Name", document.npcName);
        AddLine(builder, "Gender", document.gender.ToString());
        AddLine(builder, "Age", document.age > 0 ? document.age.ToString() : string.Empty);
        AddLine(builder, "Job", document.occupation);
        AddLine(builder, "Address", document.residence);
        AddLine(builder, "Document Code", document.documentCode);
        AddLine(builder, "Passport Code", document.passportCode);
        AddLine(builder, "Passport Expiry", document.passportExpiryDate);
        AddLine(builder, "Criminal Record", document.hasCriminalRecord ? document.criminalRecordDetails : "None");
        AddLine(builder, "Family", document.familyRelationship);
        AddLine(builder, "Medical Note", document.psychiatricHistory);

        return builder.ToString();
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
