using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(
    fileName = "NewNews",
    menuName = "Refugees/News"
)]
public class NewsSO : ScriptableObject
{
    [Header("Basic")]
    public int day;

    public string title;

    [TextArea(5, 10)]
    public string body;

    [Header("Images")]
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
