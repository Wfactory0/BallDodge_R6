using UnityEngine;

// 생존 시간 측정·표시와 게임 오버 처리를 담당한다.
public class GameManager : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private BallSpawner spawner;

    private float survivalTime;
    private bool isGameOver;
    private GUIStyle labelStyle;

    public bool IsGameOver => isGameOver;

    private void Update()
    {
        if (isGameOver) return;
        survivalTime += Time.deltaTime;
    }

    // 여러 공이 동시에 닿아도 "Game Over"는 한 번만 출력되도록 isGameOver로 막는다.
    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        Debug.Log("Game Over");
        if (player != null) player.enabled = false;
        if (spawner != null) spawner.enabled = false;
    }

    private void OnGUI()
    {
        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 28, fontStyle = FontStyle.Bold };
            labelStyle.normal.textColor = Color.white;
        }

        GUI.Label(new Rect(16f, 12f, 400f, 40f), $"Time: {survivalTime:F1}s", labelStyle);
        if (isGameOver)
        {
            GUI.Label(new Rect(16f, 48f, 400f, 40f), "Game Over", labelStyle);
        }
    }
}
