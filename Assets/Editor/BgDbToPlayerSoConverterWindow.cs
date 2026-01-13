using UnityEngine;
using UnityEditor;
using BansheeGz.BGDatabase;
using DB_; // BGDatabase 생성 코드 네임스페이스
using System.IO;

public class BgDbToPlayerSoConverterWindow : EditorWindow
{
    #region 설정 필드
    // 플레이어 SO가 저장될 경로
    private string m_outputFolder = "Assets/Resources/Player";
    // 저장될 파일 이름 (단일 파일이므로 고정)
    private string m_fileName = "PlayerProfile";

    private bool m_verboseLog = true;
    #endregion

    [MenuItem("Tools/BG DB → Create PlayerSO (Single)")]
    private static void OpenWindow()
    {
        var window = GetWindow<BgDbToPlayerSoConverterWindow>(true, "BG DB -> PlayerSO Converter");
        window.minSize = new Vector2(400, 200);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("BG Database → PlayerSO 변환기 (단일)", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // 1. 경로 설정
        EditorGUILayout.LabelField("저장 경로 (Assets/...)", EditorStyles.label);
        EditorGUILayout.BeginHorizontal();
        m_outputFolder = EditorGUILayout.TextField(m_outputFolder);
        if (GUILayout.Button("선택", GUILayout.Width(60)))
        {
            string select = EditorUtility.OpenFolderPanel("Select Output Folder", Application.dataPath, "");
            if (!string.IsNullOrEmpty(select))
            {
                if (select.StartsWith(Application.dataPath))
                    m_outputFolder = "Assets" + select.Substring(Application.dataPath.Length);
            }
        }
        EditorGUILayout.EndHorizontal();

        // 2. 파일명 설정
        m_fileName = EditorGUILayout.TextField("파일 이름", m_fileName);
        m_verboseLog = EditorGUILayout.Toggle("상세 로그 출력", m_verboseLog);

        EditorGUILayout.Space();

        // 3. 실행 버튼
        if (GUILayout.Button("변환 및 생성 (Overwrite)", GUILayout.Height(40)))
        {
            ConvertPlayerSo();
        }
    }

    private void ConvertPlayerSo()
    {
        try
        {
            // 1. BGDatabase에서 Player 데이터 확인
            if (DB_._Player.CountEntities == 0)
            {
                EditorUtility.DisplayDialog("오류", "BGDatabase의 Player 테이블에 데이터가 없습니다.", "확인");
                return;
            }

            // 플레이어는 1명이므로 첫 번째 엔티티(Index 0)만 가져옵니다.
            var entity = DB_._Player.GetEntity(0);

            // 2. 폴더 확인 및 생성
            if (!Directory.Exists(m_outputFolder))
            {
                Directory.CreateDirectory(m_outputFolder);
                AssetDatabase.Refresh();
            }

            // 3. 파일 경로 설정
            string assetPath = $"{m_outputFolder}/{m_fileName}.asset";

            // 4. 기존 파일 로드 또는 새로 생성
            PlayerSO so = AssetDatabase.LoadAssetAtPath<PlayerSO>(assetPath);
            bool isNew = false;

            if (so == null)
            {
                so = ScriptableObject.CreateInstance<PlayerSO>();
                isNew = true;
            }

            // 5. 데이터 매핑 (BG Entity -> SO)
            // PlayerSO의 필드명과 DB_._Player의 필드명을 매칭합니다.
            so.PlayerID = entity.PlayerID;
            so.name = entity.name; // 주의: SO의 내부 name이 아닌 public string name 변수에 할당
            so.HP = entity.HP;
            so.Move_Spd = entity.Move_Spd;
            so.Dash_Spd = entity.Dash_Spd;
            so.Dash_Time = entity.Dash_Time;
            so.Dash_Cool = entity.Dash_Cool;
            so.Max_Stamina = (int)entity.Max_Stamina; // DB가 float라면 int로 캐스팅 필요할 수 있음 (기획서엔 int)
            so.Stamina_Regen = entity.Stamina_Regen;
            so.Dash_Cost = entity.Dash_Cost;

            // 6. 에셋 저장
            if (isNew)
            {
                AssetDatabase.CreateAsset(so, assetPath);
                if (m_verboseLog) Debug.Log($"[PlayerSO] 새 파일 생성됨: {assetPath}");
            }
            else
            {
                EditorUtility.SetDirty(so);
                if (m_verboseLog) Debug.Log($"[PlayerSO] 기존 파일 업데이트됨: {assetPath}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("완료", "플레이어 데이터 변환이 완료되었습니다.", "확인");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"변환 중 오류 발생: {ex.Message}");
            EditorUtility.DisplayDialog("실패", "변환 중 오류가 발생했습니다. 콘솔을 확인하세요.", "확인");
        }
    }
}