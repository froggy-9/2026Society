using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class RuleListUI : MonoBehaviour
{
    [SerializeField] private TMP_Text noticeText;
    [SerializeField] private TMP_Text rulesText;

    public void Show(IList<RuleSO> rules, string notice = "")
    {
        if (noticeText != null)
            noticeText.text = notice;

        if (rulesText == null)
            return;

        if (rules == null || rules.Count == 0)
        {
            rulesText.text = string.Empty;
            return;
        }

        StringBuilder builder = new StringBuilder();

        foreach (RuleSO rule in rules)
        {
            if (rule == null)
                continue;

            builder.AppendLine($"- {rule.ruleName}");

            if (!string.IsNullOrWhiteSpace(rule.description))
                builder.AppendLine(rule.description);

            RuleCheckType[] checkTypes = rule.GetCheckTypes();
            if (checkTypes != null && checkTypes.Length > 0)
                builder.AppendLine($"검사 항목: {string.Join(", ", checkTypes)}");
        }

        rulesText.text = builder.ToString();
    }
}
