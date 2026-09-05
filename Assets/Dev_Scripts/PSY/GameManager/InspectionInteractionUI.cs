using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InspectionInteractionUI : MonoBehaviour
{
    [Header("Managers")]
    [Tooltip("현재 NPC를 관리하는 NPCManager입니다. 비워두면 씬에서 자동으로 찾습니다.")]
    [SerializeField] private NPCManager npcManager;

    [Header("NPC UI")]
    [Tooltip("현재 NPC 사진을 표시할 UI Image입니다. 월드 스프라이트를 직접 쓰면 비워둬도 됩니다.")]
    [SerializeField] private Image npcImage;

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

    [Header("Next NPC")]
    [Tooltip("현재 NPC가 없을 때 다음 NPC를 부르는 버튼입니다.")]
    [SerializeField] private Button nextNpcButton;

    [Header("Judgement")]
    [Tooltip("입국 허가 버튼입니다.")]
    [SerializeField] private Button approveButton;

    [Tooltip("입국 불허가 버튼입니다.")]
    [SerializeField] private Button rejectButton;

    private NPCController shownNpc;
    private bool waitingForPleaDecision;
    private InspectionDecision currentDecision;
    private int dialogueIndex;
    private bool showingDialogueSequence;

    private void Awake()
    {
        if (npcManager == null)
            npcManager = FindFirstObjectByType<NPCManager>();

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

        if (WasDialogueAdvancePressed())
            AdvanceDialogueByBubbleInput();

        RefreshNextNpcButton();
    }

    public void MarkDocumentChecked()
    {
    }

    public void Approve()
    {
        if (shownNpc == null || !shownNpc.IsReady)
            return;

        if (TryAdvanceDialogue())
            return;

        Submit(true);
    }

    public void Reject()
    {
        if (shownNpc == null || !shownNpc.IsReady)
            return;

        if (TryAdvanceDialogue())
            return;

        if (!waitingForPleaDecision && ShouldStartPlea())
        {
            waitingForPleaDecision = true;
            ShowDialogue(GetPleaText());
            return;
        }

        Submit(false);
    }

    public void RequestNextNpc()
    {
        npcManager?.RequestNextNPC();
        RefreshNextNpcButton();
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
        StartDialogueSequence();
        RefreshNextNpcButton();
    }

    private void OnShownNpcArrived(NPCController npc)
    {
        if (npc != shownNpc)
            return;

        if (!showingDialogueSequence)
            ShowDialogue(string.Empty);
    }

    private bool ShouldStartPlea()
    {
        return shownNpc != null && shownNpc.Data != null && shownNpc.Data.canPlead;
    }

    private string GetPleaText()
    {
        if (shownNpc?.Data == null)
            return string.Empty;

        if (!string.IsNullOrWhiteSpace(shownNpc.Data.pleaText))
            return shownNpc.Data.pleaText;

        if (shownNpc.Data.dialogueLines != null && shownNpc.Data.dialogueLines.Length > 0)
            return shownNpc.Data.dialogueLines[shownNpc.Data.dialogueLines.Length - 1];

        return string.Empty;
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
        dialogueIndex = 0;
        showingDialogueSequence = false;
        RefreshNextNpcButton();
    }

    private void AddListeners()
    {
        nextNpcButton?.onClick.AddListener(RequestNextNpc);
        approveButton?.onClick.AddListener(Approve);
        rejectButton?.onClick.AddListener(Reject);
    }

    private void RemoveListeners()
    {
        nextNpcButton?.onClick.RemoveListener(RequestNextNpc);
        approveButton?.onClick.RemoveListener(Approve);
        rejectButton?.onClick.RemoveListener(Reject);
    }

    private void UnsubscribeShownNpc()
    {
        if (shownNpc != null)
            shownNpc.Arrived -= OnShownNpcArrived;
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
            ShowDialogue(string.Empty);
        }
    }

    private void RefreshNextNpcButton()
    {
        if (nextNpcButton == null)
            return;

        nextNpcButton.gameObject.SetActive(npcManager != null && npcManager.CanRequestNextNpc);
    }

    private void StartDialogueSequence()
    {
        dialogueIndex = 0;
        showingDialogueSequence = shownNpc?.Data?.dialogueLines != null
            && shownNpc.Data.dialogueLines.Length > 0;

        if (showingDialogueSequence)
        {
            ShowDialogue(shownNpc.Data.dialogueLines[0]);
            dialogueIndex = 1;
            return;
        }

        ShowDialogue(string.Empty);
    }

    private bool TryAdvanceDialogue()
    {
        if (!showingDialogueSequence || shownNpc?.Data?.dialogueLines == null)
            return false;

        if (dialogueIndex < shownNpc.Data.dialogueLines.Length)
        {
            ShowDialogue(shownNpc.Data.dialogueLines[dialogueIndex]);
            dialogueIndex++;
            return true;
        }

        showingDialogueSequence = false;
        ShowDialogue(string.Empty);
        return true;
    }

    private void AdvanceDialogueByBubbleInput()
    {
        if (TryAdvanceDialogue())
            return;

        if (waitingForPleaDecision)
            ShowDialogue(string.Empty);
    }

    private bool WasDialogueAdvancePressed()
    {
        if (speechBubble == null || !speechBubble.activeInHierarchy)
            return false;

        Keyboard keyboard = Keyboard.current;

        if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame)
            return true;

        Mouse mouse = Mouse.current;

        if (mouse == null || !mouse.leftButton.wasPressedThisFrame)
            return false;

        RectTransform target = speechBubbleRect != null ? speechBubbleRect : speechBubble.transform as RectTransform;

        if (target == null)
            return false;

        Canvas canvas = target.GetComponentInParent<Canvas>();
        Camera camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;

        return RectTransformUtility.RectangleContainsScreenPoint(
            target,
            mouse.position.ReadValue(),
            camera
        );
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
