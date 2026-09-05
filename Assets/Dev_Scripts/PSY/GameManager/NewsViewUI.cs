using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

[System.Serializable]
public class NewsImageSlot
{
    [Tooltip("NewsSO.images에서 가져올 이미지 번호입니다. 1이면 첫 번째 이미지입니다.")]
    [Min(1)]
    public int imageNumber = 1;

    [Tooltip("해당 번호의 뉴스 이미지를 표시할 UI Image입니다.")]
    public Image image;
}

public class NewsViewUI : MonoBehaviour
{
    public event System.Action ContinueRequested;

    [System.Serializable]
    public class NewsPageSlot
    {
        [Tooltip("뉴스 한 페이지 전체 오브젝트입니다.")]
        public GameObject pageRoot;

        [Tooltip("이 페이지에 헤드라인을 표시할 TMP 텍스트들입니다.")]
        public TMP_Text[] titleTexts;

        [Tooltip("이 페이지의 본문 칸들입니다. 여러 개면 본문을 순서대로 나눠 넣습니다.")]
        public TMP_Text[] bodyTexts;

        [Tooltip("이 페이지에 표시할 이미지 슬롯들입니다.")]
        public NewsImageSlot[] imageSlots;
    }

    [Header("Pages")]
    [Tooltip("뉴스 페이지 묶음입니다. 1페이지, 2페이지처럼 Panel 오브젝트를 순서대로 넣습니다.")]
    [SerializeField] private NewsPageSlot[] pages;

    [Tooltip("이전 뉴스 페이지 버튼들입니다. 페이지마다 버튼이 따로 있으면 전부 넣습니다.")]
    [SerializeField] private Button[] previousPageButtons;

    [Tooltip("다음 뉴스 페이지 버튼들입니다. 마지막 페이지에서는 심사 화면으로 넘어갑니다.")]
    [SerializeField] private Button[] nextPageButtons;

    [Header("Page Motion")]
    [Tooltip("뉴스 페이지가 바뀔 때 아래에서 올라오는 거리입니다.")]
    [SerializeField] private Vector2 pageStartOffset = new Vector2(0f, -28f);

    [Tooltip("뉴스 페이지가 서서히 나타나는 시간입니다.")]
    [SerializeField] private float pageFadeDuration = 0.6f;

    [HideInInspector]
    [SerializeField] private Button previousPageButton;

    [HideInInspector]
    [SerializeField] private Button nextPageButton;

    [Header("Text Slots")]
    [HideInInspector]
    [SerializeField] private TMP_Text titleText;

    [HideInInspector]
    [SerializeField] private TMP_Text bodyText;

    [Header("Image Slots")]
    [HideInInspector]
    [SerializeField] private NewsImageSlot[] imageSlots;

    [HideInInspector]
    [SerializeField] private Image imageSlot1;

    [HideInInspector]
    [SerializeField] private Image imageSlot2;

    [HideInInspector]
    [SerializeField] private Image imageSlot3;

    [HideInInspector]
    [SerializeField] private Image imageSlot4;

    [HideInInspector]
    [FormerlySerializedAs("newsImages")]
    [SerializeField] private Image[] extraImageSlots;

    [HideInInspector]
    [FormerlySerializedAs("newsImage")]
    [SerializeField] private Image newsImage;

    private string currentTitle;
    private string currentBody;
    private Sprite[] currentImages = System.Array.Empty<Sprite>();
    private int currentPageIndex;
    private readonly Dictionary<RectTransform, Vector2> pageBasePositions = new Dictionary<RectTransform, Vector2>();
    private Coroutine pageMotionRoutine;

    private void OnEnable()
    {
        AddListeners();
        RefreshPageVisibility();
    }

    private void OnDisable()
    {
        RemoveListeners();

        if (pageMotionRoutine != null)
        {
            StopCoroutine(pageMotionRoutine);
            pageMotionRoutine = null;
        }
    }

    private void Update()
    {
        HandlePageButtonPointerFallback();
    }

    public void Show(NewsSO news, IEnumerable<string> extraNews = null)
    {
        if (news == null)
        {
            Clear();
            return;
        }

        currentTitle = news.title;
        currentBody = BuildBody(news.body, extraNews);
        currentImages = news.GetImages();
        currentPageIndex = 0;

        if (HasPages)
            ShowPagedNews();
        else
        {
            SetText(titleText, currentTitle);
            SetText(bodyText, currentBody);
            SetImages(currentImages);
        }
    }

    public void Clear()
    {
        currentTitle = string.Empty;
        currentBody = string.Empty;
        currentImages = System.Array.Empty<Sprite>();
        currentPageIndex = 0;

        SetText(titleText, string.Empty);
        SetText(bodyText, string.Empty);
        SetImages(null);
        ClearPages();
    }

    public void ShowPreviousPage()
    {
        if (!HasPages)
            return;

        currentPageIndex = Mathf.Max(0, currentPageIndex - 1);
        ShowPagedNews();
    }

    public void ShowNextPageOrContinue()
    {
        if (!HasPages)
        {
            ContinueRequested?.Invoke();
            return;
        }

        if (currentPageIndex < pages.Length - 1)
        {
            currentPageIndex++;
            ShowPagedNews();
            return;
        }

        ContinueRequested?.Invoke();
    }

    private static string BuildBody(string body, IEnumerable<string> extraNews)
    {
        StringBuilder builder = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(body))
            builder.AppendLine(body);

        if (extraNews == null)
            return builder.ToString();

        foreach (string news in extraNews)
        {
            if (string.IsNullOrWhiteSpace(news))
                continue;

            if (builder.Length > 0)
                builder.AppendLine();

            builder.AppendLine(news);
        }

        return builder.ToString();
    }

    private static void SetText(TMP_Text text, string value)
    {
        if (text != null)
            text.text = value;
    }

    private static void SetImage(Image image, Sprite sprite)
    {
        if (image == null)
            return;

        image.sprite = sprite;
        image.enabled = sprite != null;
        image.preserveAspect = true;
    }

    private bool HasPages => pages != null && pages.Length > 0;

    private void ShowPagedNews()
    {
        RefreshPageVisibility();

        List<TMP_Text> bodySlots = GetBodySlots();
        string[] bodyParts = SplitText(currentBody, bodySlots.Count);

        int bodyIndex = 0;

        for (int i = 0; i < pages.Length; i++)
        {
            NewsPageSlot page = pages[i];

            if (page == null)
                continue;

            SetTexts(page.titleTexts, currentTitle);

            if (page.bodyTexts != null)
            {
                for (int j = 0; j < page.bodyTexts.Length; j++)
                {
                    string text = bodyIndex < bodyParts.Length ? bodyParts[bodyIndex] : string.Empty;
                    SetText(page.bodyTexts[j], text);
                    bodyIndex++;
                }
            }

            SetImages(page.imageSlots, currentImages);
        }
    }

    private void ClearPages()
    {
        if (!HasPages)
            return;

        for (int i = 0; i < pages.Length; i++)
        {
            NewsPageSlot page = pages[i];

            if (page == null)
                continue;

            SetTexts(page.titleTexts, string.Empty);
            SetTexts(page.bodyTexts, string.Empty);
            SetImages(page.imageSlots, null);
        }

        RefreshPageVisibility();
    }

    private void RefreshPageVisibility()
    {
        if (HasPages)
        {
            currentPageIndex = Mathf.Clamp(currentPageIndex, 0, pages.Length - 1);

            for (int i = 0; i < pages.Length; i++)
            {
                if (pages[i]?.pageRoot != null)
                    pages[i].pageRoot.SetActive(i == currentPageIndex);
            }

            PlayCurrentPageMotion();
        }

        bool showPrevious = HasPages && currentPageIndex > 0;
        SetButtonsActive(previousPageButtons, showPrevious);
        if (!ContainsButton(previousPageButtons, previousPageButton))
            SetButtonActive(previousPageButton, showPrevious);

        SetButtonsActive(nextPageButtons, true);
        if (!ContainsButton(nextPageButtons, nextPageButton))
            SetButtonActive(nextPageButton, true);
    }

    private List<TMP_Text> GetBodySlots()
    {
        List<TMP_Text> slots = new List<TMP_Text>();

        if (!HasPages)
        {
            AddText(slots, bodyText);
            return slots;
        }

        for (int i = 0; i < pages.Length; i++)
        {
            TMP_Text[] bodyTexts = pages[i]?.bodyTexts;

            if (bodyTexts == null)
                continue;

            for (int j = 0; j < bodyTexts.Length; j++)
                AddText(slots, bodyTexts[j]);
        }

        return slots;
    }

    private static void SetTexts(TMP_Text[] texts, string value)
    {
        if (texts == null)
            return;

        for (int i = 0; i < texts.Length; i++)
            SetText(texts[i], value);
    }

    private static void SetImages(NewsImageSlot[] slots, Sprite[] sprites)
    {
        if (slots == null)
            return;

        for (int i = 0; i < slots.Length; i++)
        {
            NewsImageSlot slot = slots[i];

            if (slot == null)
                continue;

            int spriteIndex = Mathf.Max(1, slot.imageNumber) - 1;
            Sprite sprite = sprites != null && spriteIndex < sprites.Length ? sprites[spriteIndex] : null;
            SetImage(slot.image, sprite);
        }
    }

    private static string[] SplitText(string value, int count)
    {
        if (count <= 0)
            return System.Array.Empty<string>();

        string[] parts = new string[count];

        if (count == 1 || string.IsNullOrWhiteSpace(value))
        {
            parts[0] = value ?? string.Empty;
            return parts;
        }

        string[] lines = value.Replace("\r\n", "\n").Split('\n');
        int linesPerPart = Mathf.CeilToInt(lines.Length / (float)count);

        for (int i = 0; i < count; i++)
        {
            int start = i * linesPerPart;

            if (start >= lines.Length)
            {
                parts[i] = string.Empty;
                continue;
            }

            int length = Mathf.Min(linesPerPart, lines.Length - start);
            parts[i] = string.Join("\n", SubArray(lines, start, length)).Trim();
        }

        return parts;
    }

    private static string[] SubArray(string[] values, int start, int length)
    {
        string[] result = new string[length];
        System.Array.Copy(values, start, result, 0, length);
        return result;
    }

    private void AddListeners()
    {
        RemoveListeners();

        AddButtonListeners(previousPageButtons, ShowPreviousPage);
        AddButtonListeners(nextPageButtons, ShowNextPageOrContinue);

        if (!ContainsButton(previousPageButtons, previousPageButton))
            previousPageButton?.onClick.AddListener(ShowPreviousPage);

        if (!ContainsButton(nextPageButtons, nextPageButton))
            nextPageButton?.onClick.AddListener(ShowNextPageOrContinue);
    }

    private void RemoveListeners()
    {
        RemoveButtonListeners(previousPageButtons, ShowPreviousPage);
        RemoveButtonListeners(nextPageButtons, ShowNextPageOrContinue);

        if (!ContainsButton(previousPageButtons, previousPageButton))
            previousPageButton?.onClick.RemoveListener(ShowPreviousPage);

        if (!ContainsButton(nextPageButtons, nextPageButton))
            nextPageButton?.onClick.RemoveListener(ShowNextPageOrContinue);
    }

    private void SetImages(Sprite[] sprites)
    {
        if (imageSlots != null && imageSlots.Length > 0)
        {
            for (int i = 0; i < imageSlots.Length; i++)
            {
                NewsImageSlot slot = imageSlots[i];

                if (slot == null)
                    continue;

                int spriteIndex = Mathf.Max(1, slot.imageNumber) - 1;
                Sprite sprite = sprites != null && spriteIndex < sprites.Length ? sprites[spriteIndex] : null;
                SetImage(slot.image, sprite);
            }

            return;
        }

        Image[] legacySlots = GetLegacyImageSlots();

        if (legacySlots != null && legacySlots.Length > 0)
        {
            for (int i = 0; i < legacySlots.Length; i++)
            {
                Sprite sprite = sprites != null && i < sprites.Length ? sprites[i] : null;
                SetImage(legacySlots[i], sprite);
            }

            return;
        }

        Sprite firstSprite = sprites != null && sprites.Length > 0 ? sprites[0] : null;
        SetImage(newsImage, firstSprite);
    }

    private Image[] GetLegacyImageSlots()
    {
        List<Image> slots = new List<Image>();
        AddSlot(slots, imageSlot1);
        AddSlot(slots, imageSlot2);
        AddSlot(slots, imageSlot3);
        AddSlot(slots, imageSlot4);

        if (extraImageSlots != null)
        {
            for (int i = 0; i < extraImageSlots.Length; i++)
                AddSlot(slots, extraImageSlots[i]);
        }

        return slots.ToArray();
    }

    private static void AddSlot(List<Image> slots, Image image)
    {
        if (image != null && !slots.Contains(image))
            slots.Add(image);
    }

    private static void AddText(List<TMP_Text> slots, TMP_Text text)
    {
        if (text != null && !slots.Contains(text))
            slots.Add(text);
    }

    private static void AddButtonListeners(Button[] buttons, UnityEngine.Events.UnityAction action)
    {
        if (buttons == null)
            return;

        for (int i = 0; i < buttons.Length; i++)
            buttons[i]?.onClick.AddListener(action);
    }

    private static void RemoveButtonListeners(Button[] buttons, UnityEngine.Events.UnityAction action)
    {
        if (buttons == null)
            return;

        for (int i = 0; i < buttons.Length; i++)
            buttons[i]?.onClick.RemoveListener(action);
    }

    private static void SetButtonsActive(Button[] buttons, bool active)
    {
        if (buttons == null)
            return;

        for (int i = 0; i < buttons.Length; i++)
            SetButtonActive(buttons[i], active);
    }

    private static void SetButtonActive(Button button, bool active)
    {
        if (button != null)
            button.gameObject.SetActive(active);
    }

    private static bool ContainsButton(Button[] buttons, Button button)
    {
        if (button == null || buttons == null)
            return false;

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == button)
                return true;
        }

        return false;
    }

    private void PlayCurrentPageMotion()
    {
        GameObject pageRoot = pages[currentPageIndex]?.pageRoot;

        if (pageRoot == null)
            return;

        RectTransform rectTransform = pageRoot.transform as RectTransform;

        if (rectTransform == null)
            return;

        if (!pageBasePositions.ContainsKey(rectTransform))
            pageBasePositions.Add(rectTransform, rectTransform.anchoredPosition);

        if (pageMotionRoutine != null)
            StopCoroutine(pageMotionRoutine);

        pageMotionRoutine = StartCoroutine(AnimatePageIn(pageRoot, rectTransform));
    }

    private IEnumerator AnimatePageIn(GameObject pageRoot, RectTransform rectTransform)
    {
        CanvasGroup canvasGroup = pageRoot.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = pageRoot.AddComponent<CanvasGroup>();

        Vector2 endPosition = pageBasePositions[rectTransform];
        Vector2 startPosition = endPosition + pageStartOffset;
        rectTransform.anchoredPosition = startPosition;
        canvasGroup.alpha = 0f;

        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, pageFadeDuration);

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
        pageMotionRoutine = null;
    }

    private void HandlePageButtonPointerFallback()
    {
        Mouse mouse = Mouse.current;

        if (mouse == null || !mouse.leftButton.wasPressedThisFrame)
            return;

        Vector2 screenPoint = mouse.position.ReadValue();

        if (IsPointerOnAnyButton(previousPageButtons, screenPoint))
        {
            ShowPreviousPage();
            return;
        }
    }

    private static bool IsPointerOnAnyButton(Button[] buttons, Vector2 screenPoint)
    {
        if (buttons == null)
            return false;

        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];

            if (button == null || !button.gameObject.activeInHierarchy || !button.interactable)
                continue;

            RectTransform rectTransform = button.transform as RectTransform;

            if (rectTransform == null)
                continue;

            Canvas canvas = rectTransform.GetComponentInParent<Canvas>();
            Camera camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera
                : null;

            if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPoint, camera))
                return true;
        }

        return false;
    }
}
