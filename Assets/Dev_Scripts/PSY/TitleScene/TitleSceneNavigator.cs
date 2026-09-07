using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class TitleSceneNavigator : MonoBehaviour
{
    private enum NoticeCompleteAction
    {
        RevealTitle,
        LoadScene,
        InvokeEvent
    }

    [Header("Scenes")]
    [SerializeField] private string mainSceneName = "Dev_TestScP 2";

    [Header("Opening Notice")]
    [Tooltip("타이틀 씬 시작 시 이미 만들어 둔 StartUI 안내문을 먼저 보여줄지 여부입니다.")]
    [SerializeField] private bool playOpeningNotice = true;

    [Tooltip("이미 만들어 둔 StartUI Canvas입니다. 비워두면 씬의 StartUI를 찾아 사용합니다.")]
    [SerializeField] private Canvas introCanvas;

    [Tooltip("안내문 텍스트입니다. 비워두면 StartUI 아래의 WorningTxt를 찾아 사용합니다.")]
    [SerializeField] private TMP_Text noticeText;

    [Tooltip("켜면 아래 Fictional Notice 값으로 안내문 텍스트를 덮어씁니다. 꺼두면 씬에 만들어 둔 텍스트를 그대로 사용합니다.")]
    [SerializeField] private bool overrideNoticeText = false;

    [TextArea(3, 8)]
    [Tooltip("Override Notice Text가 켜져 있을 때만 적용할 안내문입니다.")]
    [SerializeField] private string noticeMessage =
        "본 작품에 등장하는 모든 인물, 국가, 단체 및 사건은 허구이며,\n실제와는 어떠한 관련도 없습니다.";

    [Tooltip("안내문을 그대로 보여주는 시간입니다.")]
    [SerializeField] private float noticeDisplayDuration = 2.5f;

    [Tooltip("StartUI가 서서히 사라지는 시간입니다.")]
    [SerializeField] private float noticeFadeOutDuration = 1.6f;

    [Tooltip("전환 완료 후 수행할 동작입니다. 기본값은 오버레이만 사라지고 기존 타이틀 화면을 보여줍니다.")]
    [SerializeField] private NoticeCompleteAction completeAction = NoticeCompleteAction.RevealTitle;

    [Tooltip("Complete Action이 Load Scene일 때 이동할 씬 이름입니다.")]
    [SerializeField] private string completeSceneName = "";

    [Tooltip("Complete Action이 Invoke Event일 때 호출할 이벤트입니다.")]
    [SerializeField] private UnityEvent noticeCompleted;

    private void Start()
    {
        if (playOpeningNotice)
            StartCoroutine(PlayOpeningNotice());
    }

    public void LoadMainScene()
    {
        SceneManager.LoadScene(mainSceneName);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private IEnumerator PlayOpeningNotice()
    {
        ResolveExistingIntroObjects();

        if (introCanvas == null)
        {
            Debug.LogWarning("TitleSceneNavigator: StartUI Canvas를 찾지 못해 오프닝 안내문 연출을 건너뜁니다.");
            yield break;
        }

        GameObject noticeRoot = introCanvas.gameObject;
        CanvasGroup canvasGroup = noticeRoot.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = noticeRoot.AddComponent<CanvasGroup>();

        noticeRoot.SetActive(true);
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        if (noticeText != null && overrideNoticeText)
            noticeText.text = noticeMessage;

        yield return new WaitForSecondsRealtime(Mathf.Max(0f, noticeDisplayDuration));

        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, noticeFadeOutDuration);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = Mathf.SmoothStep(0f, 1f, t);
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, eased);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        noticeRoot.SetActive(false);
        CompleteOpeningNotice();
    }

    private void CompleteOpeningNotice()
    {
        switch (completeAction)
        {
            case NoticeCompleteAction.LoadScene:
                if (!string.IsNullOrWhiteSpace(completeSceneName))
                    SceneManager.LoadScene(completeSceneName);
                break;
            case NoticeCompleteAction.InvokeEvent:
                noticeCompleted?.Invoke();
                break;
        }
    }

    private void ResolveExistingIntroObjects()
    {
        Transform startUi = FindSceneObjectByName("StartUI");

        if (startUi == null)
            return;

        if (introCanvas == null)
            introCanvas = startUi.GetComponent<Canvas>();

        if (noticeText == null)
        {
            Transform existingNotice = FindChildByName(startUi, "WorningTxt");
            if (existingNotice != null)
                noticeText = existingNotice.GetComponent<TMP_Text>();
        }

    }
    private static Transform FindSceneObjectByName(string objectName)
    {
        Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();

        for (int i = 0; i < transforms.Length; i++)
        {
            Transform candidate = transforms[i];

            if (candidate == null || candidate.name != objectName)
                continue;

            if (!candidate.gameObject.scene.IsValid())
                continue;

            return candidate;
        }

        return null;
    }

    private static Transform FindChildByName(Transform root, string childName)
    {
        if (root == null)
            return null;

        Transform[] children = root.GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < children.Length; i++)
        {
            if (children[i] != null && children[i].name == childName)
                return children[i];
        }

        return null;
    }
}
