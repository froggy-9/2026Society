using UnityEngine;

public class NpcLook : MonoBehaviour
{
    [Header("Sprite")]
    [Tooltip("NPC 사진을 표시할 SpriteRenderer입니다. NPC prefab 안의 2D 사진 오브젝트를 넣습니다.")]
    [SerializeField] private SpriteRenderer photoRenderer;

    public void SetPhoto(Sprite photo)
    {
        if (photoRenderer == null)
            photoRenderer = GetComponentInChildren<SpriteRenderer>();

        if (photoRenderer == null)
            return;

        photoRenderer.sprite = photo;
        photoRenderer.enabled = photoRenderer.sprite != null;
    }
}
