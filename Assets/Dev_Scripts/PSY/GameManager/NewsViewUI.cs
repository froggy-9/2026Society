using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
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

    [Header("Newspaper Text Slots")]
    [Tooltip("신문 상단 날짜 텍스트 칸입니다. NewsSO.dateText가 들어갑니다.")]
    [SerializeField] private TMP_Text dateText;

    [Tooltip("신문 상단 Day/신문기사 텍스트 칸입니다. NewsSO.dayText가 들어갑니다.")]
    [SerializeField] private TMP_Text dayText;

    [Tooltip("신문 헤드라인 텍스트 칸입니다. NewsSO.title이 들어갑니다.")]
    [SerializeField] private TMP_Text headlineText;

    [Tooltip("신문 본문 텍스트 칸입니다. 여러 개면 본문을 순서대로 나눠 넣습니다.")]
    [SerializeField] private TMP_Text[] newspaperBodyTexts;

    [Tooltip("신문 이미지 칸입니다. Image Number와 NewsSO.images 순서가 매칭됩니다.")]
    [SerializeField] private NewsImageSlot[] newspaperImageSlots;

    [Header("Newspaper Scroll")]
    [Tooltip("신문 본문 Scroll View의 ScrollRect입니다. 세로 스크롤이 필요한 신문 UI에 연결합니다.")]
    [SerializeField] private ScrollRect newspaperScrollRect;

    [Tooltip("신문 본문 Scroll View 안의 Content RectTransform입니다.")]
    [SerializeField] private RectTransform newspaperContent;

    [Tooltip("Content가 Viewport보다 작을 때 유지할 최소 높이입니다. 0이면 Viewport 높이를 기준으로 합니다.")]
    [SerializeField] private float minimumContentHeight;

    [Tooltip("본문/이미지 아래에 남길 여백입니다.")]
    [SerializeField] private float scrollBottomPadding = 48f;

    private string currentTitle;
    private string currentBody;
    private string currentDateText;
    private string currentDayText;
    private Sprite[] currentImages = System.Array.Empty<Sprite>();
    private readonly Dictionary<RectTransform, float> textBaseHeights = new Dictionary<RectTransform, float>();

    public void Show(NewsSO news, IEnumerable<string> extraNews = null)
    {
        if (news == null)
        {
            Clear();
            return;
        }

        currentTitle = news.title;
        currentBody = BuildBody(news.body, extraNews);
        currentDateText = news.dateText;
        currentDayText = news.dayText;
        currentImages = news.GetImages();

        ShowNewspaperSlots();
        RefreshNewspaperScroll();
    }

    public void Clear()
    {
        currentTitle = string.Empty;
        currentBody = string.Empty;
        currentDateText = string.Empty;
        currentDayText = string.Empty;
        currentImages = System.Array.Empty<Sprite>();

        ClearNewspaperSlots();
        RefreshNewspaperScroll();
    }

    public void ShowNextPageOrContinue()
    {
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

    private bool HasNewspaperSlots =>
        dateText != null
        || dayText != null
        || headlineText != null
        || HasAnyText(newspaperBodyTexts)
        || HasAnyImageSlot(newspaperImageSlots);

    private void ShowNewspaperSlots()
    {
        if (!HasNewspaperSlots)
            return;

        SetText(dateText, currentDateText);
        SetText(dayText, currentDayText);
        SetText(headlineText, currentTitle);

        List<TMP_Text> bodySlots = GetNewspaperBodySlots();
        string[] bodyParts = SplitText(currentBody, bodySlots.Count);

        for (int i = 0; i < bodySlots.Count; i++)
        {
            string value = i < bodyParts.Length ? bodyParts[i] : string.Empty;
            SetText(bodySlots[i], value);
        }

        SetImages(newspaperImageSlots, currentImages);
    }

    private void RefreshNewspaperScroll()
    {
        if (newspaperScrollRect == null && newspaperContent == null)
            ResolveScrollFromBodySlots();

        RectTransform content = newspaperContent;

        if (content == null)
            return;

        Canvas.ForceUpdateCanvases();
        ResizeTextsToPreferredHeight(newspaperBodyTexts);
        Canvas.ForceUpdateCanvases();

        RectTransform viewport = newspaperScrollRect != null ? newspaperScrollRect.viewport : null;
        float viewportHeight = viewport != null ? viewport.rect.height : 0f;
        float requiredHeight = Mathf.Max(minimumContentHeight, viewportHeight);

        for (int i = 0; i < content.childCount; i++)
        {
            RectTransform child = content.GetChild(i) as RectTransform;

            if (child == null || !child.gameObject.activeSelf)
                continue;

            float bottom = GetBottomDistanceFromContentTop(content, child);
            requiredHeight = Mathf.Max(requiredHeight, bottom + scrollBottomPadding);
        }

        Vector2 size = content.sizeDelta;
        size.y = requiredHeight;
        content.sizeDelta = size;
        content.anchoredPosition = new Vector2(content.anchoredPosition.x, 0f);

        if (newspaperScrollRect != null)
            newspaperScrollRect.verticalNormalizedPosition = 1f;
    }

    private void ResolveScrollFromBodySlots()
    {
        TMP_Text bodySlot = null;

        if (newspaperBodyTexts != null)
        {
            for (int i = 0; i < newspaperBodyTexts.Length; i++)
            {
                if (newspaperBodyTexts[i] != null)
                {
                    bodySlot = newspaperBodyTexts[i];
                    break;
                }
            }
        }

        if (bodySlot == null)
            return;

        newspaperScrollRect = bodySlot.GetComponentInParent<ScrollRect>();
        if (newspaperScrollRect != null)
            newspaperContent = newspaperScrollRect.content;
    }

    private void ResizeTextsToPreferredHeight(TMP_Text[] texts)
    {
        if (texts == null)
            return;

        for (int i = 0; i < texts.Length; i++)
        {
            TMP_Text text = texts[i];

            if (text == null)
                continue;

            RectTransform rectTransform = text.rectTransform;

            if (!textBaseHeights.ContainsKey(rectTransform))
                textBaseHeights.Add(rectTransform, rectTransform.sizeDelta.y);

            float baseHeight = textBaseHeights[rectTransform];
            float preferredHeight = text.GetPreferredValues(text.text, rectTransform.rect.width, 0f).y;
            Vector2 size = rectTransform.sizeDelta;
            size.y = Mathf.Max(baseHeight, preferredHeight);
            rectTransform.sizeDelta = size;
            text.ForceMeshUpdate();
        }
    }

    private static float GetBottomDistanceFromContentTop(RectTransform content, RectTransform child)
    {
        Vector3[] childCorners = new Vector3[4];
        child.GetWorldCorners(childCorners);

        float minY = float.MaxValue;

        for (int i = 0; i < childCorners.Length; i++)
        {
            Vector3 localCorner = content.InverseTransformPoint(childCorners[i]);
            minY = Mathf.Min(minY, localCorner.y);
        }

        return content.rect.yMax - minY;
    }

    private void ClearNewspaperSlots()
    {
        SetText(dateText, string.Empty);
        SetText(dayText, string.Empty);
        SetText(headlineText, string.Empty);
        SetTexts(newspaperBodyTexts, string.Empty);
        SetImages(newspaperImageSlots, null);
    }

    private List<TMP_Text> GetNewspaperBodySlots()
    {
        List<TMP_Text> slots = new List<TMP_Text>();

        if (newspaperBodyTexts == null)
            return slots;

        for (int i = 0; i < newspaperBodyTexts.Length; i++)
            AddText(slots, newspaperBodyTexts[i]);

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

    private static void AddText(List<TMP_Text> slots, TMP_Text text)
    {
        if (text != null && !slots.Contains(text))
            slots.Add(text);
    }

    private static bool HasAnyText(TMP_Text[] texts)
    {
        if (texts == null)
            return false;

        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i] != null)
                return true;
        }

        return false;
    }

    private static bool HasAnyImageSlot(NewsImageSlot[] slots)
    {
        if (slots == null)
            return false;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i]?.image != null)
                return true;
        }

        return false;
    }

}
