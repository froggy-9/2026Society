using UnityEngine;

[System.Serializable]
public class NpcQuestion
{
    public string question;

    [TextArea(2, 5)]
    public string answer;
}

public class NpcCase : ScriptableObject
{
    public NPCData npc;

    [Header("Dialogue")]
    public NpcDialogue dialogue;

    [Header("Questions")]
    public NpcQuestion[] questions;

    [Header("Plea")]
    public bool canPlead;

    [Range(0f, 1f)]
    public float pleaChance = 0f;

    [TextArea(2, 5)]
    public string pleaText;

    [TextArea(2, 5)]
    public string approveNews;

    [TextArea(2, 5)]
    public string rejectNews;
}
