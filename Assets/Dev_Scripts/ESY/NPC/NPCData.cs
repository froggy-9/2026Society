using UnityEngine;

[CreateAssetMenu(
    fileName = "NewNPCData",
    menuName = "Refugees/NPC Data"
)]
public class NPCData : ScriptableObject
{
    [Header("기본 정보")]
    public string npcName;

    [Header("NPC 정보")]
    public string country;
    public string occupation;
    public string realDocumentCode;

    [Header("대화")]
    [TextArea(3, 5)]
    public string[] answers;

    [Header("연결된 서류")]
    public DocumentData passport;
    public DocumentData entryPermit;
}