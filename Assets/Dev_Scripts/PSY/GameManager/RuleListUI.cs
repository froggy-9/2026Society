using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class RuleListUI : MonoBehaviour
{
    [Header("Text")]
    [Tooltip("규칙서 제목이 들어갈 TMP 텍스트입니다.")]
    [SerializeField] private TMP_Text titleText;

    [TextArea]
    [Tooltip("규칙서 제목으로 표시할 문장입니다.")]
    [SerializeField] private string title = "근무 규칙";

    [Tooltip("규칙서 본문이 들어갈 TMP 텍스트입니다.")]
    [SerializeField] private TMP_Text bodyText;

    public void Show(IList<RuleSO> rules, string notice = "")
    {
        if (titleText != null)
            titleText.text = title;

        if (bodyText == null)
            return;

        string body = BuildBody(rules, notice);
        bodyText.text = body;
    }

    private static string BuildBody(IList<RuleSO> rules, string notice)
    {
        StringBuilder builder = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(notice))
        {
            builder.AppendLine(notice.Trim());
            builder.AppendLine();
        }

        if (rules == null || rules.Count == 0)
            return builder.ToString().TrimEnd();

        foreach (RuleSO rule in rules)
        {
            if (rule == null)
                continue;

            if (!string.IsNullOrWhiteSpace(rule.description))
            {
                builder.AppendLine(rule.description.Trim());
                builder.AppendLine();
            }
        }

        return builder.ToString().TrimEnd();
    }
}
