using System.Collections.Generic;
using UnityEngine;

// 랜덤한 측면(동서남북)에서 넓은 판이 바닥을 가로질러 지나가며, 닿은 플레이어와 공을 진행 방향으로 한 번씩 밀쳐 낸다.
// 트리거 콜라이더라서 Cube 벽과는 부딪히지 않고 통과하며, Cube는 밀지 않는다.
[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
public class AppliedSidePusher : MonoBehaviour
{
    [SerializeField] private AppliedDifficulty difficulty;

    [Header("제안 기본값, 미확정")]
    [SerializeField] private float minDelay = 4f;
    [SerializeField] private float maxDelay = 8f;
    // 시간이 지나 간격이 줄어도 이 값보다 짧아지지 않는다.
    [SerializeField] private float delayFloor = 2f;
    [SerializeField] private float warnSeconds = 1f;
    [SerializeField] private float sweepSpeed = 3f;
    [SerializeField] private float pushImpulse = 6f;
    [SerializeField] private float pushStunSeconds = 0.3f;
    // 바닥 중심에서 출발·도착 지점까지의 거리. Ground가 10x10이므로 5보다 바깥에서 시작한다.
    [SerializeField] private float travelHalfLength = 6.5f;
    [SerializeField] private float panelHeight = 1f;

    private enum State { Idle, Warning, Sweeping }

    private Rigidbody rb;
    private BoxCollider box;
    private Renderer[] renderers;
    private State state;
    private float stateTimer;
    private Vector3 direction;
    private float travelled;
    // 한 번 지나갈 때 같은 대상을 여러 번 밀지 않도록 이미 민 대상을 기억한다.
    private readonly HashSet<Rigidbody> pushedThisPass = new HashSet<Rigidbody>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        box = GetComponent<BoxCollider>();
        box.isTrigger = true;
        renderers = GetComponentsInChildren<Renderer>();
        ResetHazard();
    }

    public void ResetHazard()
    {
        EnterIdle();
    }

    private void FixedUpdate()
    {
        stateTimer -= Time.deltaTime;
        switch (state)
        {
            case State.Idle:
                if (stateTimer <= 0f) EnterWarning();
                break;
            case State.Warning:
                if (stateTimer <= 0f) SetVisible(true, true, State.Sweeping);
                break;
            case State.Sweeping:
                travelled += sweepSpeed * Time.deltaTime;
                rb.MovePosition(StartPosition() + direction * travelled);
                if (travelled >= travelHalfLength * 2f) EnterIdle();
                break;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (state != State.Sweeping) return;

        Rigidbody otherBody = other.attachedRigidbody;
        if (otherBody == null || pushedThisPass.Contains(otherBody)) return;

        // 플레이어의 이동 코드는 매 물리 프레임 속도를 입력값으로 되돌리므로,
        // 계속 미는 힘 대신 넉백(순간 힘 + 짧은 조작 불가)으로 확실히 밀쳐 낸다.
        AppliedPlayerController player = other.GetComponent<AppliedPlayerController>();
        if (player != null)
        {
            player.Knockback(direction * pushImpulse, pushStunSeconds);
            pushedThisPass.Add(otherBody);
            return;
        }

        if (other.GetComponent<AppliedBall>() != null)
        {
            otherBody.AddForce(direction * pushImpulse, ForceMode.VelocityChange);
            pushedThisPass.Add(otherBody);
        }
    }

    private void EnterIdle()
    {
        SetVisible(false, false, State.Idle);
        float delay = Random.Range(minDelay, maxDelay);
        stateTimer = difficulty != null ? difficulty.Interval(delay, delayFloor) : delay;
    }

    // 미리 출발 위치에 판을 보여 주고(밀지 않음), warnSeconds 뒤 가로지르기 시작한다.
    private void EnterWarning()
    {
        Vector3[] sides = { Vector3.right, Vector3.left, Vector3.forward, Vector3.back };
        direction = sides[Random.Range(0, sides.Length)];
        travelled = 0f;
        pushedThisPass.Clear();
        Vector3 start = StartPosition();
        transform.SetPositionAndRotation(start, Quaternion.LookRotation(direction));
        rb.position = start;
        rb.rotation = transform.rotation;
        SetVisible(true, false, State.Warning);
        stateTimer = warnSeconds;
    }

    private Vector3 StartPosition()
    {
        return -direction * travelHalfLength + Vector3.up * panelHeight;
    }

    private void SetVisible(bool visible, bool colliderOn, State next)
    {
        foreach (Renderer r in renderers) r.enabled = visible;
        box.enabled = colliderOn;
        state = next;
    }
}
