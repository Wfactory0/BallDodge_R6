using UnityEngine;

// 떨어지는 공. 플레이어와 접촉하면 GameManager에 게임 오버를 알린다.
public class Ball : MonoBehaviour
{
    // 바닥 아래로 떨어진 공을 정리하는 높이.
    [SerializeField] private float destroyBelowY = -10f;

    private GameManager gameManager;

    // 생성한 쪽(BallSpawner)이 참조를 주입한다. 필드는 private으로 감추고 메서드로만 설정을 허용한다.
    public void Init(GameManager manager)
    {
        gameManager = manager;
    }

    private void Update()
    {
        if (transform.position.y < destroyBelowY) Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (gameManager == null) return;
        if (collision.gameObject.GetComponent<PlayerController>() != null)
        {
            gameManager.GameOver();
        }
    }
}
