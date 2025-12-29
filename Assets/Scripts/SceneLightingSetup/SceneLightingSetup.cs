using UnityEngine;

public class SceneLightingSetup : MonoBehaviour
{
    #region 레퍼런스
    [Header("해당 씬 skybox")]
    public Material sceneSkybox;

    #endregion

    #region 초기화
    void Start()
    {
        if (SkyboxManager.Instance != null)
            SkyboxManager.Instance.SetSkybox(sceneSkybox);
        else
        {
            // 이 디버그가 떠도 무시하세요. 단순히 SkyboxManager가 씬에 없어서 뜨는 에러문구입니다.
            Debug.LogError("SkyboxManager가 씬에 존재하지 않습니다! 첫 씬에 SkyboxManager를 배치했는지 확인하세요.");

            RenderSettings.skybox = sceneSkybox;
            DynamicGI.UpdateEnvironment();
        }
    }

    #endregion
}