using UnityEngine;

// 폭발 범위를 보여 주는 짧은 효과. 충돌체 없는 구가 폭발 반경까지 커졌다가 사라진다.
public class AppliedExplosionFx : MonoBehaviour
{
    private float radius;
    private float duration;
    private float elapsed;

    public static void Spawn(Vector3 position, float radius, Material material, float duration = 0.3f)
    {
        GameObject fx = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        fx.name = "ExplosionFx";
        Destroy(fx.GetComponent<Collider>());
        fx.transform.position = position;
        fx.transform.localScale = Vector3.zero;
        if (material != null) fx.GetComponent<Renderer>().sharedMaterial = material;

        AppliedExplosionFx effect = fx.AddComponent<AppliedExplosionFx>();
        effect.radius = radius;
        effect.duration = duration;
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);
        // 지름 = 반경 x 2.
        transform.localScale = Vector3.one * (radius * 2f * t);
        if (t >= 1f) Destroy(gameObject);
    }
}
