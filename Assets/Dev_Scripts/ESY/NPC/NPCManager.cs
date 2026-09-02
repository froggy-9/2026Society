using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class NPCManager : MonoBehaviour
{
    [Header("NPC Spawner")]
    [SerializeField] private NPCSpawner spawner;

    [Header("NPC Wait Time")]
    [FormerlySerializedAs("firstNpcWaitTime")]
    [FormerlySerializedAs("firstSpawnDelay")]
    [SerializeField] private Vector2 firstNpcWaitRange = new Vector2(1f, 3f);
    [FormerlySerializedAs("nextNpcWaitTime")]
    [FormerlySerializedAs("nextSpawnDelay")]
    [SerializeField] private Vector2 nextNpcWaitRange = new Vector2(1f, 3f);

    private readonly List<NpcCase> remainingNpcs = new List<NpcCase>();

    private Coroutine spawnRoutine;
    private NPCController currentNPC;

    public NPCController CurrentNPC => currentNPC;

    private void OnEnable()
    {
        if (spawner == null)
            spawner = FindObjectOfType<NPCSpawner>();

        RegisterGameManager();
    }

    private void OnDisable()
    {
        if (RefugeesGameManager.Instance != null)
        {
            RefugeesGameManager.Instance.StateChanged -= OnGameStateChanged;
            RefugeesGameManager.Instance.DayStarted -= OnDayStarted;
        }
    }

    private void Start()
    {
        RegisterGameManager();
    }

    public void SpawnNextNPC()
    {
        if (RefugeesGameManager.Instance != null && !RefugeesGameManager.Instance.CanSpawnNpc())
            return;

        if (currentNPC != null)
        {
            Debug.LogWarning("Current NPC still exists.");
            return;
        }

        NpcCase npcCase = TakeRandomNpc();

        if (npcCase == null)
        {
            Debug.Log("No NPC remains for this day.");
            return;
        }

        if (spawner == null)
        {
            Debug.LogError("NPC Spawner is missing.");
            return;
        }

        currentNPC = spawner.SpawnNPC(npcCase);

        if (currentNPC == null)
            return;

        currentNPC.Exited += OnNPCExited;
        Debug.Log($"NPC spawned: {currentNPC.Data.npcName}");
    }

    public void CompleteCurrentNPC(bool approved)
    {
        if (currentNPC == null)
            return;

        if (approved)
            currentNPC.Approve();
        else
            currentNPC.Reject();
    }

    public void OnNPCFinished()
    {
        ClearCurrentNPC();
        QueueSpawn(nextNpcWaitRange);
    }

    private void OnGameStateChanged(GameState state)
    {
        if (state == GameState.Inspection)
        {
            QueueSpawn(firstNpcWaitRange);
            return;
        }

        if (state == GameState.News || state == GameState.Result || state == GameState.GameOver)
            StopQueuedSpawn();
    }

    private void OnDayStarted(int day)
    {
        DayDataSO dayData = RefugeesGameManager.Instance.GetCurrentDayData();

        remainingNpcs.Clear();

        if (dayData?.npcs != null)
            remainingNpcs.AddRange(dayData.npcs);

        if (dayData?.npcTable != null)
        {
            if (day <= 1)
                dayData.npcTable.ResetRuntimeHistory();

            int randomCount = dayData.npcCount > 0
                ? dayData.npcCount
                : dayData.targetInspectionCount;

            int pleaCount = Mathf.Clamp(dayData.pleaNpcCount, 0, randomCount);
            int rejectCount = Mathf.Clamp(dayData.rejectNpcCount, 0, randomCount);
            List<int> pleaIndexes = CreateRandomIndexes(randomCount, pleaCount);
            List<int> rejectIndexes = CreateRandomIndexes(randomCount, rejectCount);

            for (int i = 0; i < randomCount; i++)
            {
                NpcFailReason failReason = rejectIndexes.Contains(i)
                    ? dayData.npcTable.PickFailReason(dayData.rejectReasons)
                    : NpcFailReason.None;

                remainingNpcs.Add(dayData.npcTable.CreateRandomCase(
                    dayData.currentDate,
                    pleaIndexes.Contains(i),
                    failReason
                ));
            }
        }
    }

    private List<int> CreateRandomIndexes(int maxCount, int count)
    {
        List<int> indexes = new List<int>();

        for (int i = 0; i < maxCount; i++)
            indexes.Add(i);

        for (int i = 0; i < indexes.Count; i++)
        {
            int swapIndex = Random.Range(i, indexes.Count);
            int temp = indexes[i];
            indexes[i] = indexes[swapIndex];
            indexes[swapIndex] = temp;
        }

        if (count < indexes.Count)
            indexes.RemoveRange(count, indexes.Count - count);

        return indexes;
    }

    private void OnNPCExited(NPCController npc)
    {
        if (npc != currentNPC)
            return;

        OnNPCFinished();
    }

    private void QueueSpawn(Vector2 waitRange)
    {
        StopQueuedSpawn();
        spawnRoutine = StartCoroutine(SpawnAfterWait(GetRandomWaitTime(waitRange)));
    }

    private IEnumerator SpawnAfterWait(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        spawnRoutine = null;
        SpawnNextNPC();
    }

    private float GetRandomWaitTime(Vector2 waitRange)
    {
        float min = Mathf.Min(waitRange.x, waitRange.y);
        float max = Mathf.Max(waitRange.x, waitRange.y);
        return Random.Range(min, max);
    }

    private void StopQueuedSpawn()
    {
        if (spawnRoutine == null)
            return;

        StopCoroutine(spawnRoutine);
        spawnRoutine = null;
    }

    private void ClearCurrentNPC()
    {
        if (currentNPC != null)
            currentNPC.Exited -= OnNPCExited;

        currentNPC = null;
    }

    private NpcCase TakeRandomNpc()
    {
        if (remainingNpcs.Count == 0)
            return null;

        int index = Random.Range(0, remainingNpcs.Count);
        NpcCase npcCase = remainingNpcs[index];
        remainingNpcs.RemoveAt(index);
        return npcCase;
    }

    private void RegisterGameManager()
    {
        if (RefugeesGameManager.Instance == null)
            return;

        RefugeesGameManager.Instance.StateChanged -= OnGameStateChanged;
        RefugeesGameManager.Instance.DayStarted -= OnDayStarted;
        RefugeesGameManager.Instance.StateChanged += OnGameStateChanged;
        RefugeesGameManager.Instance.DayStarted += OnDayStarted;

        if (RefugeesGameManager.Instance.GetCurrentDayData() != null)
            OnDayStarted(RefugeesGameManager.Instance.CurrentDay);
    }
}
