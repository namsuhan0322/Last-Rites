using UnityEngine;

public class CameraDebug : MonoBehaviour
{
    [Header("추적할 대상들")]
    [Tooltip("메인 카메라를 넣으세요")]
    public Transform mainCamera;
    [Tooltip("플레이어의 최상위 부모 객체를 넣으세요")]
    public Transform player;
    [Tooltip("시네머신이 Follow하고 있는 Camera Body를 넣으세요")]
    public Transform cameraTarget;

    [Header("디버그 감도 설정")]
    [Tooltip("1프레임당 이 속도 이상으로 움직이면 '튀었다'고 판단합니다.")]
    public float jumpSpeedThreshold = 20f;

    [Tooltip("Scene 뷰에 지나온 궤적을 선으로 그릴지 여부")]
    public bool drawTrails = true;

    // 이전 프레임의 위치 저장용
    private Vector3 _prevCamPos;
    private Vector3 _prevPlayerPos;
    private Vector3 _prevTargetPos;

    private void Start()
    {
        if (mainCamera == null && Camera.main != null)
            mainCamera = Camera.main.transform;

        // 초기 위치 저장
        if (mainCamera != null) _prevCamPos = mainCamera.position;
        if (player != null) _prevPlayerPos = player.position;
        if (cameraTarget != null) _prevTargetPos = cameraTarget.position;
    }

    // 카메라와 플레이어의 이동이 모두 끝난 LateUpdate에서 검사해야 정확합니다.
    private void LateUpdate()
    {
        CheckAndLogJump("카메라(Camera)", mainCamera, ref _prevCamPos, Color.red);
        CheckAndLogJump("플레이어(Player)", player, ref _prevPlayerPos, Color.green);
        CheckAndLogJump("카메라 타겟(Camera Body)", cameraTarget, ref _prevTargetPos, Color.cyan);
    }

    private void CheckAndLogJump(string objName, Transform targetTransform, ref Vector3 prevPos, Color trailColor)
    {
        if (targetTransform == null) return;

        Vector3 currentPos = targetTransform.position;
        float distance = Vector3.Distance(prevPos, currentPos);

        // 프레임간 이동 속도 계산 (거리 / 걸린 시간)
        float speed = 0f;
        if (Time.deltaTime > 0)
        {
            speed = distance / Time.deltaTime;
        }

        // Scene 뷰에 궤적 그리기 (2초 동안 유지됨)
        if (drawTrails && distance > 0.01f)
        {
            Debug.DrawLine(prevPos, currentPos, trailColor, 2f);
        }

        // 설정한 임계값보다 비정상적으로 빠르게 움직였다면 로그 출력!
        if (speed > jumpSpeedThreshold)
        {
            string hexColor = ColorUtility.ToHtmlStringRGB(trailColor);
            Debug.Log($"<color=#{hexColor}><b>[지터링 감지] {objName} 튐!</b></color>\n" +
                      $"속도: {speed:F1} | 이동 거리: {distance:F3} | 위치: {prevPos} -> {currentPos}");
        }

        // 현재 위치를 다음 프레임을 위해 저장
        prevPos = currentPos;
    }
}