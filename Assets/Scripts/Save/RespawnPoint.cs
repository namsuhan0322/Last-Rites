using UnityEngine;

public class RespawnPoint : MonoBehaviour
{
    private bool _isActivated = false;
    public Transform safeSpawnPosition;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (_isActivated) return;
            _isActivated = true;

            Vector3 savePos = safeSpawnPosition != null ? safeSpawnPosition.position : transform.position;

            GameProgressManager.Instance.SaveRespawnPoint(savePos);

            Debug.Log("<color=cyan>[체크포인트] 새로운 부활 지점이 저장되었습니다!</color>");
        }
    }
}