using UnityEngine;

public class DocumentManager : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private NPCManager npcManager;

    private void Awake()
    {
        if (npcManager == null)
            npcManager = FindObjectOfType<NPCManager>();
    }

    private NPCData CurrentNPC
    {
        get
        {
            if (npcManager == null)
                return null;

            if (npcManager.CurrentNPC == null)
                return null;

            return npcManager.CurrentNPC.Data;
        }
    }

    public DocumentData GetPassport()
    {
        if (CurrentNPC == null)
        {
            Debug.LogWarning("Current NPC is missing.");
            return null;
        }

        return CurrentNPC.passport;
    }

    public DocumentData GetEntryPermit()
    {
        if (CurrentNPC == null)
        {
            Debug.LogWarning("Current NPC is missing.");
            return null;
        }

        return CurrentNPC.entryPermit;
    }

    public DocumentData GetDocument(DocumentType type)
    {
        if (CurrentNPC == null)
            return null;

        switch (type)
        {
            case DocumentType.Passport:
                return CurrentNPC.passport;

            case DocumentType.EntryPermit:
                return CurrentNPC.entryPermit;

            default:
                return null;
        }
    }
}
