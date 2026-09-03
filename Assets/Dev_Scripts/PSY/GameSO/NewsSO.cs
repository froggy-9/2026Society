using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(
    fileName = "NewNews",
    menuName = "Refugees/News"
)]
public class NewsSO : ScriptableObject
{
    [Header("Basic")]
    [Tooltip("몇 일차 뉴스인지 구분하기 위한 값입니다.")]
    public int day;

    [Tooltip("뉴스 헤드라인 텍스트입니다.")]
    public string title;

    [Tooltip("뉴스 본문 텍스트입니다.")]
    [TextArea(5, 10)]
    public string body;

    [Header("Images")]
    [Tooltip("뉴스에 사용할 이미지 목록입니다. NewsViewUI의 Image Slots에서 번호로 골라 표시합니다.")]
    public Sprite[] images;

    [HideInInspector]
    [FormerlySerializedAs("image")]
    public Sprite image;

    public Sprite[] GetImages()
    {
        if (images != null && images.Length > 0)
            return images;

        if (image != null)
            return new[] { image };

        return System.Array.Empty<Sprite>();
    }
}
