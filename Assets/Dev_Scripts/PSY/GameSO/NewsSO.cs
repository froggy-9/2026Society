using UnityEngine;

[CreateAssetMenu(menuName = "Refugees/News")]
public class NewsSO : ScriptableObject
{
    public int day;

    public string title;

    [TextArea(5, 10)]
    public string body;
}