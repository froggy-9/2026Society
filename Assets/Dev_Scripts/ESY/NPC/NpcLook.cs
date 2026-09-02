using System.Collections.Generic;
using UnityEngine;

public class NpcLook : MonoBehaviour
{
    private const int RecentPhotoLimit = 2;
    private static readonly Queue<int> recentPhotoIndexes = new Queue<int>();

    [HideInInspector]
    [SerializeField] private NpcPhotos photos;

    [Header("Sprite")]
    [SerializeField] private SpriteRenderer photoRenderer;

    [HideInInspector]
    [SerializeField] private bool randomOnStart = false;

    private void Start()
    {
        if (randomOnStart)
            PickRandomPhoto();
    }

    public static void ResetUsedLooks()
    {
        ResetUsedPhotos();
    }

    public static void ResetUsedPhotos()
    {
        recentPhotoIndexes.Clear();
    }

    public static void LoadUsedPhotos()
    {
        recentPhotoIndexes.Clear();
    }

    public void SetPhoto(Sprite photo)
    {
        if (photoRenderer == null)
            photoRenderer = GetComponentInChildren<SpriteRenderer>();

        if (photoRenderer == null)
            return;

        photoRenderer.sprite = photo;
        photoRenderer.enabled = photoRenderer.sprite != null;
    }

    public void PickRandomPhoto()
    {
        if (photoRenderer == null)
            photoRenderer = GetComponentInChildren<SpriteRenderer>();

        if (photoRenderer == null || photos == null || photos.photos == null || photos.photos.Length == 0)
            return;

        int index = PickUnusedPhotoIndex();

        if (index < 0)
        {
            Debug.LogWarning("No NPC photo remains.");
            photoRenderer.enabled = false;
            return;
        }

        SetPhoto(photos.photos[index]);
    }

    private int PickUnusedPhotoIndex()
    {
        if (photos.photos.Length == 0)
            return -1;

        int startIndex = Random.Range(0, photos.photos.Length);

        for (int offset = 0; offset < photos.photos.Length; offset++)
        {
            int index = (startIndex + offset) % photos.photos.Length;

            if (photos.photos.Length > RecentPhotoLimit && ContainsRecentPhoto(index))
                continue;

            AddRecentPhoto(index);
            return index;
        }

        return -1;
    }

    private static bool ContainsRecentPhoto(int index)
    {
        foreach (int recentIndex in recentPhotoIndexes)
        {
            if (recentIndex == index)
                return true;
        }

        return false;
    }

    private static void AddRecentPhoto(int index)
    {
        recentPhotoIndexes.Enqueue(index);

        while (recentPhotoIndexes.Count > RecentPhotoLimit)
            recentPhotoIndexes.Dequeue();
    }
}
