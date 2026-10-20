using UnityEngine;

/// <summary>
/// [역할] 테스트 씬 카메라가 플레이어를 쿼터뷰로 따라가게 한다.
/// [이유] 실제 씬의 시네머신 가상 카메라 구성을 옮기지 않고 단순하게 대신한다
///        (Main Camera 프리팹의 CinemachineBrain은 가상 카메라가 없으면 카메라를 움직이지 않는다).
/// [근거] 명세 3.6-3
/// </summary>
public class DevTestCameraFollow : MonoBehaviour
{
    public Transform target;

    [Tooltip("플레이어 기준 카메라 위치 (위로 14, 뒤로 10 = 쿼터뷰)")]
    public Vector3 offset = new Vector3(0f, 14f, -10f);

    private void LateUpdate()
    {
        if (target == null) return;
        transform.position = target.position + offset;
        transform.LookAt(target.position + Vector3.up);
    }
}
