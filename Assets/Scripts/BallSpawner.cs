using UnityEngine;

// spawnInterval초마다 공중 랜덤 위치에 Rigidbody Sphere를 생성한다.
public class BallSpawner : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private float spawnHeight = 8f;
    // Ground(10x10) 안쪽 범위(중심 기준 ±).
    [SerializeField] private float spawnRange = 4.5f;
    [SerializeField] private float ballScale = 0.8f;
    [SerializeField] private Material ballMaterial;

    private float timer;

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer -= spawnInterval;
            SpawnBall();
        }
    }

    private void SpawnBall()
    {
        Vector3 position = new Vector3(
            Random.Range(-spawnRange, spawnRange),
            spawnHeight,
            Random.Range(-spawnRange, spawnRange));

        GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ball.name = "Ball";
        ball.transform.position = position;
        ball.transform.localScale = Vector3.one * ballScale;
        if (ballMaterial != null) ball.GetComponent<Renderer>().sharedMaterial = ballMaterial;

        Rigidbody rb = ball.AddComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        ball.AddComponent<Ball>().Init(gameManager);
    }
}
