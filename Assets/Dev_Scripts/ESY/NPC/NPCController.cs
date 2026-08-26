using UnityEngine;

public class NPCController : MonoBehaviour
{
    [Header("NPC 데이터")]
    [SerializeField] private NPCData npcData;

    [Header("등장 위치")]
    [SerializeField] private float targetX = 0f;

    [Header("이동 속도")]
    [SerializeField] private float moveSpeed = 2f;

    [Header("퇴장 위치")]
    [SerializeField] private float exitX = 10f;

    private bool isMoving;
    private bool isExiting;

    public NPCData Data => npcData;

    public void Initialize(NPCData data)
    {
        npcData = data;
        isMoving = true;
        isExiting = false;
    }

    private void Update()
    {
        if (isMoving)
        {
            MoveToCenter();
        }
        else if (isExiting)
        {
            MoveToExit();
        }
    }

    private void MoveToCenter()
    {
        Vector3 targetPosition = new Vector3(
            targetX,
            transform.position.y,
            transform.position.z
        );

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        if (Mathf.Abs(transform.position.x - targetX) < 0.01f)
        {
            transform.position = targetPosition;
            isMoving = false;

            OnArrived();
        }
    }

    private void MoveToExit()
    {
        Vector3 targetPosition = new Vector3(
            exitX,
            transform.position.y,
            transform.position.z
        );

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        if (Mathf.Abs(transform.position.x - exitX) < 0.01f)
        {
            Destroy(gameObject);
        }
    }

    public void OnArrived()
    {
        Debug.Log($"NPC 도착: {npcData?.npcName}");
    }

    public void Approve()
    {
        Debug.Log($"NPC 승인: {npcData?.npcName}");

        exitX = 10f;
        isExiting = true;
    }

    public void Reject()
    {
        Debug.Log($"NPC 거부: {npcData?.npcName}");

        exitX = -10f;
        isExiting = true;
    }
}