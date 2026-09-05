using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RefugeesGameUI : MonoBehaviour
{
    [Header("Panels")]
    [Tooltip("뉴스 화면 전체 패널입니다. 뉴스/규칙 UI가 들어있는 Canvas 또는 Panel을 넣습니다.")]
    [SerializeField] private GameObject newsPanel;

    [Tooltip("심사 화면 전체 패널입니다.")]
    [SerializeField] private GameObject inspectionPanel;

    [Tooltip("하루 결과 화면 전체 패널입니다.")]
    [SerializeField] private GameObject resultPanel;

    [Tooltip("상시 열람용 규칙서 패널입니다.")]
    [SerializeField] private GameObject rulePanel;

    [Tooltip("시계, 뉴스/규칙 팝업 버튼 등 플레이 중에만 보일 BasicUI Canvas입니다.")]
    [SerializeField] private GameObject basicUiPanel;

    [Header("Day Intro UI")]
    [Tooltip("일차 시작 때 가장 먼저 뜨는 DayUI Canvas입니다.")]
    [SerializeField] private GameObject dayIntroPanel;

    [Tooltip("DayUI 안에서 현재 일차를 표시할 TMP 텍스트입니다.")]
    [SerializeField] private TMP_Text dayIntroText;

    [Tooltip("DayUI에 한 글자씩 표시할 문장 형식입니다. {0} 자리에 현재 일차가 들어갑니다. 예: DAY {0}")]
    [SerializeField] private string dayIntroTextFormat = "DAY {0}";

    [Tooltip("DayUI가 처음 켜질 때 서서히 나타나는 시간입니다.")]
    [SerializeField] private float dayIntroFadeInDuration = 0.65f;

    [Tooltip("DayUI 글자가 한 글자씩 찍히는 간격입니다.")]
    [SerializeField] private float dayIntroCharacterInterval = 0.1f;

    [Tooltip("DayUI가 유지되는 시간입니다.")]
    [SerializeField] private float dayIntroHoldDuration = 1.35f;

    [Tooltip("DayUI가 서서히 사라지는 시간입니다.")]
    [SerializeField] private float dayIntroFadeDuration = 1.4f;

    [Header("Popup Backgrounds")]
    [Tooltip("뉴스를 버튼으로 다시 열 때 숨길 뒤 배경 패널입니다. NewsPanelUI 안의 검은 전체 Panel을 넣습니다.")]
    [SerializeField] private GameObject newsPopupBackground;

    [Tooltip("규칙서를 버튼으로 다시 열 때 숨길 뒤 배경 패널입니다. RuleUI 안의 검은 전체 Panel을 넣습니다.")]
    [SerializeField] private GameObject rulePopupBackground;

    [Header("Popup Motion Roots")]
    [Tooltip("뉴스 팝업에서 실제로 밑에서 올라올 종이 UI RectTransform입니다. 보통 NewsPanelUI 안의 NewsPanel을 넣습니다.")]
    [SerializeField] private RectTransform newsPopupMotionRoot;

    [Tooltip("규칙서 팝업에서 실제로 밑에서 올라올 종이 UI RectTransform입니다. 보통 RuleUI 안의 규칙서 종이 Panel을 넣습니다.")]
    [SerializeField] private RectTransform rulePopupMotionRoot;

    [Tooltip("뉴스 팝업 버튼 RectTransform입니다. 팝업 밖 클릭 판정에서 버튼 클릭은 제외합니다.")]
    [SerializeField] private RectTransform newsPopupButtonRoot;

    [Tooltip("규칙 팝업 버튼 RectTransform입니다. 팝업 밖 클릭 판정에서 버튼 클릭은 제외합니다.")]
    [SerializeField] private RectTransform rulePopupButtonRoot;

    [Header("News UI")]
    [Tooltip("뉴스 제목/본문/이미지 슬롯을 제어하는 NewsViewUI입니다.")]
    [SerializeField] private NewsViewUI newsView;

    [Tooltip("근무 규칙 제목/본문을 제어하는 RuleListUI입니다.")]
    [SerializeField] private RuleListUI ruleListView;

    [Tooltip("일일 업무평가 화면 UI입니다.")]
    [SerializeField] private DailyEvaluationUI dailyEvaluationView;

    [Tooltip("최종 엔딩 뉴스를 표시할 전용 UI입니다.")]
    [SerializeField] private EndingNewsUI endingNewsView;

    [Header("Panel Motion")]
    [Tooltip("뉴스/규칙 팝업이 열릴 때 시작 위치 오프셋입니다.")]
    [SerializeField] private Vector2 popupStartOffset = new Vector2(0f, -120f);

    [Tooltip("결과 공문서 UI가 열릴 때 시작 위치 오프셋입니다.")]
    [SerializeField] private Vector2 resultStartOffset = new Vector2(0f, -80f);

    [Tooltip("패널 등장 애니메이션 시간입니다.")]
    [SerializeField] private float panelMotionDuration = 0.95f;

    [Tooltip("팝업이 닫힐 때 서서히 사라지는 시간입니다.")]
    [SerializeField] private float panelFadeOutDuration = 0.75f;

    [Header("Inspection HUD")]
    [Tooltip("현재 일차를 표시할 TMP 텍스트입니다.")]
    [SerializeField] private TMP_Text dayText;

    [Tooltip("남은 제한시간을 표시할 TMP 텍스트입니다.")]
    [SerializeField] private TMP_Text timerText;

    [Tooltip("현재 심사 수 / 목표 심사 수를 표시할 TMP 텍스트입니다.")]
    [SerializeField] private TMP_Text quotaText;

    [Header("Result")]
    [Tooltip("다음 날로 넘어가는 버튼입니다.")]
    [SerializeField] private Button nextDayButton;

    [Tooltip("게임을 처음부터 다시 시작하는 버튼입니다.")]
    [SerializeField] private Button restartButton;

    private RefugeesGameManager gameManager;
    private bool openedNewsAsPopup;
    private bool openedRuleAsPopup;
    private bool showingStartRule;
    private int ignoreOutsideClickUntilFrame = -1;
    private Coroutine dayIntroRoutine;
    private readonly Dictionary<RectTransform, Vector2> panelBasePositions = new Dictionary<RectTransform, Vector2>();
    private readonly Dictionary<GameObject, Coroutine> panelMotionRoutines = new Dictionary<GameObject, Coroutine>();

    private void Awake()
    {
        ResolveSceneReferences();
        RegisterGameManager();
    }

    private void OnEnable()
    {
        ResolveSceneReferences();
        RegisterGameManager();

        if (newsView != null)
        {
            newsView.ContinueRequested -= ContinueFromNews;
            newsView.ContinueRequested += ContinueFromNews;
        }

        if (gameManager != null)
        {
            gameManager.StateChanged -= RefreshState;
            gameManager.StateChanged += RefreshState;
        }

        if (nextDayButton != null)
            nextDayButton.onClick.AddListener(NextDay);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
    }

    private void OnDisable()
    {
        if (newsView != null)
            newsView.ContinueRequested -= ContinueFromNews;

        if (gameManager != null)
            gameManager.StateChanged -= RefreshState;

        if (nextDayButton != null)
            nextDayButton.onClick.RemoveListener(NextDay);

        if (restartButton != null)
            restartButton.onClick.RemoveListener(RestartGame);
    }

    private void Start()
    {
        ResolveSceneReferences();
        RegisterGameManager();

        if (gameManager != null && gameManager.CurrentState == GameState.None)
            gameManager.GameStart();

        RefreshAll();
    }

    private void Update()
    {
        if (gameManager == null)
            return;

        if (IsPopupOpen() && (WasClosePopupKeyPressed() || WasOutsidePopupClickPressed()))
            ClosePopups();
        else if (gameManager.CurrentState == GameState.News && WasNewsAdvanceInputPressed())
            ContinueFromNews();

        RefreshHud();
    }

    public void ContinueFromNews()
    {
        if (gameManager == null || gameManager.CurrentState != GameState.News)
        {
            ClosePopups();
            return;
        }

        if (!showingStartRule)
        {
            showingStartRule = true;
            FadeOutPanel(newsPanel, newsPopupMotionRoot);
            SetActive(rulePanel, true);
            SetPopupBackgroundVisible(rulePopupBackground, true, 1f);
            ruleListView?.Show(gameManager.GetCurrentRules(), gameManager.GetCurrentRuleDescription());
            PlayPanelOpenMotion(rulePanel, rulePopupMotionRoot, popupStartOffset);
            return;
        }

        gameManager.StartInspection();

        PleaResultLog.Clear();
    }

    public void OpenNewsPopup()
    {
        if (gameManager == null)
            return;

        if (openedRuleAsPopup)
        {
            FadeOutPanel(rulePanel, rulePopupMotionRoot);
            openedRuleAsPopup = false;
        }

        openedNewsAsPopup = gameManager.CurrentState != GameState.News;
        ignoreOutsideClickUntilFrame = Time.frameCount + 1;
        SetBasicUiVisible(false);
        SetPopupBackgroundVisible(newsPopupBackground, true, 0f);
        SetActive(newsPanel, true);
        RefreshNews();
        PlayPanelOpenMotion(newsPanel, newsPopupMotionRoot, popupStartOffset);
    }

    public void OpenRulePopup()
    {
        if (gameManager == null)
            return;

        if (openedNewsAsPopup)
        {
            FadeOutPanel(newsPanel, newsPopupMotionRoot);
            openedNewsAsPopup = false;
        }

        openedRuleAsPopup = true;
        ignoreOutsideClickUntilFrame = Time.frameCount + 1;
        SetBasicUiVisible(false);
        SetPopupBackgroundVisible(rulePopupBackground, true, 0f);
        SetActive(rulePanel, true);
        ruleListView?.Show(gameManager.GetCurrentRules(), gameManager.GetCurrentRuleDescription());
        PlayPanelOpenMotion(rulePanel, rulePopupMotionRoot, popupStartOffset);
    }

    public void ClosePopups()
    {
        if (openedNewsAsPopup)
            FadeOutPanel(newsPanel, newsPopupMotionRoot);

        if (openedRuleAsPopup)
            FadeOutPanel(rulePanel, rulePopupMotionRoot);

        openedNewsAsPopup = false;
        openedRuleAsPopup = false;
        SetBasicUiVisible(gameManager != null && gameManager.CurrentState == GameState.Inspection);
    }

    public void NextDay()
    {
        gameManager?.NextDay();
    }

    public void RestartGame()
    {
        gameManager?.RestartGame();
    }

    private void RefreshState(GameState state)
    {
        bool showNews = state == GameState.News;
        bool showDayIntro = state == GameState.DayIntro;

        if (showNews)
        {
            SetPopupBackgroundVisible(newsPopupBackground, true, 1f);
            SetPopupBackgroundVisible(rulePopupBackground, true, 1f);
        }

        if (showNews)
            SetActive(newsPanel, true);
        else if (newsPanel != null && newsPanel.activeSelf)
            FadeOutPanel(newsPanel, newsPopupMotionRoot);

        SetActive(inspectionPanel, state == GameState.Inspection);
        SetActive(resultPanel, state == GameState.Result);

        if (showNews)
            SetActive(rulePanel, false);
        else if (rulePanel != null && rulePanel.activeSelf)
            FadeOutPanel(rulePanel, rulePopupMotionRoot);

        SetBasicUiVisible(state == GameState.Inspection);

        openedNewsAsPopup = false;
        openedRuleAsPopup = false;
        showingStartRule = false;
        RefreshAll();

        if (showDayIntro)
            PlayDayIntro();
        else if (dayIntroPanel != null && dayIntroPanel.activeSelf)
            FadeOutPanel(dayIntroPanel);

        if (state == GameState.News)
        {
            PlayPanelOpenMotion(newsPanel, newsPopupMotionRoot, popupStartOffset);
        }

        if (state == GameState.Result)
        {
            dailyEvaluationView?.Show(gameManager);
            PlayPanelOpenMotion(resultPanel, null, resultStartOffset);
        }
        else
            dailyEvaluationView?.Hide();

        if (state == GameState.Ending)
            ShowEndingNews();
        else
            endingNewsView?.Hide();
    }

    private void RefreshAll()
    {
        RefreshNews();
        RefreshHud();
    }

    private void RefreshNews()
    {
        if (gameManager == null)
            return;

        newsView?.Show(gameManager.GetCurrentNews(), PleaResultLog.PendingNews);
        ruleListView?.Show(gameManager.GetCurrentRules(), gameManager.GetCurrentRuleDescription());
    }

    private void RefreshHud()
    {
        if (gameManager == null)
            return;

        DayDataSO dayData = gameManager.GetCurrentDayData();

        SetText(dayText, $"DAY {gameManager.CurrentDay}");
        SetText(timerText, FormatTime(gameManager.RemainingTime));

        if (dayData != null)
            SetText(quotaText, $"{gameManager.InspectedNpcCount} / {dayData.targetInspectionCount}");
    }

    private static string FormatTime(float seconds)
    {
        int totalSeconds = Mathf.CeilToInt(seconds);
        int minutes = totalSeconds / 60;
        int remain = totalSeconds % 60;
        return $"{minutes:00}:{remain:00}";
    }

    private static void SetActive(GameObject target, bool active)
    {
        if (target != null)
            target.SetActive(active);
    }

    private static void SetText(TMP_Text text, string value)
    {
        if (text != null)
            text.text = value;
    }

    private void PlayPanelOpenMotion(GameObject panel, RectTransform motionRoot, Vector2 startOffset)
    {
        if (panel == null)
            return;

        RectTransform rectTransform = motionRoot != null ? motionRoot : panel.GetComponent<RectTransform>();

        if (rectTransform == null)
            return;

        if (!panelBasePositions.ContainsKey(rectTransform))
            panelBasePositions.Add(rectTransform, rectTransform.anchoredPosition);

        if (panelMotionRoutines.TryGetValue(panel, out Coroutine routine) && routine != null)
            StopCoroutine(routine);

        panelMotionRoutines[panel] = StartCoroutine(AnimatePanelOpen(panel, rectTransform, startOffset));
    }

    private void FadeOutPanel(GameObject panel, RectTransform motionRoot = null)
    {
        if (panel == null)
            return;

        if (panelMotionRoutines.TryGetValue(panel, out Coroutine routine) && routine != null)
            StopCoroutine(routine);

        RectTransform fadeTarget = motionRoot != null ? motionRoot : panel.GetComponent<RectTransform>();
        panelMotionRoutines[panel] = StartCoroutine(AnimatePanelFadeOut(panel, fadeTarget));
    }

    private IEnumerator AnimatePanelOpen(GameObject panel, RectTransform rectTransform, Vector2 startOffset)
    {
        Vector2 endPosition = panelBasePositions[rectTransform];
        Vector2 startPosition = endPosition + startOffset;
        CanvasGroup canvasGroup = rectTransform.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = rectTransform.gameObject.AddComponent<CanvasGroup>();

        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
        rectTransform.anchoredPosition = startPosition;
        canvasGroup.alpha = 0f;

        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, panelMotionDuration);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = Mathf.SmoothStep(0f, 1f, t);

            rectTransform.anchoredPosition = Vector2.LerpUnclamped(startPosition, endPosition, eased);
            canvasGroup.alpha = eased;

            yield return null;
        }

        rectTransform.anchoredPosition = endPosition;
        canvasGroup.alpha = 1f;
        panelMotionRoutines[panel] = null;
    }

    private IEnumerator AnimatePanelFadeOut(GameObject panel, RectTransform fadeTarget)
    {
        CanvasGroup canvasGroup = fadeTarget != null ? fadeTarget.GetComponent<CanvasGroup>() : panel.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = fadeTarget != null ? fadeTarget.gameObject.AddComponent<CanvasGroup>() : panel.AddComponent<CanvasGroup>();

        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, panelFadeOutDuration);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = Mathf.SmoothStep(0f, 1f, t);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, eased);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
        SetActive(panel, false);
        panelMotionRoutines[panel] = null;
    }

    private void SetBasicUiVisible(bool visible)
    {
        SetActive(basicUiPanel, visible);
    }

    private void PlayDayIntro()
    {
        if (dayIntroRoutine != null)
            StopCoroutine(dayIntroRoutine);

        dayIntroRoutine = StartCoroutine(AnimateDayIntro());
    }

    private IEnumerator AnimateDayIntro()
    {
        if (dayIntroPanel == null)
        {
            gameManager?.BeginDayNews();
            yield break;
        }

        string introText = string.Format(dayIntroTextFormat, gameManager.CurrentDay);
        SetText(dayIntroText, string.Empty);
        SetActive(dayIntroPanel, true);
        SetActive(newsPanel, false);
        SetActive(rulePanel, false);
        SetActive(inspectionPanel, false);
        SetActive(resultPanel, false);
        SetBasicUiVisible(false);

        CanvasGroup canvasGroup = dayIntroPanel.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = dayIntroPanel.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        float fadeInElapsed = 0f;
        float fadeInDuration = Mathf.Max(0.01f, dayIntroFadeInDuration);

        while (fadeInElapsed < fadeInDuration)
        {
            fadeInElapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(fadeInElapsed / fadeInDuration);
            float eased = Mathf.SmoothStep(0f, 1f, t);
            canvasGroup.alpha = eased;
            yield return null;
        }

        canvasGroup.alpha = 1f;

        for (int i = 0; i < introText.Length; i++)
        {
            SetText(dayIntroText, introText.Substring(0, i + 1));
            yield return new WaitForSecondsRealtime(Mathf.Max(0f, dayIntroCharacterInterval));
        }

        yield return new WaitForSecondsRealtime(Mathf.Max(0f, dayIntroHoldDuration));

        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, dayIntroFadeDuration);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = Mathf.SmoothStep(0f, 1f, t);
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, eased);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        SetActive(dayIntroPanel, false);
        dayIntroRoutine = null;
        gameManager?.BeginDayNews();
    }

    private static void SetPopupBackgroundVisible(GameObject background, bool visible, float alpha = 1f)
    {
        if (background == null)
            return;

        background.SetActive(visible);

        Image image = background.GetComponent<Image>();
        if (image != null)
        {
            Color color = image.color;
            color.a = alpha;
            image.color = color;
            image.raycastTarget = visible;
        }

        CanvasGroup canvasGroup = background.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = alpha;
            canvasGroup.blocksRaycasts = visible;
            canvasGroup.interactable = visible;
        }
    }

    private bool WasNewsAdvanceInputPressed()
    {
        Keyboard keyboard = Keyboard.current;
        bool keyboardPressed = keyboard != null
            && (keyboard.enterKey.wasPressedThisFrame
                || keyboard.numpadEnterKey.wasPressedThisFrame
                || keyboard.spaceKey.wasPressedThisFrame);

        return keyboardPressed || WasStartBackgroundClickPressed();
    }

    private void RegisterGameManager()
    {
        RefugeesGameManager nextManager = RefugeesGameManager.Instance;

        if (nextManager == null || gameManager == nextManager)
            return;

        if (gameManager != null)
            gameManager.StateChanged -= RefreshState;

        gameManager = nextManager;
        gameManager.StateChanged += RefreshState;
    }

    private void ResolveSceneReferences()
    {
        if (newsView == null)
            newsView = GetComponent<NewsViewUI>();

        if (ruleListView == null)
            ruleListView = GetComponent<RuleListUI>();

        if (dailyEvaluationView == null)
            dailyEvaluationView = GetComponentInChildren<DailyEvaluationUI>(true);

        if (endingNewsView == null)
            endingNewsView = GetComponentInChildren<EndingNewsUI>(true);
    }

    private bool IsPopupOpen()
    {
        return openedNewsAsPopup || openedRuleAsPopup;
    }

    private static bool WasClosePopupKeyPressed()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return false;

        return keyboard.enterKey.wasPressedThisFrame
            || keyboard.numpadEnterKey.wasPressedThisFrame
            || keyboard.escapeKey.wasPressedThisFrame;
    }

    private bool WasOutsidePopupClickPressed()
    {
        Mouse mouse = Mouse.current;

        if (mouse == null || !mouse.leftButton.wasPressedThisFrame)
            return false;

        if (Time.frameCount <= ignoreOutsideClickUntilFrame)
            return false;

        Vector2 screenPoint = mouse.position.ReadValue();

        if (openedNewsAsPopup)
        {
            if (IsScreenPointInside(newsPopupMotionRoot, screenPoint) || IsScreenPointInside(newsPopupButtonRoot, screenPoint))
                return false;
        }

        if (openedRuleAsPopup)
        {
            if (IsScreenPointInside(rulePopupMotionRoot, screenPoint) || IsScreenPointInside(rulePopupButtonRoot, screenPoint))
                return false;
        }

        return true;
    }

    private bool WasStartBackgroundClickPressed()
    {
        Mouse mouse = Mouse.current;

        if (mouse == null || !mouse.leftButton.wasPressedThisFrame)
            return false;

        Vector2 screenPoint = mouse.position.ReadValue();
        RectTransform activeBackground = showingStartRule
            ? GetRectTransform(rulePopupBackground)
            : GetRectTransform(newsPopupBackground);
        RectTransform activePaper = showingStartRule ? rulePopupMotionRoot : newsPopupMotionRoot;

        if (!IsScreenPointInside(activeBackground, screenPoint))
            return false;

        return !IsScreenPointInside(activePaper, screenPoint);
    }

    private static RectTransform GetRectTransform(GameObject target)
    {
        return target != null ? target.GetComponent<RectTransform>() : null;
    }

    private static bool IsScreenPointInside(RectTransform target, Vector2 screenPoint)
    {
        if (target == null || !target.gameObject.activeInHierarchy)
            return false;

        Canvas canvas = target.GetComponentInParent<Canvas>();
        Camera camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;

        return RectTransformUtility.RectangleContainsScreenPoint(target, screenPoint, camera);
    }

    private void ShowEndingNews()
    {
        EvaluationManager evaluation = gameManager != null ? gameManager.GetEvaluation() : null;

        if (evaluation == null)
            return;

        SetActive(newsPanel, false);
        SetActive(rulePanel, false);
        SetActive(inspectionPanel, false);
        SetActive(resultPanel, false);
        SetBasicUiVisible(false);

        endingNewsView?.Show(evaluation.GetEndingType(), evaluation.GetEndingNewsContent());
    }
}
