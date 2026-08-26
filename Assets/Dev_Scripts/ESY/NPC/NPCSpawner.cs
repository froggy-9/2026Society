using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    [Header("NPC 프리팹")]
    [SerializeField] private NPCController npcPrefab;

    [Header("NPC 데이터")]
    [SerializeField] private NPCData[] npcPool;

    [Header("생성 위치")]
    [SerializeField] private Transform spawnPoint;

    public NPCController SpawnNPC()
    {
        if (npcPrefab == null)
        {
            Debug.LogError("NPC Prefab이 연결되지 않았습니다.");
            return null;
        }

        if (npcPool == null || npcPool.Length == 0)
        {
            Debug.LogError("NPC Pool에 NPCData가 없습니다.");
            return null;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("Spawn Point가 연결되지 않았습니다.");
            return null;
        }

        // NPC 랜덤 선택
        int randomIndex = Random.Range(0, npcPool.Length);
        NPCData selectedNPC = npcPool[randomIndex];

        // NPC 생성
        NPCController newNPC =
            Instantiate(
                npcPrefab,
                spawnPoint.position,
                spawnPoint.rotation
            );

        // NPC 데이터 연결
        newNPC.Initialize(selectedNPC);

        return newNPC;
    }
}