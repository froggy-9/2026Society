using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class InspectionInteractionUI : MonoBehaviour
{
    [Header("Managers")]
    [Tooltip("현재 NPC를 관리하는 NPCManager입니다. 비워두면 씬에서 자동으로 찾습니다.")]
    [SerializeField] private NPCManager npcManager;

    [Tooltip("현재 NPC의 여권/입국허가서 데이터를 가져오는 DocumentManager입니다. 비워두면 씬에서 자동으로 찾습니다.")]
    [SerializeField] private DocumentManager documentManager;

    [Header("NPC UI")]
    [Tooltip("현재 NPC 사진을 표시할 UI Image입니다. 월드 스프라이트를 직접 쓰면 비워둬도 됩니다.")]
    [SerializeField] private Image npcImage;

    [Tooltip("현재 NPC의 한글 성명을 표시할 TMP 텍스트입니다. 화면에 안 보이면 비워둬도 됩니다.")]
    [FormerlySerializedAs("npcNameText")]
    [SerializeField] private TMP_Text koreanNameText;

    [Header("Plea Bubble")]
    [Tooltip("간청 이벤트 때 사연 문장을 표시할 TMP 텍스트입니다.")]
    [SerializeField] private TMP_Text dialogueText;

    [Tooltip("말풍선 전체 오브젝트입니다. 간청 문장이 있을 때만 켜집니다.")]
    [SerializeField] private GameObject speechBubble;

    [Tooltip("말풍선 배경 Image의 RectTransform입니다. 글자 길이에 맞춰 크기가 변합니다.")]
    [SerializeField] private RectTransform speechBubbleRect;

    [Tooltip("말풍선 안쪽 여백입니다.")]
    [SerializeField] private Vector2 speechPadding = new Vector2(60f, 40f);

    [Tooltip("말풍선 최소 크기입니다.")]
    [SerializeField] private Vector2 speechMinSize = new Vector2(360f, 110f);

    [Tooltip("말풍선 최대 크기입니다.")]
    [SerializeField] private Vector2 speechMaxSize = new Vector2(800f, 260f);

    [Header("Documents")]
    [Tooltip("여권 버튼입니다. 현재 NPC가 여권을 가지고 있으면 켜집니다.")]
    [SerializeField] private Button passportButton;

    [Tooltip("입국허가서 버튼입니다. 현재 NPC가 입국허가서를 가지고 있으면 켜집니다.")]
    [SerializeField] private Button entryPermitButton;

    [Tooltip("여권을 펼쳐서 보여줄 DocumentViewUI입니다.")]
    [SerializeField] private DocumentViewUI passportView;

    [Tooltip("입국허가서를 펼쳐서 보여줄 DocumentViewUI입니다.")]
    [SerializeField] private DocumentViewUI entryPermitView;

    [Header("Judgement")]
    [Tooltip("입국 허가 버튼입니다.")]
    [SerializeField] private Button approveButton;

    [Tooltip("입국 불허가 버튼입니다.")]
    [SerializeField] private Button rejectButton;

    [Tooltip("간청 중 승인 버튼을 강조할 Graphic입니다. 버튼 텍스트나 테두리 이미지를 넣으면 됩니다.")]
    [SerializeField] private Graphic approveHighlight;

    private NPCController shownNpc;
    private bool waitingForPleaDecision;
    private InspectionDecision currentDecision;

    private void Awake()
    {
        if (npcManager == null)
            npcManager = FindFirstObjectByType<NPCManager>();

        if (documentManager == null)
            documentManager = FindFirstObjectByType<DocumentManager>();
    }

    private void OnEnable()
    {
        AddListeners();
        RefreshForCurrentNpc();
    }

    private void OnDisable()
    {
        RemoveListeners();
        UnsubscribeShownNpc();
    }

    private void Update()
    {
        if (npcManager == null)
            return;

        if (shownNpc != npcManager.CurrentNPC)
            RefreshForCurrentNpc();
    }

    public void OpenPassport()
    {
        if (shownNpc == null || !shownNpc.IsReady)
            return;

        passportView?.Show(documentManager != null ? documentManager.GetPassport() : null);
    }

    public void OpenEntryPermit()
    {
        if (shownNpc == null || !shownNpc.IsReady)
            return;

        entryPermitView?.Show(documentManager != null ? documentManager.GetEntryPermit() : null);
    }

    public void MarkDocumentChecked()
    {
    }

    public void Approve()
    {
        if (shownNpc == null || !shownNpc.IsReady)
            return;

        Submit(true);
    }

    public void Reject()
    {
        if (shownNpc == null || !shownNpc.IsReady)
            return;

        if (!waitingForPleaDecision && ShouldStartPlea())
        {
            waitingForPleaDecision = true;
            ShowDialogue(shownNpc.Data.pleaText);
            SetApproveHighlight(true);
            return;
        }

        Submit(false);
    }

    private void Submit(bool approved)
    {
        if (shownNpc == null || RefugeesGameManager.Instance == null)
            return;

        RefugeesGameManager.Instance.SubmitJudgement(
            approved,
            currentDecision.shouldApprove,
            shownNpc.Data,
            currentDecision.reason
        );

        npcManager.CompleteCurrentNPC(approved);
        StorePleaNews(approved);
        ResetNpcUiState();
    }

    private void RefreshForCurrentNpc()
    {
        UnsubscribeShownNpc();
        shownNpc = npcManager != null ? npcManager.CurrentNPC : null;
        ResetNpcUiState();

        if (shownNpc == null)
        {
            SetNpcVisible(false);
            SetDocumentButtons();
            return;
        }

        shownNpc.Arrived += OnShownNpcArrived;

        DayDataSO dayData = RefugeesGameManager.Instance != null
            ? RefugeesGameManager.Instance.GetCurrentDayData()
            : null;

        currentDecision = InspectionJudge.Evaluate(
            shownNpc.Data,
            RefugeesGameManager.Instance != null ? RefugeesGameManager.Instance.GetCurrentRules() : null,
            dayData != null ? dayData.currentDate : string.Empty
        );

        SetNpcVisible(true);
        SetText(koreanNameText, shownNpc.Data != null ? shownNpc.Data.koreanName : string.Empty);
        SetDocumentButtons();
        ShowDialogue(string.Empty);
    }

    private void OnShownNpcArrived(NPCController npc)
    {
        if (npc != shownNpc)
            return;

        ShowDialogue(string.Empty);
    }

    private bool ShouldStartPlea()
    {
        return shownNpc != null && shownNpc.Data != null && shownNpc.Data.canPlead;
    }

    private void StorePleaNews(bool approved)
    {
        if (!waitingForPleaDecision || shownNpc?.Data == null)
            return;

        PleaResultLog.Add(approved
            ? shownNpc.Data.approvedFollowUpNews
            : shownNpc.Data.rejectedFollowUpNews
        );
    }

    private void ResetNpcUiState()
    {
        waitingForPleaDecision = false;
        currentDecision = default;
        SetApproveHighlight(false);
        passportView?.Close();
        entryPermitView?.Close();
    }

    private void AddListeners()
    {
        passportButton?.onClick.AddListener(OpenPassport);
        entryPermitButton?.onClick.AddListener(OpenEntryPermit);
        approveButton?.onClick.AddListener(Approve);
        rejectButton?.onClick.AddListener(Reject);
    }

    private void RemoveListeners()
    {
        passportButton?.onClick.RemoveListener(OpenPassport);
        entryPermitButton?.onClick.RemoveListener(OpenEntryPermit);
        approveButton?.onClick.RemoveListener(Approve);
        rejectButton?.onClick.RemoveListener(Reject);
    }

    private void UnsubscribeShownNpc()
    {
        if (shownNpc != null)
            shownNpc.Arrived -= OnShownNpcArrived;
    }

    private void SetApproveHighlight(bool active)
    {
        if (approveHighlight != null)
            approveHighlight.enabled = active;
    }

    private void SetNpcVisible(bool visible)
    {
        if (npcImage != null)
        {
            npcImage.sprite = visible && shownNpc != null && shownNpc.Data != null ? shownNpc.Data.portrait : null;
            npcImage.enabled = npcImage.sprite != null;
        }

        if (!visible)
        {
            SetText(koreanNameText, string.Empty);
            ShowDialogue(string.Empty);
        }
    }

    private void SetDocumentButtons()
    {
        NPCData data = shownNpc != null ? shownNpc.Data : null;

        if (passportButton != null)
            passportButton.gameObject.SetActive(data != null && data.passport != null);

        if (entryPermitButton != null)
            entryPermitButton.gameObject.SetActive(data != null && data.entryPermit != null);
    }

    private void ShowDialogue(string value)
    {
        SetText(dialogueText, value);

        bool hasText = !string.IsNullOrWhiteSpace(value);
        if (speechBubble != null)
            speechBubble.SetActive(hasText);

        if (hasText)
            ResizeSpeechBubble(value);
    }

    private void ResizeSpeechBubble(string value)
    {
        if (dialogueText == null)
            return;

        RectTransform bubbleRect = speechBubbleRect;
        if (bubbleRect == null && dialogueText.transform.parent != null)
            bubbleRect = dialogueText.transform.parent as RectTransform;

        if (bubbleRect == null)
            return;

        float preferredWidthLimit = Mathf.Max(speechMinSize.x, speechMaxSize.x - speechPadding.x);
        Vector2 preferred = dialogueText.GetPreferredValues(value, preferredWidthLimit, 0f);

        float width = Mathf.Clamp(preferred.x + speechPadding.x, speechMinSize.x, speechMaxSize.x);
        float height = Mathf.Clamp(preferred.y + speechPadding.y, speechMinSize.y, speechMaxSize.y);
        bubbleRect.sizeDelta = new Vector2(width, height);
    }

    private static void SetText(TMP_Text text, string value)
    {
        if (text != null)
            text.text = value;
    }
}
