using UnityEngine;

// Cube 벽을 랜덤 속도·방향으로 Y축 회전시킨다. 일정 시간마다 속도와 방향을 새로 뽑는다.
// 키네마틱 Rigidbody의 MoveRotation으로 돌려서, 회전하는 벽이 플레이어를 물리적으로 밀어낸다.
[RequireComponent(typeof(Rigidbody))]
public class AppliedRandomRotator : MonoBehaviour
{
    [SerializeField] private float minSpeed = 30f;
    [SerializeField] private float maxSpeed = 150f;
    [SerializeField] private float minChangeInterval = 2f;
    [SerializeField] private float maxChangeInterval = 5f;

    private Rigidbody rb;
    private float currentSpeed;
    private float changeTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        PickNewRotation();
    }

    private void FixedUpdate()
    {
        changeTimer -= Time.deltaTime;
        if (changeTimer <= 0f) PickNewRotation();

        Quaternion step = Quaternion.Euler(0f, currentSpeed * Time.deltaTime, 0f);
        rb.MoveRotation(step * rb.rotation);
    }

    private void PickNewRotation()
    {
        float direction = Random.value < 0.5f ? -1f : 1f;
        currentSpeed = Random.Range(minSpeed, maxSpeed) * direction;
        changeTimer = Random.Range(minChangeInterval, maxChangeInterval);
    }
}
