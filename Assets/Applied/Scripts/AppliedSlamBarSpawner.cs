using System.Collections.Generic;
using UnityEngine;

// 강타 막대를 일정 간격으로 등장시킨다. 생존 시간에 따라 간격이 줄고 동시에 등장하는 막대 수가 늘어난다(상한 있음).
public class AppliedSlamBarSpawner : MonoBehaviour
{
    [SerializeField] private AppliedDifficulty difficulty;
    [SerializeField] private Transform player;
    [SerializeField] private Material barMaterial;
    [SerializeField] private Material warningMaterial;

    [Header("등장 간격·동시 개수 (시험값)")]
    [SerializeField] private float baseInterval = 6f;
    [SerializeField] private float minInterval = 2.5f;
    [SerializeField] private int startConcurrent = 1;
    [SerializeField] private int maxConcurrent = 3;
    // 이 확률로 플레이어가 서 있는 곳을 노리고, 나머지는 경기장 안 랜덤 위치를 노린다.
    [SerializeField, Range(0f, 1f)] private float targetPlayerChance = 0.5f;
    [SerializeField] private float arenaHalfSize = 5f;

    [SerializeField] private AppliedSlamSettings slamSettings = new AppliedSlamSettings();

    private readonly List<AppliedSlamBar> bars = new List<AppliedSlamBar>();
    private float timer;

    public IReadOnlyList<AppliedSlamBar> Bars => bars;

    private void Awake()
    {
        for (int i = 0; i < maxConcurrent; i++)
        {
            var bar = new GameObject($"SlamBar_{i}").AddComponent<AppliedSlamBar>();
            bar.transform.SetParent(transform, false);
            bar.Setup(slamSettings, barMaterial, warningMaterial);
            bars.Add(bar);
        }
    }

    public void ResetSpawner()
    {
        foreach (AppliedSlamBar bar in bars) bar.ResetBar();
        timer = 0f;
    }

    private void Update()
    {
        float interval = difficulty != null ? difficulty.Interval(baseInterval, minInterval) : baseInterval;
        timer += Time.deltaTime;
        if (timer < interval) return;
        timer = 0f;

        int allowed = difficulty != null ? difficulty.AllowedCount(startConcurrent, maxConcurrent) : startConcurrent;
        int active = 0;
        AppliedSlamBar idle = null;
        foreach (AppliedSlamBar bar in bars)
        {
            if (bar.IsActive) active++;
            else if (idle == null) idle = bar;
        }
        if (idle == null || active >= allowed) return;

        idle.Begin(PickTarget());
    }

    // 구역이 경기장 밖으로 나가지 않도록 중심을 안쪽으로 제한한다.
    public Vector3 PickTarget()
    {
        float limitX = arenaHalfSize - slamSettings.zoneSize.x * 0.5f;
        float limitZ = arenaHalfSize - slamSettings.zoneSize.y * 0.5f;
        Vector3 point = player != null && Random.value < targetPlayerChance
            ? player.position
            : new Vector3(Random.Range(-limitX, limitX), 0f, Random.Range(-limitZ, limitZ));
        return new Vector3(Mathf.Clamp(point.x, -limitX, limitX), 0f, Mathf.Clamp(point.z, -limitZ, limitZ));
    }
}
