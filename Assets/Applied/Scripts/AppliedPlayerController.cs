using UnityEngine;

// 응용 버전 플레이어. 기본 버전(키네마틱 + MovePosition)과 달리 동적 Rigidbody를 속도로 움직여서
// Cube 벽에 물리적으로 막히고, 공·밀기 장치·막대에 밀려 바닥 밖으로 떨어질 수 있다.
[RequireComponent(typeof(Rigidbody))]
public class AppliedPlayerController : MonoBehaviour
{
    [SerializeField] private AppliedKeyBindings keyBindings;
    // 점프는 이 콜라이더(경기장 바닥)에 서 있을 때만 허용한다. 공·기둥 위나 공중에서는 점프할 수 없다.
    [SerializeField] private Collider arenaGround;
    [SerializeField] private float moveSpeed = 5f;

    [Header("관성 (시험값, 미확정)")]
    // 키를 누른 방향으로 빨라지는 가속도.
    [SerializeField] private float acceleration = 30f;
    // 키를 놓았을 때 멈추는 감속도. 가속도보다 작게 잡아 살짝 미끄러지게 한다.
    [SerializeField] private float deceleration = 12f;
    // 현재 진행 방향과 반대쪽 키를 누를 때(방향 전환) 쓰는 가속도.
    [SerializeField] private float turnAcceleration = 45f;

    [Header("점프 (시험값, 미확정)")]
    [SerializeField] private float jumpSpeed = 5f;
    // 바닥 접촉으로 볼 최소 법선 Y값(1이면 완전히 평평한 면).
    [SerializeField] private float groundNormalMinY = 0.7f;

    private Rigidbody rb;
    private float stunTimer;
    private bool isOnArenaGround;
    private bool jumpRequested;

    public bool IsOnArenaGround => isOnArenaGround;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        // 빠르게 움직여도 얇은 Cube 벽을 뚫지 않도록 연속 충돌 검사를 쓴다.
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    // 넉백: 순간 힘을 주고 잠깐 입력을 막아 밀려나는 움직임이 보이게 한다.
    public void Knockback(Vector3 impulse, float stunSeconds)
    {
        rb.AddForce(impulse, ForceMode.Impulse);
        stunTimer = Mathf.Max(stunTimer, stunSeconds);
    }

    // 재시작용: 위치·속도와 함께 스턴·점프 같은 일시 상태도 모두 처음으로 되돌린다.
    public void ResetState(Vector3 position)
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        // 보간이 켜진 채 순간이동하면 몇 프레임 동안 화면상 위치가 이전 자리(예: 추락 지점)에서 따라오므로 잠깐 끄고 옮긴다.
        RigidbodyInterpolation interpolation = rb.interpolation;
        rb.interpolation = RigidbodyInterpolation.None;
        rb.position = position;
        transform.position = position;
        rb.interpolation = interpolation;
        stunTimer = 0f;
        jumpRequested = false;
        isOnArenaGround = false;
    }

    // 메뉴·설정·게임 오버 중에는 GameManager가 이 컴포넌트를 꺼서 입력을 막는다.
    // 켜고 끌 때 모두 점프 요청을 지워, 설정 중에 누른 키가 닫은 뒤 점프로 실행되지 않게 한다.
    private void OnEnable()
    {
        jumpRequested = false;
    }

    private void OnDisable()
    {
        jumpRequested = false;
    }

    // 입력은 매 프레임 읽고, 실제 점프는 물리 단계(FixedUpdate)에서 처리한다.
    private void Update()
    {
        if (keyBindings != null && keyBindings.WasPressedThisFrame(AppliedKeyBindings.Action.Jump)) jumpRequested = true;
    }

    private void FixedUpdate()
    {
        bool grounded = isOnArenaGround;
        // 접촉 정보는 이번 물리 단계의 OnCollisionStay에서 다시 채운다.
        isOnArenaGround = false;

        if (stunTimer > 0f)
        {
            stunTimer -= Time.deltaTime;
            jumpRequested = false;
            return;
        }

        if (jumpRequested)
        {
            jumpRequested = false;
            if (grounded)
            {
                Vector3 v = rb.linearVelocity;
                rb.linearVelocity = new Vector3(v.x, jumpSpeed, v.z);
            }
        }

        Vector3 input = Vector3.zero;
        if (keyBindings != null)
        {
            if (keyBindings.IsPressed(AppliedKeyBindings.Action.Up)) input.z += 1f;
            if (keyBindings.IsPressed(AppliedKeyBindings.Action.Down)) input.z -= 1f;
            if (keyBindings.IsPressed(AppliedKeyBindings.Action.Right)) input.x += 1f;
            if (keyBindings.IsPressed(AppliedKeyBindings.Action.Left)) input.x -= 1f;
        }

        // 수평 속도만 목표값으로 가속시키고, 수직 속도(중력·점프·추락)는 그대로 둔다.
        Vector3 velocity = rb.linearVelocity;
        Vector3 horizontal = new Vector3(velocity.x, 0f, velocity.z);
        Vector3 target = input.normalized * moveSpeed;

        float rate;
        if (target == Vector3.zero) rate = deceleration;
        else if (Vector3.Dot(horizontal, target) < 0f) rate = turnAcceleration;
        else rate = acceleration;

        horizontal = Vector3.MoveTowards(horizontal, target, rate * Time.deltaTime);
        rb.linearVelocity = new Vector3(horizontal.x, velocity.y, horizontal.z);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (arenaGround == null || collision.collider != arenaGround) return;
        for (int i = 0; i < collision.contactCount; i++)
        {
            if (collision.GetContact(i).normal.y >= groundNormalMinY)
            {
                isOnArenaGround = true;
                return;
            }
        }
    }
}
