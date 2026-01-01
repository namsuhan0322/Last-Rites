using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    #region 레퍼런스
    private PlayerMovement playerMovement;

    [Header("Input 세팅")]
    [SerializeField] private float _clickDistanceTolerance = 0.5f;
    [SerializeField] private LayerMask _groundLayer;

    public static event System.Action<Vector3> OnGroundTouch;

    #endregion

    #region 초기화
    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    #endregion

    #region 업데이트
    void Update()
    {
        if (Input.GetMouseButtonDown(1)) HandleInput();
    }

    #endregion

    #region 인풋 관리
    private void HandleInput()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, _groundLayer))
        {
            if (NavMesh.SamplePosition(hit.point, out NavMeshHit navMeshHit, _clickDistanceTolerance, NavMesh.AllAreas))
            {
                playerMovement.MoveTo(navMeshHit.position);
                OnGroundTouch?.Invoke(navMeshHit.position);

                //EffectManager.Instance.PlayEffect("ClickMousePoint", navMeshHit.position + Vector3.up * 0.1f, Quaternion.identity);
            }
        }
    }

    #endregion
}