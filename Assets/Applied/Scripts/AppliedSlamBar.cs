using UnityEngine;

// 강타 막대 설정 묶음. 생성기(AppliedSlamBarSpawner)가 갖고 있다가 막대를 만들 때 넘겨준다.
[System.Serializable]
public class AppliedSlamSettings
{
    [Tooltip("강타 구역 크기 X/Z. 예고 표시와 실제 타격 판정이 이 값을 함께 쓴다 (시험값)")]
    public Vector2 zoneSize = new Vector2(1.6f, 1.6f);
    [Tooltip("판정 높이. 바닥부터 이 높이까지 맞는다 (시험값)")] public float hitHeight = 2.2f;
    [Tooltip("예고(흔들림) 시간 (시험값)")] public float warnSeconds = 1.6f;
    [Tooltip("흔들림 한 번 왕복 시간. warnSeconds를 이 값으로 나눈 횟수만큼 흔든다 (시험값)")] public float swingPeriod = 0.8f;
    [Tooltip("최대 흔들림 각도 (시험값)")] public float swingAngle = 40f;
    // 카메라(높이 12, 55도 아래)에 흔들림이 보이도록 머리가 높이 약 3.75에서 흔들리게 둔다.
    [Tooltip("공중 회전축 높이 (시험값)")] public float pivotHeight = 6.5f;
    [Tooltip("막대 길이 (시험값)")] public float rodLength = 2.5f;
    [Tooltip("머리 두께 (시험값)")] public float headThickness = 0.5f;
    [Tooltip("내려찍는 시간 (시험값)")] public float slamSeconds = 0.15f;
    [Tooltip("바닥에 머무는 시간 (시험값)")] public float holdSeconds = 0.4f;
    [Tooltip("올라가는 시간 (시험값)")] public float riseSeconds = 0.5f;
    // v2 4/2.5에서 강화한 시험값. 즉사는 아니며 사망은 추락으로만 일어난다.
    [Tooltip("수평 넉백 (시험값)")] public float knockbackForce = 6f;
    [Tooltip("위쪽 넉백 (시험값)")] public float knockbackUpForce = 3f;
    [Tooltip("넉백 후 조작 불가 (시험값)")] public float stunSeconds = 0.4f;
}

// 바이킹처럼 공중에서 흔들리다가 예고된 좁은 구역을 위에서 내려찍는 막대 1개.
// 머리·막대에는 충돌체가 없고, 내려찍는 순간 예고 표시와 같은 크기의 상자 안에 있는 대상만 맞는다.
public class AppliedSlamBar : MonoBehaviour
{
    private enum State { Idle, Swinging, Slamming, Holding, Rising }

    private AppliedSlamSettings settings;
    private Transform pivot;
    private Transform marker;
    private State state = State.Idle;
    private float stateTime;
    private Vector3 target;
    private Vector3 swingAxis;
    private float dropDistance;

    public bool IsActive => state != State.Idle;
    public Vector3 Target => target;
    // 판정 상자(중심·반크기). 예고 표시도 같은 값으로 그린다.
    public Vector3 HitCenter => target + Vector3.up * (settings.hitHeight * 0.5f);
    public Vector3 HitHalfExtents => new Vector3(settings.zoneSize.x * 0.5f, settings.hitHeight * 0.5f, settings.zoneSize.y * 0.5f);
    public Vector3 MarkerScale => marker.localScale;
    public int HitCount { get; private set; }

    public void Setup(AppliedSlamSettings slamSettings, Material barMaterial, Material warningMaterial)
    {
        settings = slamSettings;

        // 회전축(pivot) 아래로 막대와 머리를 매단다. pivot을 돌리면 바이킹처럼 흔들린다.
        pivot = new GameObject("Pivot").transform;
        pivot.SetParent(transform, false);

        Transform rod = CreatePart("Rod", PrimitiveType.Cube, barMaterial, pivot);
        rod.localScale = new Vector3(0.15f, settings.rodLength, 0.15f);
        rod.localPosition = Vector3.down * (settings.rodLength * 0.5f);

        Transform head = CreatePart("Head", PrimitiveType.Cube, barMaterial, pivot);
        head.localScale = new Vector3(settings.zoneSize.x, settings.headThickness, settings.zoneSize.y);
        head.localPosition = Vector3.down * (settings.rodLength + settings.headThickness * 0.5f);

        marker = CreatePart("WarningZone", PrimitiveType.Cube, warningMaterial, transform);
        marker.localScale = new Vector3(settings.zoneSize.x, 0.02f, settings.zoneSize.y);

        // 머리 아랫면이 바닥(y=0)에 닿을 때까지 내려야 하는 거리.
        dropDistance = settings.pivotHeight - settings.rodLength - settings.headThickness;
        SetVisible(false);
    }

    public void Begin(Vector3 groundTarget)
    {
        target = new Vector3(groundTarget.x, 0f, groundTarget.z);
        swingAxis = Random.value < 0.5f ? Vector3.right : Vector3.forward;
        transform.position = target;
        marker.position = target + Vector3.up * 0.011f;
        pivot.position = target + Vector3.up * settings.pivotHeight;
        pivot.rotation = Quaternion.identity;
        HitCount = 0;
        SetVisible(true);
        Enter(State.Swinging);
    }

    public void ResetBar()
    {
        if (settings == null) return;
        SetVisible(false);
        Enter(State.Idle);
    }

    private void Update()
    {
        if (state == State.Idle) return;
        stateTime += Time.deltaTime;

        switch (state)
        {
            case State.Swinging:
            {
                // 흔들림 폭을 점점 키우며, 정수 번 왕복한 뒤 수직(각도 0)에서 끝나게 한다.
                int swings = Mathf.Max(1, Mathf.RoundToInt(settings.warnSeconds / settings.swingPeriod));
                float duration = swings * settings.swingPeriod;
                float t = Mathf.Min(stateTime, duration);
                float angle = Mathf.Sin(t / settings.swingPeriod * Mathf.PI * 2f) * settings.swingAngle * (t / duration);
                pivot.rotation = Quaternion.AngleAxis(angle, swingAxis);
                if (stateTime >= duration)
                {
                    pivot.rotation = Quaternion.identity;
                    Enter(State.Slamming);
                }
                break;
            }
            case State.Slamming:
            {
                float t = Mathf.Clamp01(stateTime / settings.slamSeconds);
                pivot.position = target + Vector3.up * (settings.pivotHeight - dropDistance * t * t);
                if (t >= 1f)
                {
                    Strike();
                    Enter(State.Holding);
                }
                break;
            }
            case State.Holding:
                if (stateTime >= settings.holdSeconds) Enter(State.Rising);
                break;
            case State.Rising:
            {
                float t = Mathf.Clamp01(stateTime / settings.riseSeconds);
                pivot.position = target + Vector3.up * (settings.pivotHeight - dropDistance * (1f - t));
                if (t >= 1f) ResetBar();
                break;
            }
        }
    }

    private void Strike()
    {
        foreach (Collider hit in Physics.OverlapBox(HitCenter, HitHalfExtents, Quaternion.identity, ~0, QueryTriggerInteraction.Ignore))
        {
            Rigidbody body = hit.attachedRigidbody;
            if (body == null || body.isKinematic) continue;

            Vector3 away = body.position - target;
            away.y = 0f;
            if (away.sqrMagnitude < 0.0001f) away = swingAxis;
            Vector3 impulse = away.normalized * settings.knockbackForce + Vector3.up * settings.knockbackUpForce;

            AppliedPlayerController player = hit.GetComponent<AppliedPlayerController>();
            if (player != null)
            {
                player.Knockback(impulse, settings.stunSeconds);
                HitCount++;
            }
            else if (hit.GetComponent<AppliedBall>() != null)
            {
                body.AddForce(impulse, ForceMode.VelocityChange);
            }
        }
    }

    private void Enter(State next)
    {
        state = next;
        stateTime = 0f;
    }

    private void SetVisible(bool visible)
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>(true)) r.enabled = visible;
    }

    private static Transform CreatePart(string partName, PrimitiveType type, Material material, Transform parent)
    {
        GameObject part = GameObject.CreatePrimitive(type);
        part.name = partName;
        Destroy(part.GetComponent<Collider>());
        if (material != null) part.GetComponent<Renderer>().sharedMaterial = material;
        part.transform.SetParent(parent, false);
        return part.transform;
    }
}
