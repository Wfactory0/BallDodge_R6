using UnityEngine;

// 생존 시간이 길어질수록 기믹 발생 간격을 줄이고 동시 개수 한도를 늘린다.
// 각 기믹은 Interval()로 줄어든 간격을, AllowedCount()로 현재 허용 개수를 받아 쓴다.
public class AppliedDifficulty : MonoBehaviour
{
    [SerializeField] private AppliedGameManager gameManager;

    [Header("시험값, 미확정")]
    // 이 시간(초)이 지나면 배율이 최대에 도달한다.
    [SerializeField] private float rampSeconds = 90f;
    // 발생 빈도 최대 배율. 2.5면 간격이 최대 1/2.5까지 줄어든다.
    [SerializeField] private float maxMultiplier = 2.5f;

    // 0(시작) ~ 1(최대 난이도).
    public float Progress => gameManager == null ? 0f : Mathf.Clamp01(gameManager.SurvivalTime / rampSeconds);

    public float Multiplier => Mathf.Lerp(1f, maxMultiplier, Progress);

    // 기본 간격을 배율로 나누되 minInterval 아래로는 줄이지 않는다.
    public float Interval(float baseInterval, float minInterval)
    {
        return Mathf.Max(minInterval, baseInterval / Multiplier);
    }

    // 시작 개수에서 최대 개수까지 진행도에 따라 늘린다.
    public int AllowedCount(int startCount, int maxCount)
    {
        return Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(startCount, maxCount, Progress)), startCount, maxCount);
    }
}
