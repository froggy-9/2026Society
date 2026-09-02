using UnityEngine;

[System.Serializable]
public class PlayerQuestion
{
    public string question;

    [TextArea(2, 5)]
    public string answer;
}

public class NpcDialogue : ScriptableObject
{
    [TextArea(2, 5)]
    public string firstLine;

    public PlayerQuestion[] questions;
}
