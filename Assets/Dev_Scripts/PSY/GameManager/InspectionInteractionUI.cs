using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InspectionInteractionUI : MonoBehaviour
{
    [Header("Managers")]
    [Tooltip("현재 NPC를 관리하는 NPCManager입니다. 비워두면 씬에서 자동으로 찾습니다.")]
    [SerializeField] private NPCManager npcManager;

    [Tooltip("NPC가 제출한 문서를 표시/숨김 처리하는 DocumentManager입니다. 비워두면 씬에서 자동으로 찾습니다.")]
    [SerializeField] private DocumentManager documentManager;

    [Header("NPC Canvas Image")]
    [Tooltip("NPCCanvas > Image입니다. 현재 NPC 사진을 이 Image에 넣고 움직입니다.")]
    [SerializeField] private Image npcImage;

    [Tooltip("NPC Image의 RectTransform입니다. 비워두면 Npc Image에서 자동으로 가져옵니다.")]
    [SerializeField] private RectTransform npcImageRoot;

    [Tooltip("NPC가 화면 밖에서 시작하는 씬 포인트입니다. 비워두면 아래 좌표값을 사용합니다.")]
    [SerializeField] private Transform npcSpawnPoint;

    [Tooltip("NPC가 걸어와서 멈추는 씬 포인트입니다. 비워두면 아래 좌표값을 사용합니다.")]
    [SerializeField] private Transform npcWaitPoint;

    [Tooltip("승인된 NPC가 나가는 씬 포인트입니다. 비워두면 아래 좌표값을 사용합니다.")]
    [SerializeField] private Transform npcApproveExitPoint;

    [Tooltip("거절된 NPC가 나가는 씬 포인트입니다. 비워두면 아래 좌표값을 사용합니다.")]
    [SerializeField] private Transform npcRejectExitPoint;

    [Tooltip("NPC가 화면 밖에서 시작하는 UI 위치입니다.")]
    [SerializeField] private Vector2 npcSpawnPosition = new Vector2(-650f, -60f);

    [Tooltip("NPC가 걸어와서 멈추는 심사 위치입니다.")]
    [SerializeField] private Vector2 npcWaitPosition = new Vector2(160f, -60f);

    [Tooltip("승인된 NPC가 화면 밖으로 나가는 UI 위치입니다.")]
    [SerializeField] private Vector2 npcApproveExitPosition = new Vector2(760f, -60f);

    [Tooltip("거절된 NPC가 화면 밖으로 되돌아가는 UI 위치입니다.")]
    [SerializeField] private Vector2 npcRejectExitPosition = new Vector2(-760f, -60f);

    [Tooltip("NPC Image 입장 이동 시간입니다.")]
    [SerializeField] private float npcEnterDuration = 1.8f;

    [Tooltip("NPC Image 퇴장 이동 시간입니다.")]
    [SerializeField] private float npcExitDuration = 1.25f;

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

    [Tooltip("문서가 모두 접혀 있어 판정 가능한 상태일 때 OBt에 표시할 스프라이트입니다.")]
    [SerializeField] private Sprite approveReadySprite;

    [Tooltip("문서가 모두 접혀 있어 판정 가능한 상태일 때 XBt에 표시할 스프라이트입니다.")]
    [SerializeField] private Sprite rejectReadySprite;

    private NPCController shownNpc;
    private bool waitingForPleaDecision;
    private InspectionDecision currentDecision;
    private int dialogueIndex;
    private bool showingDialogueSequence;
    private Coroutine npcMoveRoutine;
    private Image approveButtonImage;
    private Image rejectButtonImage;
    private Sprite approveDefaultSprite;
    private Sprite rejectDefaultSprite;
    private bool judgementLocked;

    private void Awake()
    {
        if (npcManager == null)
            npcManager = FindFirstObjectByType<NPCManager>();

        if (documentManager == null)
            documentManager = FindFirstObjectByType<DocumentManager>();

        ResolveNpcImageRoot();
        CacheButtonImages();
    }

    private void OnEnable()
    {
        AddListeners();
        DocumentDrag.StateChanged += RefreshJudgementButtons;
        RefreshForCurrentNpc();
    }

    private void OnDisable()
    {
        DocumentDrag.StateChanged -= RefreshJudgementButtons;
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
        RefreshJudgementButtons();
    }

    public void MarkDocumentChecked()
    {
    }

    public void Approve()
    {
        if (!CanSubmitJudgement())
            return;

        if (TryAdvanceDialogue())
            return;

        Submit(true);
    }

    public void Reject()
    {
        if (!CanSubmitJudgement())
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

        judgementLocked = true;
        RefreshJudgementButtons();
        documentManager?.HideSubmittedDocuments();

        npcManager.CompleteCurrentNPC(approved);
        MoveNpcImageTo(
            GetNpcUiPosition(approved ? npcApproveExitPoint : npcRejectExitPoint, approved ? npcApproveExitPosition : npcRejectExitPosition),
            npcExitDuration
        );
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
            RefreshJudgementButtons();
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
        SetNpcImagePosition(GetNpcUiPosition(npcSpawnPoint, npcSpawnPosition));
        MoveNpcImageTo(GetNpcUiPosition(npcWaitPoint, npcWaitPosition), npcEnterDuration);
        StartDialogueSequence();
        RefreshNextNpcButton();
        RefreshJudgementButtons();
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
        if (shownNpc?.Data == null)
            return;

        string followUpNews = approved
            ? shownNpc.Data.approvedFollowUpNews
            : shownNpc.Data.rejectedFollowUpNews;

        PleaResultLog.Add(followUpNews);
    }

    private void ResetNpcUiState()
    {
        waitingForPleaDecision = false;
        currentDecision = default;
        dialogueIndex = 0;
        showingDialogueSequence = false;
        judgementLocked = false;
        RefreshNextNpcButton();
        RefreshJudgementButtons();
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

    private void CacheButtonImages()
    {
        if (approveButton != null)
        {
            approveButtonImage = approveButton.image;
            approveDefaultSprite = approveButtonImage != null ? approveButtonImage.sprite : null;
        }

        if (rejectButton != null)
        {
            rejectButtonImage = rejectButton.image;
            rejectDefaultSprite = rejectButtonImage != null ? rejectButtonImage.sprite : null;
        }
    }

    private bool CanSubmitJudgement()
    {
        return shownNpc != null
            && shownNpc.IsReady
            && !judgementLocked
            && !showingDialogueSequence
            && !IsSpeechBubbleVisible()
            && AreSubmittedDocumentsFolded();
    }

    private void RefreshJudgementButtons()
    {
        bool canSubmit = CanSubmitJudgement();

        if (approveButton != null)
            approveButton.interactable = canSubmit;

        if (rejectButton != null)
            rejectButton.interactable = canSubmit;

        SetButtonSprite(approveButtonImage, canSubmit ? approveReadySprite : approveDefaultSprite);
        SetButtonSprite(rejectButtonImage, canSubmit ? rejectReadySprite : rejectDefaultSprite);
    }

    private static void SetButtonSprite(Image image, Sprite sprite)
    {
        if (image != null && sprite != null)
            image.sprite = sprite;
    }

    private static bool AreSubmittedDocumentsFolded()
    {
        DocumentDrag[] documents = FindObjectsByType<DocumentDrag>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );

        for (int i = 0; i < documents.Length; i++)
        {
            DocumentDrag document = documents[i];

            if (document != null && document.IsShowingDocument && !document.IsFolded)
                return false;
        }

        return true;
    }

    private bool IsSpeechBubbleVisible()
    {
        return speechBubble != null && speechBubble.activeInHierarchy;
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
            StopNpcMove();
            SetNpcImagePosition(GetNpcUiPosition(npcSpawnPoint, npcSpawnPosition));
            ShowDialogue(string.Empty);
        }
    }

    private void ResolveNpcImageRoot()
    {
        if (npcImageRoot == null && npcImage != null)
            npcImageRoot = npcImage.rectTransform;
    }

    private void SetNpcImagePosition(Vector2 position)
    {
        ResolveNpcImageRoot();

        if (npcImageRoot != null)
            npcImageRoot.anchoredPosition = position;
    }

    private void MoveNpcImageTo(Vector2 targetPosition, float duration)
    {
        ResolveNpcImageRoot();

        if (npcImageRoot == null)
            return;

        StopNpcMove();
        npcMoveRoutine = StartCoroutine(AnimateNpcImageMove(targetPosition, duration));
    }

    private Vector2 GetNpcUiPosition(Transform point, Vector2 fallbackPosition)
    {
        ResolveNpcImageRoot();

        if (point == null || npcImageRoot == null || npcImageRoot.parent == null)
            return fallbackPosition;

        RectTransform parentRect = npcImageRoot.parent as RectTransform;

        if (parentRect == null)
            return fallbackPosition;

        Canvas canvas = parentRect.GetComponentInParent<Canvas>();
        Camera worldCamera = Camera.main;
        Camera uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera
            : null;

        Vector2 screenPoint = worldCamera != null
            ? RectTransformUtility.WorldToScreenPoint(worldCamera, point.position)
            : RectTransformUtility.WorldToScreenPoint(uiCamera, point.position);

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            screenPoint,
            uiCamera,
            out Vector2 localPoint
        ))
        {
            return localPoint;
        }

        return fallbackPosition;
    }

    private void StopNpcMove()
    {
        if (npcMoveRoutine == null)
            return;

        StopCoroutine(npcMoveRoutine);
        npcMoveRoutine = null;
    }

    private IEnumerator AnimateNpcImageMove(Vector2 targetPosition, float duration)
    {
        ResolveNpcImageRoot();

        if (npcImageRoot == null)
            yield break;

        Vector2 startPosition = npcImageRoot.anchoredPosition;
        float elapsed = 0f;
        float safeDuration = Mathf.Max(0.01f, duration);

        while (elapsed < safeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / safeDuration);
            float eased = Mathf.SmoothStep(0f, 1f, t);

            npcImageRoot.anchoredPosition = Vector2.LerpUnclamped(startPosition, targetPosition, eased);
            yield return null;
        }

        npcImageRoot.anchoredPosition = targetPosition;
        npcMoveRoutine = null;
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
        Keyboard keyboard = Keyboard.current;

        if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame)
            return true;

        if (speechBubble == null || !speechBubble.activeInHierarchy)
            return false;

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
