using UnityEngine;

public class NPCController : MonoBehaviour
{
    public event System.Action<NPCController> Arrived;
    public event System.Action<NPCController> Exited;

    [HideInInspector]
    [SerializeField] private NPCData npcData;

    [Header("Look")]
    [SerializeField] private NpcLook npcLook;
    [SerializeField] private Transform visualRoot;

    [Header("Move")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float localMoveSpeed = 420f;
    [SerializeField] private float walkBobHeight = 0.04f;
    [SerializeField] private float walkBobSpeed = 8f;

    private Transform waitPoint;
    private Transform approveExitPoint;
    private Transform rejectExitPoint;
    private Vector3 visualStartLocalPosition;
    private Vector3 moveTarget;
    private bool moveInLocalSpace;
    private bool hasMoveTarget;
    private bool isLeaving;

    public NPCData Data => npcData;
    public bool IsReady { get; private set; }

    public void Initialize(
        NPCData npcData,
        Transform waitPoint,
        Transform approveExitPoint,
        Transform rejectExitPoint
    )
    {
        this.npcData = npcData;
        this.waitPoint = waitPoint;
        this.approveExitPoint = approveExitPoint;
        this.rejectExitPoint = rejectExitPoint;

        if (visualRoot == null)
            visualRoot = transform;

        if (npcLook == null)
            npcLook = GetComponentInChildren<NpcLook>();

        visualStartLocalPosition = visualRoot.localPosition;
        IsReady = false;
        isLeaving = false;

        npcLook?.SetPhoto(Data != null ? Data.portrait : null);

        if (waitPoint != null)
            MoveToPoint(waitPoint);
        else
            ArriveNow();
    }

    public void Approve()
    {
        LeaveTo(approveExitPoint);
    }

    public void Reject()
    {
        LeaveTo(rejectExitPoint);
    }

    private void Update()
    {
        if (!hasMoveTarget)
            return;

        MoveToTarget();
        UpdateWalkLook();
    }

    private void MoveToPoint(Transform point)
    {
        if (point == null)
            return;

        moveInLocalSpace = point.parent != null && point.parent == transform.parent;
        MoveTo(moveInLocalSpace ? point.localPosition : point.position);
    }

    private void MoveTo(Vector3 target)
    {
        moveTarget = target;
        hasMoveTarget = true;
    }

    private void LeaveTo(Transform exitPoint)
    {
        IsReady = false;
        isLeaving = true;

        if (exitPoint != null)
            MoveToPoint(exitPoint);
        else
        {
            moveInLocalSpace = false;
            MoveTo(transform.position + Vector3.right * 10f);
        }
    }

    private void MoveToTarget()
    {
        Vector3 currentPosition = moveInLocalSpace ? transform.localPosition : transform.position;
        Vector3 nextPosition = Vector3.MoveTowards(
            currentPosition,
            moveTarget,
            GetCurrentMoveSpeed() * Time.deltaTime
        );

        if (moveInLocalSpace)
            transform.localPosition = nextPosition;
        else
            transform.position = nextPosition;

        if (Vector3.Distance(nextPosition, moveTarget) > 0.01f)
            return;

        if (moveInLocalSpace)
            transform.localPosition = moveTarget;
        else
            transform.position = moveTarget;

        hasMoveTarget = false;
        ResetWalkLook();

        if (isLeaving)
        {
            Exited?.Invoke(this);
            Destroy(gameObject);
            return;
        }

        ArriveNow();
    }

    private void ArriveNow()
    {
        IsReady = true;
        Arrived?.Invoke(this);
    }

    private float GetCurrentMoveSpeed()
    {
        float speed = moveInLocalSpace ? localMoveSpeed : moveSpeed;
        return Mathf.Max(0.01f, speed);
    }

    private void UpdateWalkLook()
    {
        if (visualRoot == null || walkBobHeight <= 0f)
            return;

        Vector3 localPosition = visualStartLocalPosition;
        localPosition.y += Mathf.Sin(Time.time * walkBobSpeed) * walkBobHeight;
        visualRoot.localPosition = localPosition;
    }

    private void ResetWalkLook()
    {
        if (visualRoot != null)
            visualRoot.localPosition = visualStartLocalPosition;
    }
}
