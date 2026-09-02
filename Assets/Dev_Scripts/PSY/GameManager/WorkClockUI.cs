using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorkClockUI : MonoBehaviour
{
    [Header("Time Source")]
    [SerializeField] private RefugeesGameManager gameManager;

    [Header("Work Day")]
    [SerializeField] private int startHour = 9;
    [SerializeField] private int endHour = 18;
    [SerializeField] private float fallbackDayTime = 60f;

    [Header("Clock Parts")]
    [SerializeField] private Image remainingWorkImage;
    [SerializeField] private RectTransform hourHand;
    [SerializeField] private RectTransform minuteHand;
    [SerializeField] private TMP_Text amPmText;

    private void Awake()
    {
        if (gameManager == null)
            gameManager = RefugeesGameManager.Instance;

        if (remainingWorkImage == null)
        {
            Transform fill = transform.Find("WorkClockRoot/ClockFace/RemainingWorkFill");
            if (fill != null)
                remainingWorkImage = fill.GetComponent<Image>();
        }

        if (remainingWorkImage != null)
        {
            remainingWorkImage.type = Image.Type.Filled;
            remainingWorkImage.fillMethod = Image.FillMethod.Radial360;
            remainingWorkImage.fillOrigin = (int)Image.Origin360.Left;
            remainingWorkImage.fillClockwise = true;
        }
    }

    private void Update()
    {
        if (gameManager == null)
            gameManager = RefugeesGameManager.Instance;

        Refresh();
    }

    private void Refresh()
    {
        if (gameManager == null)
            return;

        DayDataSO dayData = gameManager.GetCurrentDayData();
        bool hasDayData = dayData != null;
        float dayTime = hasDayData ? dayData.dayTime : fallbackDayTime;

        if (dayTime <= 0f)
            return;

        float remainingTime = hasDayData ? Mathf.Clamp(gameManager.RemainingTime, 0f, dayTime) : dayTime;
        float progress = Mathf.Clamp01((dayTime - remainingTime) / dayTime);
        float workHours = GetClockwiseHours(startHour, endHour);
        float simulatedHour = startHour + workHours * progress;
        float remainingHours = progress >= 1f ? 0f : Mathf.Repeat(endHour - simulatedHour, 12f);
        int hour24 = Mathf.FloorToInt(simulatedHour) % 24;
        float minute = (simulatedHour - Mathf.Floor(simulatedHour)) * 60f;
        float hour12 = simulatedHour % 12f;
        float hourAngle = hour12 * 30f;

        if (remainingWorkImage != null)
        {
            remainingWorkImage.fillAmount = Mathf.Clamp01(remainingHours / 12f);
            remainingWorkImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -90f - hourAngle);
        }

        if (hourHand != null)
            hourHand.localRotation = Quaternion.Euler(0f, 0f, -hourAngle);

        if (minuteHand != null)
            minuteHand.localRotation = Quaternion.Euler(0f, 0f, -minute * 6f);

        if (amPmText != null)
            amPmText.text = hour24 < 12 ? "AM" : "PM";
    }

    private static float GetClockwiseHours(float fromHour, float toHour)
    {
        float hours = Mathf.Repeat(toHour - fromHour, 12f);
        return Mathf.Approximately(hours, 0f) ? 12f : hours;
    }
}
