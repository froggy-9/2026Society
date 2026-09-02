using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SaveCardMenu : MonoBehaviour
{
    public const string SelectedSlotKey = "Refugees_SelectedSlot";
    public const string IsNewGameKey = "Refugees_IsNewGame";

    [System.Serializable]
    public class SaveCard
    {
        public RectTransform card;
        public TMP_Text titleText;
        public TMP_Text infoText;
        public Image image;
    }

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "MainGameScene";

    [Header("Cards")]
    [SerializeField] private SaveCard[] cards = new SaveCard[4];

    [Header("Hover")]
    [SerializeField] private float hoverUp = 40f;
    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private float moveSpeed = 14f;

    private Vector2[] basePositions;
    private Vector3[] baseScales;
    private bool[] hovering;

    private void Awake()
    {
        int count = cards != null ? cards.Length : 0;

        basePositions = new Vector2[count];
        baseScales = new Vector3[count];
        hovering = new bool[count];

        for (int i = 0; i < count; i++)
        {
            if (cards[i].card == null)
                continue;

            basePositions[i] = cards[i].card.anchoredPosition;
            baseScales[i] = cards[i].card.localScale;

            SaveCardClick click = cards[i].card.GetComponent<SaveCardClick>();
            if (click == null)
                click = cards[i].card.gameObject.AddComponent<SaveCardClick>();

            click.Set(this, i);
        }
    }

    private void Start()
    {
        Refresh();
    }

    private void Update()
    {
        if (cards == null)
            return;

        for (int i = 0; i < cards.Length; i++)
            MoveCard(i);
    }

    public void Refresh()
    {
        if (cards == null)
            return;

        for (int i = 0; i < cards.Length; i++)
        {
            bool hasSave = HasSave(i);

            SetText(cards[i].titleText, hasSave ? $"SAVE {i + 1}" : "+");
            SetText(cards[i].infoText, hasSave ? GetSaveInfo(i) : "NEW GAME");

            if (cards[i].image != null)
                cards[i].image.enabled = hasSave && cards[i].image.sprite != null;
        }
    }

    public void EnterCard(int index)
    {
        if (!IsValid(index))
            return;

        hovering[index] = true;
    }

    public void ExitCard(int index)
    {
        if (!IsValid(index))
            return;

        hovering[index] = false;
    }

    public void ClickCard(int index)
    {
        if (!IsValid(index))
            return;

        bool isNewGame = !HasSave(index);

        PlayerPrefs.SetInt(SelectedSlotKey, index);
        PlayerPrefs.SetInt(IsNewGameKey, isNewGame ? 1 : 0);

        if (isNewGame)
            MakeNewSave(index);

        PlayerPrefs.Save();
        SceneManager.LoadScene(gameSceneName);
    }

    public static bool HasSave(int index)
    {
        return PlayerPrefs.GetInt(Key(index, "HasSave"), 0) == 1;
    }

    public static int GetSelectedSlot()
    {
        return PlayerPrefs.GetInt(SelectedSlotKey, 0);
    }

    public static bool ShouldStartNewGame()
    {
        return PlayerPrefs.GetInt(IsNewGameKey, 1) == 1;
    }

    public static int GetSavedDay(int index)
    {
        return PlayerPrefs.GetInt(Key(index, "Day"), 1);
    }

    public static int GetSavedScore(int index)
    {
        return PlayerPrefs.GetInt(Key(index, "Score"), 0);
    }

    public static void SaveProgress(int day, int score)
    {
        int index = GetSelectedSlot();

        PlayerPrefs.SetInt(Key(index, "HasSave"), 1);
        PlayerPrefs.SetInt(Key(index, "Day"), day);
        PlayerPrefs.SetInt(Key(index, "Score"), score);
        PlayerPrefs.SetString(Key(index, "SavedAt"), System.DateTime.Now.ToString("yyyy.MM.dd HH:mm"));
        PlayerPrefs.Save();
    }

    public static void ClearSave(int index)
    {
        PlayerPrefs.DeleteKey(Key(index, "HasSave"));
        PlayerPrefs.DeleteKey(Key(index, "Day"));
        PlayerPrefs.DeleteKey(Key(index, "Score"));
        PlayerPrefs.DeleteKey(Key(index, "SavedAt"));
        PlayerPrefs.Save();
    }

    private void MoveCard(int index)
    {
        if (!IsValid(index) || cards[index].card == null)
            return;

        Vector2 targetPosition = basePositions[index];
        Vector3 targetScale = baseScales[index];

        if (hovering[index])
        {
            targetPosition += Vector2.up * hoverUp;
            targetScale = baseScales[index] * hoverScale;
        }

        float t = Time.deltaTime * moveSpeed;
        cards[index].card.anchoredPosition = Vector2.Lerp(cards[index].card.anchoredPosition, targetPosition, t);
        cards[index].card.localScale = Vector3.Lerp(cards[index].card.localScale, targetScale, t);
    }

    private void MakeNewSave(int index)
    {
        PlayerPrefs.SetInt(Key(index, "HasSave"), 1);
        PlayerPrefs.SetInt(Key(index, "Day"), 1);
        PlayerPrefs.SetInt(Key(index, "Score"), 0);
        PlayerPrefs.SetString(Key(index, "SavedAt"), System.DateTime.Now.ToString("yyyy.MM.dd HH:mm"));
    }

    private string GetSaveInfo(int index)
    {
        int day = GetSavedDay(index);
        int score = GetSavedScore(index);
        string savedAt = PlayerPrefs.GetString(Key(index, "SavedAt"), string.Empty);

        if (string.IsNullOrWhiteSpace(savedAt))
            return $"DAY {day} / SCORE {score}";

        return $"DAY {day} / SCORE {score}\n{savedAt}";
    }

    private bool IsValid(int index)
    {
        return cards != null && index >= 0 && index < cards.Length;
    }

    private static string Key(int index, string name)
    {
        return $"Refugees_Save_{index}_{name}";
    }

    private static void SetText(TMP_Text text, string value)
    {
        if (text != null)
            text.text = value;
    }
}

public class SaveCardClick : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private SaveCardMenu menu;
    private int index;

    public void Set(SaveCardMenu menu, int index)
    {
        this.menu = menu;
        this.index = index;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        menu?.EnterCard(index);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        menu?.ExitCard(index);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        menu?.ClickCard(index);
    }
}
