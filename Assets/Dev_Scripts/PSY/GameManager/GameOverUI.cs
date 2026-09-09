using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [Header("Root")]
    [Tooltip("GameOverUI 전체 오브젝트입니다. 보통 이 스크립트가 붙은 GameOverUI를 넣습니다.")]
    [SerializeField] private GameObject panelRoot;

    [Tooltip("먼저 서서히 나타날 검정 배경 Panel의 CanvasGroup입니다.")]
    [SerializeField] private CanvasGroup blackPanelGroup;

    [Header("Text")]
    [Tooltip("한 글자씩 출력될 GameOver TMP 텍스트입니다.")]
    [SerializeField] private TMP_Text gameOverText;

    [Tooltip("GameOver 텍스트에 출력할 문장입니다.")]
    [SerializeField] private string gameOverMessage = "GameOver";

    [Header("Title Button")]
    [Tooltip("타이틀로 돌아가는 버튼의 CanvasGroup입니다. 처음에는 숨겨졌다가 마지막에 서서히 나타납니다.")]
    [SerializeField] private CanvasGroup titleButtonGroup;

    [Tooltip("누르면 타이틀 화면으로 이동할 버튼입니다.")]
    [SerializeField] private Button titleButton;

    [Tooltip("돌아갈 타이틀 씬 이름입니다. Build Settings에 등록된 씬 이름과 같아야 합니다.")]
    [SerializeField] private string titleSceneName = "TitleScene";

    [Header("Motion")]
    [Tooltip("검정 배경이 서서히 나타나는 시간입니다.")]
    [SerializeField] private float blackFadeDuration = 1.2f;

    [Tooltip("GameOver 텍스트가 한 글자씩 출력되는 전체 시간입니다.")]
    [SerializeField] private float textTypeDuration = 1.35f;

    [Tooltip("텍스트가 모두 나온 뒤 버튼이 나타나기 전 기다리는 시간입니다.")]
    [SerializeField] private float buttonDelay = 0.45f;

    [Tooltip("타이틀 버튼이 서서히 나타나는 시간입니다.")]
    [SerializeField] private float buttonFadeDuration = 0.9f;

    private Coroutine sequenceRoutine;
    private bool isShowing;

    private void Awake()
    {
        if (panelRoot == null)
            panelRoot = gameObject;

        if (!isShowing)
            HideImmediate();
    }

    private void OnEnable()
    {
        if (titleButton != null)
            titleButton.onClick.AddListener(GoToTitle);
    }

    private void OnDisable()
    {
        if (titleButton != null)
            titleButton.onClick.RemoveListener(GoToTitle);
    }

    public void Show()
    {
        Show(null);
    }

    public void Show(System.Action onBackgroundCovered)
    {
        isShowing = true;

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        if (panelRoot != null)
            panelRoot.SetActive(true);

        StopSequence();

        if (blackPanelGroup != null)
        {
            blackPanelGroup.alpha = 0f;
            blackPanelGroup.blocksRaycasts = true;
            blackPanelGroup.interactable = true;
        }

        if (gameOverText != null)
        {
            gameOverText.text = gameOverMessage;
            gameOverText.maxVisibleCharacters = 0;
        }

        SetTitleButtonVisible(false);

        sequenceRoutine = StartCoroutine(PlaySequence(onBackgroundCovered));
    }

    public void HideImmediate()
    {
        isShowing = false;
        StopSequence();

        if (blackPanelGroup != null)
        {
            blackPanelGroup.alpha = 0f;
            blackPanelGroup.blocksRaycasts = false;
            blackPanelGroup.interactable = false;
        }

        if (gameOverText != null)
        {
            gameOverText.text = gameOverMessage;
            gameOverText.maxVisibleCharacters = 0;
        }

        SetTitleButtonVisible(false);

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    public void GoToTitle()
    {
        if (string.IsNullOrWhiteSpace(titleSceneName))
            return;

        Time.timeScale = 1f;
        SceneManager.LoadScene(titleSceneName);
    }

    private void SetTitleButtonVisible(bool visible)
    {
        if (titleButtonGroup != null)
        {
            titleButtonGroup.alpha = visible ? 1f : 0f;
            titleButtonGroup.blocksRaycasts = visible;
            titleButtonGroup.interactable = visible;
        }

        if (titleButton != null)
            titleButton.interactable = visible;
    }

    private IEnumerator PlaySequence(System.Action onBackgroundCovered)
    {
        if (blackPanelGroup != null)
        {
            yield return FadeCanvasGroup(blackPanelGroup, 0f, 1f, blackFadeDuration, true);
            onBackgroundCovered?.Invoke();
        }

        if (gameOverText != null)
        {
            int characterCount = gameOverMessage.Length;
            float elapsed = 0f;
            float duration = Mathf.Max(0.01f, textTypeDuration);

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                gameOverText.maxVisibleCharacters = Mathf.RoundToInt(characterCount * t);
                yield return null;
            }

            gameOverText.maxVisibleCharacters = characterCount;
        }

        yield return new WaitForSecondsRealtime(Mathf.Max(0f, buttonDelay));

        if (titleButtonGroup != null)
            yield return FadeCanvasGroup(titleButtonGroup, 0f, 1f, buttonFadeDuration, false);

        SetTitleButtonVisible(true);
        sequenceRoutine = null;
    }

    private IEnumerator FadeCanvasGroup(
        CanvasGroup canvasGroup,
        float from,
        float to,
        float duration,
        bool keepRaycast)
    {
        if (canvasGroup == null)
            yield break;

        canvasGroup.alpha = from;
        canvasGroup.blocksRaycasts = keepRaycast;
        canvasGroup.interactable = keepRaycast;

        float elapsed = 0f;
        float safeDuration = Mathf.Max(0.01f, duration);

        while (elapsed < safeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / safeDuration);
            float eased = Mathf.SmoothStep(0f, 1f, t);
            canvasGroup.alpha = Mathf.Lerp(from, to, eased);
            yield return null;
        }

        canvasGroup.alpha = to;
    }

    private void StopSequence()
    {
        if (sequenceRoutine == null)
            return;

        StopCoroutine(sequenceRoutine);
        sequenceRoutine = null;
    }
}
