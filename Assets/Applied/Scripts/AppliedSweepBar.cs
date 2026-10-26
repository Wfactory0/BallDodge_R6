using UnityEngine;

// 위에서 막대기가 랜덤 위치로 내려와, 위아래로 흔들리며 바닥을 한쪽에서 반대쪽으로 쓸고 다시 올라간다.
// 키네마틱 Rigidbody를 MovePosition으로 움직여 플레이어와 공을 물리적으로 밀어낸다.
// 키네마틱끼리는 충돌하지 않으므로 Cube 벽(키네마틱)은 그대로 통과한다.
[RequireComponent(typeof(Rigidbody))]
public class AppliedSweepBar : MonoBehaviour
{
    [SerializeField] private AppliedDifficulty difficulty;

    [Header("제안 기본값, 미확정")]
    [SerializeField] private float minDelay = 5f;
    [SerializeField] private float maxDelay = 10f;
    // 시간이 지나 간격이 줄어도 이 값보다 짧아지지 않는다.
    [SerializeField] private float delayFloor = 2.5f;
    [SerializeField] private float startHeight = 5f;
    [SerializeField] private float sweepHeight = 0.8f;
    [SerializeField] private float bobAmplitude = 0.5f;
    [SerializeField] private float bobSpeed = 4f;
    [SerializeField] private float verticalSpeed = 4f;
    [SerializeField] private float sweepSpeed = 3.5f;
    [SerializeField] private float travelHalfLength = 6.5f;
    // 막대 길이(Transform X 스케일)보다 작게 잡아 막대가 바닥 일부만 쓸고 피할 틈을 남긴다.
    [SerializeField] private float laneRange = 2.5f;

    private enum State { Idle, Descending, Sweeping, Ascending }

    private Rigidbody rb;
    private Collider[] colliders;
    private Renderer[] renderers;
    private State state;
    private float idleTimer;
    private Vector3 direction;
    private Vector3 laneOffset;
    private float travelled;
    private float height;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        colliders = GetComponentsInChildren<Collider>();
        renderers = GetComponentsInChildren<Renderer>();
        ResetHazard();
    }

    public void ResetHazard()
    {
        state = State.Idle;
        float delay = Random.Range(minDelay, maxDelay);
        idleTimer = difficulty != null ? difficulty.Interval(delay, delayFloor) : delay;
        SetVisible(false);
    }

    private void FixedUpdate()
    {
        float dt = Time.deltaTime;
        switch (state)
        {
            case State.Idle:
                idleTimer -= dt;
                if (idleTimer <= 0f) Begin();
                return;
            case State.Descending:
                height = Mathf.MoveTowards(height, sweepHeight, verticalSpeed * dt);
                if (Mathf.Approximately(height, sweepHeight)) state = State.Sweeping;
                break;
            case State.Sweeping:
                travelled += sweepSpeed * dt;
                height = sweepHeight + Mathf.Sin(travelled * bobSpeed) * bobAmplitude;
                if (travelled >= travelHalfLength * 2f) state = State.Ascending;
                break;
            case State.Ascending:
                height = Mathf.MoveTowards(height, startHeight, verticalSpeed * dt);
                if (Mathf.Approximately(height, startHeight)) ResetHazard();
                break;
        }
        rb.MovePosition(CurrentPosition());
    }

    private void Begin()
    {
        Vector3[] sides = { Vector3.right, Vector3.left, Vector3.forward, Vector3.back };
        direction = sides[Random.Range(0, sides.Length)];
        Vector3 across = Vector3.Cross(Vector3.up, direction);
        laneOffset = across * Random.Range(-laneRange, laneRange);
        travelled = 0f;
        height = startHeight;

        // 막대의 길이 방향(로컬 X)이 진행 방향과 수직이 되도록 돌린다.
        Quaternion rotation = Quaternion.LookRotation(direction);
        Vector3 position = CurrentPosition();
        transform.SetPositionAndRotation(position, rotation);
        rb.position = position;
        rb.rotation = rotation;

        state = State.Descending;
        SetVisible(true);
    }

    private Vector3 CurrentPosition()
    {
        return -direction * travelHalfLength + direction * travelled + laneOffset + Vector3.up * height;
    }

    private void SetVisible(bool visible)
    {
        foreach (Renderer r in renderers) r.enabled = visible;
        foreach (Collider c in colliders) c.enabled = visible;
    }
}
