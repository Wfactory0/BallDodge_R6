using System.Collections.Generic;
using UnityEngine;

// 폭발 공 설정 묶음. 생성기가 갖고 있다가 공을 만들 때 넘겨준다.
[System.Serializable]
public class AppliedBallSettings
{
    [Tooltip("폭발 반경 (시험값)")] public float explosionRadius = 1.8f;
    // v2 5/2에서 강화한 시험값. 즉사는 아니며 사망은 추락으로만 일어난다.
    [Tooltip("폭발 수평 넉백 (시험값)")] public float explosionForce = 7f;
    [Tooltip("폭발 위쪽 넉백 (시험값)")] public float explosionUpForce = 3f;
    [Tooltip("넉백 후 조작 불가 시간 (시험값)")] public float stunSeconds = 0.3f;
    [Tooltip("바닥에 닿아도 폭발할지 (미확정, 기본 꺼짐)")] public bool explodeOnGround = false;
    [Tooltip("공 수명(초). 지나면 사라진다 (시험값)")] public float lifetime = 20f;
    public Material explosionMaterial;
}

// 공중 랜덤 위치에 폭발 공을 생성한다. 생존 시간에 따라 간격이 줄고, 동시에 존재하는 공 수는 상한을 둔다.
public class AppliedBallSpawner : MonoBehaviour
{
    [SerializeField] private AppliedDifficulty difficulty;
    [SerializeField] private Collider arenaGround;
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private float spawnHeight = 8f;
    [SerializeField] private float spawnRange = 4.5f;
    [SerializeField] private float ballScale = 0.8f;
    [SerializeField] private Material ballMaterial;

    [Header("시간별 증가·상한 (시험값)")]
    [SerializeField] private float minSpawnInterval = 1f;
    [SerializeField] private int startMaxBalls = 6;
    [SerializeField] private int maxBallsCap = 12;

    [SerializeField] private AppliedBallSettings ballSettings = new AppliedBallSettings();

    private readonly List<GameObject> spawnedBalls = new List<GameObject>();
    private float timer;

    public int ActiveBallCount
    {
        get
        {
            spawnedBalls.RemoveAll(ball => ball == null);
            return spawnedBalls.Count;
        }
    }

    private void Update()
    {
        float interval = difficulty != null ? difficulty.Interval(spawnInterval, minSpawnInterval) : spawnInterval;
        timer += Time.deltaTime;
        if (timer >= interval)
        {
            timer -= interval;
            SpawnBall();
        }
    }

    public void ResetSpawner()
    {
        foreach (GameObject ball in spawnedBalls)
        {
            if (ball != null) Destroy(ball);
        }
        spawnedBalls.Clear();
        timer = 0f;
    }

    private void SpawnBall()
    {
        int maxBalls = difficulty != null ? difficulty.AllowedCount(startMaxBalls, maxBallsCap) : startMaxBalls;
        // 상한에 도달하면 이번 생성은 건너뛴다(기존 공을 갑자기 없애지 않는다).
        if (ActiveBallCount >= maxBalls) return;

        Vector3 position = new Vector3(
            Random.Range(-spawnRange, spawnRange),
            spawnHeight,
            Random.Range(-spawnRange, spawnRange));

        GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ball.name = "AppliedBall";
        ball.transform.position = position;
        ball.transform.localScale = Vector3.one * ballScale;
        if (ballMaterial != null) ball.GetComponent<Renderer>().sharedMaterial = ballMaterial;

        Rigidbody rb = ball.AddComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        ball.AddComponent<AppliedBall>().Init(ballSettings, arenaGround);
        spawnedBalls.Add(ball);
    }
}
