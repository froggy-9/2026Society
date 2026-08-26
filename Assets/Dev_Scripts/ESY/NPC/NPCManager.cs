using UnityEngine;

public class NPCManager : MonoBehaviour
{
    [Header("NPC Spawner")]
    [SerializeField] private NPCSpawner spawner;

    [Header("생성 설정")]
    [SerializeField] private float firstSpawnDelay = 3f;

    private NPCController currentNPC;

    private void Start()
    {
        Invoke(nameof(SpawnNextNPC), firstSpawnDelay);
    }

    public void SpawnNextNPC()
    {
        if (currentNPC != null)
        {
            Debug.LogWarning("현재 NPC가 아직 존재합니다.");
            return;
        }

        currentNPC = spawner.SpawnNPC();

        if (currentNPC != null)
        {
            Debug.Log(
                $"NPC 생성: {currentNPC.Data.npcName}"
            );
        }
    }

    public void OnNPCFinished()
    {
        currentNPC = null;

        Invoke(nameof(SpawnNextNPC), 3f);
    }

    public NPCController CurrentNPC => currentNPC;
}