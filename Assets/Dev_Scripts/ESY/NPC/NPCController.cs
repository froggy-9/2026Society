using UnityEngine;

public class NPCController : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 2f;

    [SerializeField] private float targetX = 0f;

    private bool isMoving = true;

    private void Update()
    {
        if (!isMoving)
            return;

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

    private void OnArrived()
    {
        Debug.Log("NPC가 검사 위치에 도착했습니다.");
    }
}