using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    [Header("NPC Prefab")]
    [SerializeField] private NPCController npcPrefab;

    [Header("Move Points")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform waitPoint;
    [SerializeField] private Transform approveExitPoint;
    [SerializeField] private Transform rejectExitPoint;

    public NPCController SpawnNPC(NpcCase npcCase)
    {
        if (npcCase == null)
        {
            Debug.LogError("NPC case is missing.");
            return null;
        }

        if (npcCase.npc == null)
        {
            Debug.LogError("NPC data is missing in NPC case.");
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
            npcCase,
            waitPoint,
            approveExitPoint,
            rejectExitPoint
        );

        return newNPC;
    }
}