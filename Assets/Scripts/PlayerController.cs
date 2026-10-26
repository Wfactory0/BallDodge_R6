using UnityEngine;
using UnityEngine.InputSystem;

// WASD로 플레이어(Capsule)를 XZ 평면에서 이동시킨다.
// 키네마틱 Rigidbody + MovePosition을 써서, 바닥에 멈춰 잠든(sleep) 공과 닿아도 충돌 이벤트가 발생하게 한다.
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    // Ground(10x10)를 벗어나지 않도록 하는 이동 한계(중심 기준 ±).
    [SerializeField] private float boundary = 4.5f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    private void FixedUpdate()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        Vector3 input = Vector3.zero;
        if (keyboard.wKey.isPressed) input.z += 1f;
        if (keyboard.sKey.isPressed) input.z -= 1f;
        if (keyboard.dKey.isPressed) input.x += 1f;
        if (keyboard.aKey.isPressed) input.x -= 1f;
        if (input == Vector3.zero) return;

        // FixedUpdate 안에서 Time.deltaTime은 고정 물리 간격(fixedDeltaTime)을 돌려준다.
        Vector3 next = rb.position + input.normalized * moveSpeed * Time.deltaTime;
        next.x = Mathf.Clamp(next.x, -boundary, boundary);
        next.z = Mathf.Clamp(next.z, -boundary, boundary);
        rb.MovePosition(next);
    }
}
