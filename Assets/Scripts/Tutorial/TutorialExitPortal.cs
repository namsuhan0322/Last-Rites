using UnityEngine;

public class TutorialExitPortal : MonoBehaviour
{
    private bool _hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (_hasTriggered) return;

        if (other.CompareTag("Player"))
        {
            if (GameProgressManager.Instance.progressData.isTutorialCleared)
            {
                _hasTriggered = true;
                Debug.Log("튜토리얼 출구 도달! 로비로 이동합니다.");
                ScenesManager.Instance.LoadLobbyScene();
            }
            else
            {
                Debug.Log("아직 튜토리얼 보스를 잡지 않아 나갈 수 없습니다!");
            }
        }
    }
}