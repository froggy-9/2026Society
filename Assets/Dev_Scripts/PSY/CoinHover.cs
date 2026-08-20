using UnityEngine;
using UnityEngine.InputSystem;

public class CoinHover : MonoBehaviour
{
    [Header("마우스 따라가기")]
    [SerializeField] private float influenceRange = 2f;
    [SerializeField] private float moveAmount = 0.15f;
    [SerializeField] private float moveSpeed = 8f;

    [Header("Gizmos")]
    [SerializeField] private bool showGizmo = true;

    private Vector3 startPosition;
    private Camera mainCamera;

    private void Start()
    {
        startPosition = transform.position;
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Mouse.current == null || mainCamera == null)
            return;

        // 마우스 위치를 월드 좌표로 변환
        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();

        Vector3 mouseWorldPosition =
            mainCamera.ScreenToWorldPoint(
                new Vector3(
                    mouseScreenPosition.x,
                    mouseScreenPosition.y,
                    Mathf.Abs(mainCamera.transform.position.z - transform.position.z)
                )
            );

        mouseWorldPosition.z = transform.position.z;

        // 동전과 마우스 사이 거리
        float distance = Vector2.Distance(
            transform.position,
            mouseWorldPosition
        );

        Vector3 targetPosition = startPosition;

        // 감지 범위 안에 있을 때
        if (distance <= influenceRange)
        {
            Vector2 direction =
                mouseWorldPosition - transform.position;

            if (direction.sqrMagnitude > 0.001f)
            {
                direction.Normalize();

                // 가까울수록 더 많이 움직임
                float influence =
                    1f - (distance / influenceRange);

                targetPosition =
                    startPosition +
                    (Vector3)(direction * moveAmount * influence);
            }
        }

        // 목표 위치로 부드럽게 이동
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            Time.deltaTime * moveSpeed
        );
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmo)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            transform.position,
            influenceRange
        );
    }
}