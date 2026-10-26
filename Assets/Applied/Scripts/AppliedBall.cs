using UnityEngine;

// 응용 버전 분홍 낙하공. 플레이어에 닿으면 한 번 폭발해 반경 안의 플레이어와 다른 공을 바깥쪽으로 밀쳐 낸다.
// 즉사가 아니라 넉백이며, 사망은 기존처럼 바닥 밖으로 떨어질 때만 일어난다.
public class AppliedBall : MonoBehaviour
{
    [SerializeField] private float destroyBelowY = -10f;

    private AppliedBallSettings settings;
    private Collider arenaGround;
    private float age;
    private bool exploded;

    public void Init(AppliedBallSettings ballSettings, Collider ground)
    {
        settings = ballSettings;
        arenaGround = ground;
    }

    private void Update()
    {
        age += Time.deltaTime;
        // 수명이 다했거나 바닥 아래로 떨어진 공은 정리한다(장시간 실행 시 누적 방지).
        if (transform.position.y < destroyBelowY || (settings != null && age > settings.lifetime)) Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (settings == null || exploded) return;

        bool hitPlayer = collision.gameObject.GetComponent<AppliedPlayerController>() != null;
        bool hitGround = settings.explodeOnGround && arenaGround != null && collision.collider == arenaGround;
        if (hitPlayer || hitGround) Explode();
    }

    private void Explode()
    {
        exploded = true;
        Vector3 center = transform.position;

        foreach (Collider hit in Physics.OverlapSphere(center, settings.explosionRadius))
        {
            Rigidbody body = hit.attachedRigidbody;
            if (body == null || body.isKinematic || body.gameObject == gameObject) continue;

            Vector3 away = body.position - center;
            away.y = 0f;
            if (away.sqrMagnitude < 0.0001f) away = Random.insideUnitSphere;
            away.y = 0f;
            // 가까울수록 세게, 반경 끝에서는 절반 세기로 민다.
            float falloff = Mathf.Lerp(1f, 0.5f, Mathf.Clamp01(away.magnitude / settings.explosionRadius));
            Vector3 impulse = (away.normalized * settings.explosionForce + Vector3.up * settings.explosionUpForce) * falloff;

            AppliedPlayerController player = hit.GetComponent<AppliedPlayerController>();
            if (player != null) player.Knockback(impulse, settings.stunSeconds);
            else if (hit.GetComponent<AppliedBall>() != null) body.AddForce(impulse, ForceMode.VelocityChange);
        }

        AppliedExplosionFx.Spawn(center, settings.explosionRadius, settings.explosionMaterial);
        Destroy(gameObject);
    }
}
