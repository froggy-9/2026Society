using UnityEngine;

public class DocumentManager : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private NPCManager npcManager;

    [Header("Document Views")]
    [Tooltip("현재 씬처럼 여권/입국 신고서/진단서를 한 스크립트에서 모두 관리하는 통합 문서 UI입니다.")]
    [SerializeField] private DocumentViewUI submittedDocumentsView;

    [Tooltip("NPC가 도착한 뒤 제출한 여권을 보여줄 UI입니다.")]
    [SerializeField] private DocumentViewUI passportView;

    [Tooltip("NPC가 도착한 뒤 제출한 입국 신고서를 보여줄 UI입니다.")]
    [SerializeField] private DocumentViewUI entryPermitView;

    [Tooltip("NPC가 도착한 뒤 제출한 진단서를 보여줄 UI입니다.")]
    [SerializeField] private DocumentViewUI medicalCertificateView;

    private void Awake()
    {
        if (npcManager == null)
            npcManager = FindFirstObjectByType<NPCManager>();
    }

    private void OnEnable()
    {
        RegisterNpcManager();
        HideSubmittedDocuments();
    }

    private void OnDisable()
    {
        UnregisterNpcManager();
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

    public DocumentData GetMedicalCertificate()
    {
        if (CurrentNPC == null)
        {
            Debug.LogWarning("Current NPC is missing.");
            return null;
        }

        return CurrentNPC.medicalCertificate;
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

            case DocumentType.MedicalCertificate:
                return CurrentNPC.medicalCertificate;

            default:
                return null;
        }
    }

    public void ShowSubmittedDocuments(NPCData npc)
    {
        if (npc == null)
        {
            HideSubmittedDocuments();
            return;
        }

        if (submittedDocumentsView != null)
        {
            submittedDocumentsView.ShowSubmittedDocuments(npc);
            return;
        }

        ShowOrClose(passportView, npc.passport);
        ShowOrClose(entryPermitView, npc.entryPermit);
        ShowOrClose(medicalCertificateView, npc.medicalCertificate);
    }

    public void HideSubmittedDocuments()
    {
        submittedDocumentsView?.Close();
        passportView?.Close();
        entryPermitView?.Close();
        medicalCertificateView?.Close();
    }

    private void RegisterNpcManager()
    {
        if (npcManager == null)
            npcManager = FindFirstObjectByType<NPCManager>();

        if (npcManager == null)
            return;

        npcManager.NpcSpawned -= OnNpcSpawned;
        npcManager.NpcArrived -= OnNpcArrived;
        npcManager.NpcCleared -= OnNpcCleared;
        npcManager.NpcSpawned += OnNpcSpawned;
        npcManager.NpcArrived += OnNpcArrived;
        npcManager.NpcCleared += OnNpcCleared;
    }

    private void UnregisterNpcManager()
    {
        if (npcManager == null)
            return;

        npcManager.NpcSpawned -= OnNpcSpawned;
        npcManager.NpcArrived -= OnNpcArrived;
        npcManager.NpcCleared -= OnNpcCleared;
    }

    private void OnNpcSpawned(NPCController npc)
    {
        HideSubmittedDocuments();
    }

    private void OnNpcArrived(NPCController npc)
    {
        ShowSubmittedDocuments(npc != null ? npc.Data : null);
    }

    private void OnNpcCleared()
    {
        HideSubmittedDocuments();
    }

    private static void ShowOrClose(DocumentViewUI view, DocumentData document)
    {
        if (view == null)
            return;

        if (document == null)
            view.Close();
        else
            view.Show(document);
    }
}
