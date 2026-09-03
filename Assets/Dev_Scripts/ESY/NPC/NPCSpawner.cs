using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    [Header("NPC Prefab")]
    [Tooltip("화면에 등장시킬 NPC 프리팹입니다. NPCController와 NpcLook이 붙어 있어야 합니다.")]
    [SerializeField] private NPCController npcPrefab;

    [Header("Move Points")]
    [Tooltip("NPC가 화면 밖 왼쪽에서 처음 생성되는 위치입니다.")]
    [SerializeField] private Transform spawnPoint;

    [Tooltip("NPC가 걸어와서 멈추는 심사 위치입니다.")]
    [SerializeField] private Transform waitPoint;

    [Tooltip("승인된 NPC가 화면 밖 오른쪽으로 나가는 위치입니다.")]
    [SerializeField] private Transform approveExitPoint;

    [Tooltip("거절된 NPC가 화면 밖 왼쪽으로 되돌아가는 위치입니다.")]
    [SerializeField] private Transform rejectExitPoint;

    public NPCController SpawnNPC(NPCData npcData)
    {
        if (npcData == null)
        {
            Debug.LogError("NPC data is missing.");
            return null;
        }

        if (npcPrefab == null)
        {
            Debug.LogError("NPC Prefab is missing.");
            return null;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("Spawn Point is missing.");
            return null;
        }

        NPCController newNPC = Instantiate(
            npcPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        newNPC.Initialize(
            npcData,
            waitPoint,
            approveExitPoint,
            rejectExitPoint
        );

        return newNPC;
    }
}
