using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    public event System.Action QueueChanged;

    [Header("NPC Spawner")]
    [Tooltip("NPC를 실제 씬에 생성하는 스포너입니다.")]
    [SerializeField] private NPCSpawner spawner;

    [Header("Spawn Flow")]
    [Tooltip("심사 시작 직후 첫 NPC를 자동으로 부를지 여부입니다.")]
    [SerializeField] private bool spawnFirstNpcAutomatically = true;

    private readonly List<NPCData> remainingNpcs = new List<NPCData>();

    private NPCController currentNPC;

    public NPCController CurrentNPC => currentNPC;
    public bool HasQueuedNpc => remainingNpcs.Count > 0;
    public bool CanRequestNextNpc => currentNPC == null
        && HasQueuedNpc
        && RefugeesGameManager.Instance != null
        && RefugeesGameManager.Instance.CanSpawnNpc();

    private void OnEnable()
    {
        if (spawner == null)
            spawner = FindFirstObjectByType<NPCSpawner>();

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

        NPCData npcData = TakeRandomNpc();

        if (npcData == null)
        {
            Debug.Log("No NPC remains for this day.");
            return;
        }

        if (spawner == null)
        {
            Debug.LogError("NPC Spawner is missing.");
            return;
        }

        currentNPC = spawner.SpawnNPC(npcData);

        if (currentNPC == null)
            return;

        currentNPC.Exited += OnNPCExited;
        QueueChanged?.Invoke();
        Debug.Log($"NPC spawned: {currentNPC.Data.koreanName}");
    }

    public void RequestNextNPC()
    {
        if (!CanRequestNextNpc)
            return;

        SpawnNextNPC();
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
        QueueChanged?.Invoke();
    }

    private void OnGameStateChanged(GameState state)
    {
        if (state == GameState.Inspection)
        {
            if (spawnFirstNpcAutomatically)
                SpawnNextNPC();
            else
                QueueChanged?.Invoke();

            return;
        }
    }

    private void OnDayStarted(int day)
    {
        DayDataSO dayData = RefugeesGameManager.Instance.GetCurrentDayData();

        remainingNpcs.Clear();

        if (dayData?.npcTable != null)
        {
            if (day <= 1)
                dayData.npcTable.ResetRuntimeHistory();

            int randomCount = dayData.npcCount > 0
                ? dayData.npcCount
                : dayData.targetInspectionCount;

            int rejectCount = Mathf.Clamp(dayData.rejectNpcCount, 0, randomCount);
            List<int> rejectIndexes = CreateRandomIndexes(randomCount, rejectCount);

            for (int i = 0; i < randomCount; i++)
            {
                NpcFailReason failReason = rejectIndexes.Contains(i)
                    ? dayData.npcTable.PickFailReason(dayData.rejectReasons)
                    : NpcFailReason.None;

                remainingNpcs.Add(dayData.npcTable.CreateRandomNpc(
                    dayData.currentDate,
                    failReason
                ));
            }
        }

        AddSpecialNpcs(dayData);
        Shuffle(remainingNpcs);
        QueueChanged?.Invoke();
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

    private void ClearCurrentNPC()
    {
        if (currentNPC != null)
            currentNPC.Exited -= OnNPCExited;

        currentNPC = null;
    }

    private NPCData TakeRandomNpc()
    {
        if (remainingNpcs.Count == 0)
            return null;

        int index = Random.Range(0, remainingNpcs.Count);
        NPCData npcData = remainingNpcs[index];
        remainingNpcs.RemoveAt(index);
        return npcData;
    }

    private void AddSpecialNpcs(DayDataSO dayData)
    {
        if (dayData == null || dayData.specialNpcs == null)
            return;

        for (int i = 0; i < dayData.specialNpcs.Length; i++)
        {
            SpecialNpcSO specialNpc = dayData.specialNpcs[i];

            if (specialNpc == null || !specialNpc.CanAppearOnDay(dayData.day))
                continue;

            remainingNpcs.Add(specialNpc.CreateNpc());
        }
    }

    private static void Shuffle<T>(List<T> values)
    {
        if (values == null)
            return;

        for (int i = 0; i < values.Count; i++)
        {
            int swapIndex = Random.Range(i, values.Count);
            T temp = values[i];
            values[i] = values[swapIndex];
            values[swapIndex] = temp;
        }
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
