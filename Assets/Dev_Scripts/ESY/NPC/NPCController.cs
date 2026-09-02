using UnityEngine;

public class NPCController : MonoBehaviour
{
    public event System.Action<NPCController> Arrived;
    public event System.Action<NPCController> Exited;

    [HideInInspector]
    [SerializeField] private NpcCase npcCase;

    [Header("Look")]
    [SerializeField] private NpcLook npcLook;
    [SerializeField] private Transform visualRoot;

    [Header("Move")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float walkBobHeight = 0.04f;
    [SerializeField] private float walkBobSpeed = 8f;

    private Transform waitPoint;
    private Transform approveExitPoint;
    private Transform rejectExitPoint;
    private Vector3 visualStartLocalPosition;
    private Vector3 moveTarget;
    private bool hasMoveTarget;
    private bool isLeaving;

    public NpcCase Case => npcCase;
    public NPCData Data => npcCase != null ? npcCase.npc : null;
    public NpcDialogue Dialogue => npcCase != null ? npcCase.dialogue : null;
    public bool IsReady { get; private set; }

    public void Initialize(
        NpcCase npcCase,
        Transform waitPoint,
        Transform approveExitPoint,
        Transform rejectExitPoint
    )
    {
        this.npcCase = npcCase;
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

        if (Data != null && Data.portrait != null)
            npcLook?.SetPhoto(Data.portrait);
        else
            npcLook?.PickRandomPhoto();

        if (waitPoint != null)
            MoveTo(waitPoint.position);
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
            MoveTo(exitPoint.position);
        else
            MoveTo(transform.position + Vector3.right * 10f);
    }

    private void MoveToTarget()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            moveTarget,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, moveTarget) > 0.01f)
            return;

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
