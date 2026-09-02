using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class NewsViewUI : MonoBehaviour
{
    [Header("Text Slots")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;

    [Header("Image Slots")]
    [SerializeField] private Image[] newsImages;

    [HideInInspector]
    [FormerlySerializedAs("newsImage")]
    [SerializeField] private Image newsImage;

    public void Show(NewsSO news, IEnumerable<string> extraNews = null)
    {
        if (news == null)
        {
            Clear();
            return;
        }

        SetText(titleText, news.title);
        SetText(bodyText, BuildBody(news.body, extraNews));
        SetImages(news.GetImages());
    }

    public void Clear()
    {
        SetText(titleText, string.Empty);
        SetText(bodyText, string.Empty);
        SetImages(null);
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
    }

    private void SetImages(Sprite[] sprites)
    {
        if (newsImages != null && newsImages.Length > 0)
        {
            for (int i = 0; i < newsImages.Length; i++)
            {
                Sprite sprite = sprites != null && i < sprites.Length ? sprites[i] : null;
                SetImage(newsImages[i], sprite);
            }

            return;
        }

        Sprite firstSprite = sprites != null && sprites.Length > 0 ? sprites[0] : null;
        SetImage(newsImage, firstSprite);
    }
}
