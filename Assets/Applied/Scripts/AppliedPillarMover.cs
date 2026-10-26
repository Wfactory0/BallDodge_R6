using UnityEngine;

// 기둥(Cube)을 경기장 안의 랜덤 목표 지점으로 천천히 이동시킨다. 회전은 AppliedRandomRotator가 따로 맡는다.
// 키네마틱 Rigidbody의 MovePosition으로 움직여 플레이어를 물리적으로 밀어낸다.
// 경기장 밖으로 나가지 않도록 목표를 범위 안에서만 고르고, 다른 기둥과 겹칠 위치로는 움직이지 않는다.
[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
public class AppliedPillarMover : MonoBehaviour
{
    [Header("시험값, 미확정")]
    [SerializeField] private float moveSpeed = 1.2f;
    // 경기장 중심 기준 이동 가능 범위(±). Ground 반폭 5에서 회전한 Cube 반대각선(약 0.71)을 뺀 값보다 작게 둔다.
    [SerializeField] private float moveRange = 4.2f;
    // 다른 기둥과 떨어져 있어야 하는 여유 간격. 플레이어 캡슐 지름(1)보다 크게 잡아 기둥 사이에 끼지 않게 한다.
    [SerializeField] private float separationPadding = 1.1f;
    [SerializeField] private float minWait = 0.5f;
    [SerializeField] private float maxWait = 2f;

    private Rigidbody rb;
    private BoxCollider box;
    private Vector3 startPosition;
    private Vector3 target;
    private float waitTimer;
    private readonly Collider[] overlapBuffer = new Collider[32];

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        box = GetComponent<BoxCollider>();
        startPosition = transform.position;
        target = startPosition;
        waitTimer = Random.Range(minWait, maxWait);
    }

    public void ResetPillar()
    {
        // 보간이 켜진 채 순간이동하면 몇 프레임 동안 화면상 위치가 이전 자리에서 따라오므로 잠깐 끄고 옮긴다.
        RigidbodyInterpolation interpolation = rb.interpolation;
        rb.interpolation = RigidbodyInterpolation.None;
        rb.position = startPosition;
        transform.position = startPosition;
        rb.interpolation = interpolation;
        target = startPosition;
        waitTimer = Random.Range(minWait, maxWait);
    }

    private void FixedUpdate()
    {
        if (waitTimer > 0f)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f) PickTarget();
            return;
        }

        Vector3 next = Vector3.MoveTowards(rb.position, target, moveSpeed * Time.deltaTime);
        if (MoveBlocked(rb.position, next))
        {
            // 다른 기둥과 부딪힐 위치면 멈추고 잠시 뒤 새 목표를 고른다.
            target = rb.position;
            waitTimer = Random.Range(minWait, maxWait);
            return;
        }

        rb.MovePosition(next);
        if ((next - target).sqrMagnitude < 0.0001f) waitTimer = Random.Range(minWait, maxWait);
    }

    private void PickTarget()
    {
        for (int attempt = 0; attempt < 8; attempt++)
        {
            Vector3 candidate = new Vector3(
                Random.Range(-moveRange, moveRange),
                startPosition.y,
                Random.Range(-moveRange, moveRange));
            if (!OverlapsOtherPillar(candidate))
            {
                target = candidate;
                return;
            }
        }
        // 빈자리를 못 찾으면 제자리에서 다시 기다린다.
        target = rb.position;
        waitTimer = Random.Range(minWait, maxWait);
    }

    // 기둥은 계속 회전하므로 상자 모양 대신 "수평 반대각선"을 반지름으로 보고,
    // 두 기둥 중심 거리가 반지름 합 + 여유보다 가까우면 겹친다고 판단한다.
    // 단, 이미 가까운 상태에서 서로 멀어지는 이동은 허용한다(처음 배치가 가까워도 멈춰 버리지 않게).
    private bool MoveBlocked(Vector3 from, Vector3 to)
    {
        float minDistance = MinPillarDistance();
        float nearestAfter = NearestPillarDistance(to);
        return nearestAfter < minDistance && nearestAfter <= NearestPillarDistance(from);
    }

    private bool OverlapsOtherPillar(Vector3 position)
    {
        return NearestPillarDistance(position) < MinPillarDistance();
    }

    private float MinPillarDistance()
    {
        return HalfDiagonal() * 2f + separationPadding;
    }

    private float NearestPillarDistance(Vector3 position)
    {
        float nearest = float.MaxValue;
        int count = Physics.OverlapSphereNonAlloc(position, MinPillarDistance(), overlapBuffer, ~0, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < count; i++)
        {
            AppliedPillarMover other = overlapBuffer[i].GetComponent<AppliedPillarMover>();
            if (other == null || other == this) continue;
            Vector3 offset = other.rb.position - position;
            offset.y = 0f;
            nearest = Mathf.Min(nearest, offset.magnitude);
        }
        return nearest;
    }

    private float HalfDiagonal()
    {
        Vector3 size = Vector3.Scale(box.size, transform.lossyScale);
        return new Vector2(size.x, size.z).magnitude * 0.5f;
    }
}
