using System.Collections.Generic;
using UnityEngine;

// 옆에서 날아오는 구체를 등장시킨다. 생존 시간에 따라 간격이 줄고 동시 개수가 늘어난다(상한 있음).
public class AppliedSideBallSpawner : MonoBehaviour
{
    [SerializeField] private AppliedDifficulty difficulty;
    // 낙하 폭발 공(분홍)과 구분되는 색의 재질.
    [SerializeField] private Material ballMaterial;
    [SerializeField] private Material pathMaterial;

    [Header("등장 간격·동시 개수 (시험값)")]
    [SerializeField] private float baseInterval = 5f;
    [SerializeField] private float minInterval = 1.8f;
    [SerializeField] private int startConcurrent = 1;
    [SerializeField] private int maxConcurrent = 3;
    // 경로가 경기장 안을 지나도록 하는 옆 방향 범위(±).
    [SerializeField] private float laneRange = 4f;

    [SerializeField] private AppliedSideBallSettings ballSettings = new AppliedSideBallSettings();

    private readonly List<AppliedSideBall> balls = new List<AppliedSideBall>();
    private float timer;

    public int ActiveCount
    {
        get
        {
            balls.RemoveAll(ball => ball == null);
            return balls.Count;
        }
    }

    public IReadOnlyList<AppliedSideBall> Balls
    {
        get
        {
            balls.RemoveAll(ball => ball == null);
            return balls;
        }
    }

    public void ResetSpawner()
    {
        foreach (AppliedSideBall ball in balls)
        {
            if (ball != null) Destroy(ball.gameObject);
        }
        balls.Clear();
        timer = 0f;
    }

    private void Update()
    {
        float interval = difficulty != null ? difficulty.Interval(baseInterval, minInterval) : baseInterval;
        timer += Time.deltaTime;
        if (timer < interval) return;
        timer = 0f;

        int allowed = difficulty != null ? difficulty.AllowedCount(startConcurrent, maxConcurrent) : startConcurrent;
        if (ActiveCount >= allowed) return;
        Spawn();
    }

    public AppliedSideBall Spawn()
    {
        Vector3[] sides = { Vector3.right, Vector3.left, Vector3.forward, Vector3.back };
        Vector3 direction = sides[Random.Range(0, sides.Length)];
        Vector3 across = Vector3.Cross(Vector3.up, direction);
        Vector3 laneOffset = across * Random.Range(-laneRange, laneRange);
        return Spawn(direction, laneOffset);
    }

    public AppliedSideBall Spawn(Vector3 direction, Vector3 laneOffset)
    {
        // 예고: 구체가 지나갈 경로를 바닥에 얇은 띠로 표시한다(폭 = 구체 지름).
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        marker.name = "SideBallPath";
        Destroy(marker.GetComponent<Collider>());
        marker.transform.SetPositionAndRotation(laneOffset + Vector3.up * 0.012f, Quaternion.LookRotation(direction));
        marker.transform.localScale = new Vector3(ballSettings.diameter, 0.02f, ballSettings.travelHalfLength * 2f);
        if (pathMaterial != null) marker.GetComponent<Renderer>().sharedMaterial = pathMaterial;

        GameObject ballGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ballGo.name = "SideBall";
        ballGo.transform.localScale = Vector3.one * ballSettings.diameter;
        if (ballMaterial != null) ballGo.GetComponent<Renderer>().sharedMaterial = ballMaterial;
        ballGo.AddComponent<Rigidbody>();
        AppliedSideBall ball = ballGo.AddComponent<AppliedSideBall>();
        ball.Launch(ballSettings, direction, laneOffset, marker);
        balls.Add(ball);
        return ball;
    }
}
