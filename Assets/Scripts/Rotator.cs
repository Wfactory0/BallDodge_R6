using UnityEngine;

// 붙어 있는 오브젝트를 월드 Y축 기준으로 초당 rotationSpeed도 회전시킨다.
public class Rotator : MonoBehaviour
{
    // Inspector에서 조정 가능하지만 다른 스크립트가 임의로 바꾸지 못하도록 private으로 둔다.
    [SerializeField] private float rotationSpeed = 90f;

    private void Update()
    {
        // Time.deltaTime(지난 프레임 경과 시간)을 곱해 프레임레이트와 무관하게 "초당" 회전량이 되게 한다.
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.World);
    }
}
