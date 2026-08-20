using UnityEngine;

[CreateAssetMenu(menuName = "Refugees/Rule")]
public class RuleSO : ScriptableObject
{
    public int ruleID;

    public string ruleName;

    [TextArea]
    public string description;
}