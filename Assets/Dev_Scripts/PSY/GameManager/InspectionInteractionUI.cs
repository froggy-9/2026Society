using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InspectionInteractionUI : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private NPCManager npcManager;
    [SerializeField] private DocumentManager documentManager;

    [Header("NPC UI")]
    [SerializeField] private Image npcImage;
    [SerializeField] private TMP_Text npcNameText;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Speech Bubble")]
    [SerializeField] private GameObject speechBubble;
    [SerializeField] private RectTransform speechBubbleRect;
    [SerializeField] private Vector2 speechPadding = new Vector2(60f, 40f);
    [SerializeField] private Vector2 speechMinSize = new Vector2(360f, 110f);
    [SerializeField] private Vector2 speechMaxSize = new Vector2(800f, 260f);

    [Header("Documents")]
    [SerializeField] private Button passportButton;
    [SerializeField] private Button entryPermitButton;
    [SerializeField] private DocumentViewUI passportView;
    [SerializeField] private DocumentViewUI entryPermitView;

    [Header("Questions")]
    [SerializeField] private GameObject questionPanel;
    [SerializeField] private Button[] questionButtons;
    [SerializeField] private TMP_Text[] questionLabels;

    [Header("Judgement")]
    [SerializeField] private Button approveButton;
    [SerializeField] private Button rejectButton;
    [SerializeField] private Graphic approveHighlight;

    private NPCController shownNpc;
    private bool checkedAnyDocument;
    private bool waitingForPleaDecision;
    private bool firstLineShown;
    private bool[] usedQuestions;
    private InspectionDecision currentDecision;

    private void Awake()
    {
        if (npcManager == null)
            npcManager = FindObjectOfType<NPCManager>();

        if (documentManager == null)
            documentManager = FindObjectOfType<DocumentManager>();
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

        MarkDocumentChecked();
        passportView?.Show(documentManager != null ? documentManager.GetPassport() : null);
    }

    public void OpenEntryPermit()
    {
        if (shownNpc == null || !shownNpc.IsReady)
            return;

        MarkDocumentChecked();
        entryPermitView?.Show(documentManager != null ? documentManager.GetEntryPermit() : null);
    }

    public void MarkDocumentChecked()
    {
        checkedAnyDocument = true;
        RefreshQuestions();
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
            ShowDialogue(shownNpc.Case.pleaText);
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
        SetText(npcNameText, shownNpc.Data != null ? shownNpc.Data.npcName : string.Empty);
        SetDocumentButtons();

        if (npcImage != null)
        {
            npcImage.sprite = shownNpc.Data != null ? shownNpc.Data.portrait : null;
            npcImage.enabled = visibleNpcImageExists();
        }

        if (shownNpc.IsReady)
            ShowFirstLine();
        else
            ShowDialogue(string.Empty);

        RefreshQuestions();
    }

    private void OnShownNpcArrived(NPCController npc)
    {
        if (npc != shownNpc)
            return;

        ShowFirstLine();
        RefreshQuestions();
    }

    private void ShowFirstLine()
    {
        if (firstLineShown)
            return;

        firstLineShown = true;

        string firstLine = shownNpc?.Dialogue != null ? shownNpc.Dialogue.firstLine : string.Empty;
        ShowDialogue(firstLine);
    }

    private void RefreshQuestions()
    {
        NpcQuestion[] questions = GetQuestions();
        bool shouldShow = checkedAnyDocument
            && shownNpc != null
            && shownNpc.IsReady
            && questions != null
            && questions.Length > 0
            && !AllQuestionsUsed(questions.Length);

        if (questionPanel != null)
            questionPanel.SetActive(shouldShow);

        EnsureQuestionState(questions);

        if (questionButtons == null)
            return;

        for (int i = 0; i < questionButtons.Length; i++)
        {
            bool hasQuestion = shouldShow
                && i < questions.Length
                && usedQuestions != null
                && !usedQuestions[i];

            if (questionButtons[i] != null)
            {
                int index = i;
                questionButtons[i].gameObject.SetActive(hasQuestion);
                questionButtons[i].interactable = hasQuestion;
                questionButtons[i].onClick.RemoveAllListeners();

                if (hasQuestion)
                    questionButtons[i].onClick.AddListener(() => AskQuestion(index));
            }

            if (questionLabels != null && i < questionLabels.Length && questionLabels[i] != null)
                questionLabels[i].text = hasQuestion ? questions[i].question : string.Empty;
        }
    }

    private void AskQuestion(int index)
    {
        NpcQuestion[] questions = GetQuestions();

        if (questions == null || index < 0 || index >= questions.Length)
            return;

        EnsureQuestionState(questions);
        usedQuestions[index] = true;
        ShowDialogue(questions[index].answer);
        RefreshQuestions();
    }

    private NpcQuestion[] GetQuestions()
    {
        NpcCase npcCase = shownNpc?.Case;

        if (npcCase == null)
            return null;

        if (npcCase.questions != null && npcCase.questions.Length > 0)
            return npcCase.questions;

        PlayerQuestion[] dialogueQuestions = npcCase.dialogue != null ? npcCase.dialogue.questions : null;

        if (dialogueQuestions == null || dialogueQuestions.Length == 0)
            return null;

        NpcQuestion[] converted = new NpcQuestion[dialogueQuestions.Length];
        for (int i = 0; i < dialogueQuestions.Length; i++)
        {
            converted[i] = new NpcQuestion
            {
                question = dialogueQuestions[i].question,
                answer = dialogueQuestions[i].answer
            };
        }

        return converted;
    }

    private void EnsureQuestionState(NpcQuestion[] questions)
    {
        int length = questions != null ? questions.Length : 0;

        if (usedQuestions == null || usedQuestions.Length != length)
            usedQuestions = new bool[length];
    }

    private bool AllQuestionsUsed(int questionCount)
    {
        if (questionCount <= 0)
            return true;

        if (usedQuestions == null || usedQuestions.Length != questionCount)
            return false;

        for (int i = 0; i < usedQuestions.Length; i++)
        {
            if (!usedQuestions[i])
                return false;
        }

        return true;
    }

    private bool ShouldStartPlea()
    {
        NpcCase npcCase = shownNpc?.Case;

        if (npcCase == null || !npcCase.canPlead)
            return false;

        return Random.value <= npcCase.pleaChance;
    }

    private void StorePleaNews(bool approved)
    {
        if (!waitingForPleaDecision || shownNpc?.Case == null)
            return;

        PleaResultLog.Add(approved
            ? shownNpc.Case.approveNews
            : shownNpc.Case.rejectNews
        );
    }

    private void ResetNpcUiState()
    {
        checkedAnyDocument = false;
        waitingForPleaDecision = false;
        firstLineShown = false;
        usedQuestions = null;
        currentDecision = default;
        SetApproveHighlight(false);
        questionPanel?.SetActive(false);
        passportView?.Close();
        entryPermitView?.Close();
        HideQuestions();
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

    private void HideQuestions()
    {
        if (questionButtons == null)
            return;

        for (int i = 0; i < questionButtons.Length; i++)
        {
            if (questionButtons[i] != null)
                questionButtons[i].gameObject.SetActive(false);

            if (questionLabels != null && i < questionLabels.Length && questionLabels[i] != null)
                questionLabels[i].text = string.Empty;
        }
    }

    private void SetApproveHighlight(bool active)
    {
        if (approveHighlight != null)
            approveHighlight.enabled = active;
    }

    private void SetNpcVisible(bool visible)
    {
        if (npcImage != null)
            npcImage.enabled = visible && npcImage.sprite != null;

        if (!visible)
        {
            if (npcImage != null)
                npcImage.sprite = null;

            SetText(npcNameText, string.Empty);
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

    private bool visibleNpcImageExists()
    {
        return npcImage != null && npcImage.sprite != null;
    }

    private static void SetText(TMP_Text text, string value)
    {
        if (text != null)
            text.text = value;
    }
}
