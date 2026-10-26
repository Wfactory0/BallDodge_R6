using UnityEngine;

// 측면 구체 설정 묶음. 생성기(AppliedSideBallSpawner)가 갖고 있다가 구체를 만들 때 넘겨준다.
[System.Serializable]
public class AppliedSideBallSettings
{
    [Tooltip("예고(경로 표시) 시간 (시험값)")] public float warnSeconds = 1.2f;
    [Tooltip("날아가는 속도 (시험값)")] public float speed = 7f;
    [Tooltip("구체 지름 (시험값)")] public float diameter = 0.9f;
    [Tooltip("날아가는 높이(구체 중심) (시험값)")] public float height = 0.8f;
    [Tooltip("수평 넉백 (시험값)")] public float knockbackForce = 6f;
    [Tooltip("위쪽 넉백 (시험값)")] public float knockbackUpForce = 1.5f;
    [Tooltip("넉백 후 조작 불가 (시험값)")] public float stunSeconds = 0.35f;
    [Tooltip("최대 수명(초). 경로를 다 지나지 못해도 이 시간이 지나면 사라진다 (시험값)")] public float lifetime = 6f;
    [Tooltip("출발·도착 지점까지 경기장 중심에서의 거리")] public float travelHalfLength = 7f;
}

// 옆에서 날아오는 구체 1개. 먼저 경로를 바닥에 표시하고(예고) 출발 지점에 멈춰 있다가,
// 예고가 끝나면 경로를 따라 직선으로 날아가며 닿은 플레이어를 진행 방향으로 한 번 밀친다.
// 트리거라서 기둥·공과는 부딪히지 않고 통과한다(낙하 폭발 공과 구분).
[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
public class AppliedSideBall : MonoBehaviour
{
    private AppliedSideBallSettings settings;
    private GameObject pathMarker;
    private Rigidbody rb;
    private Vector3 direction;
    private Vector3 start;
    private float age;
    private float travelled;
    private bool hasHitPlayer;

    public bool HasHitPlayer => hasHitPlayer;
    public bool IsLaunched => age >= settings.warnSeconds;

    public void Launch(AppliedSideBallSettings sideSettings, Vector3 moveDirection, Vector3 laneOffset, GameObject marker)
    {
        settings = sideSettings;
        direction = moveDirection;
        pathMarker = marker;
        start = -direction * settings.travelHalfLength + laneOffset + Vector3.up * settings.height;

        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        GetComponent<SphereCollider>().isTrigger = true;
        transform.position = start;
        rb.position = start;
    }

    private void FixedUpdate()
    {
        if (settings == null) return;
        age += Time.deltaTime;
        if (age > settings.lifetime) { Remove(); return; }
        if (age < settings.warnSeconds) return;

        travelled += settings.speed * Time.deltaTime;
        rb.MovePosition(start + direction * travelled);
        if (travelled >= settings.travelHalfLength * 2f) Remove();
    }

    // 예고 중에 이미 겹쳐 있던 플레이어도 출발 후에 맞도록 Enter 대신 Stay로 검사하고, 한 번만 민다.
    private void OnTriggerStay(Collider other)
    {
        if (settings == null || hasHitPlayer || !IsLaunched) return;
        AppliedPlayerController player = other.GetComponent<AppliedPlayerController>();
        if (player == null) return;

        hasHitPlayer = true;
        player.Knockback(direction * settings.knockbackForce + Vector3.up * settings.knockbackUpForce, settings.stunSeconds);
    }

    private void OnDestroy()
    {
        if (pathMarker != null) Destroy(pathMarker);
    }

    private void Remove()
    {
        Destroy(gameObject);
    }
}
