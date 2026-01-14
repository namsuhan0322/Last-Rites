using UnityEngine;
using UnityEditor;
using BansheeGz.BGDatabase;
using DB_;
using System;
using System.Collections.Generic;
using System.IO;

public class BgDbToAiSoConverterWindow : EditorWindow
{
    #region 설정 필드
    private string m_outputFolder = "Assets/AISOs";
    private AIDatabaseSO m_targetDatabaseSo;       

    private bool m_overwriteExisting = true;
    private bool m_addToDatabase = true;
    private bool m_verboseLog = true;
    #endregion

    [MenuItem("Tools/BG DB → Create AISO")]
    private static void OpenWindow()
    {
        var window = GetWindow<BgDbToAiSoConverterWindow>(true, "BG DB -> AISO Converter");
        window.minSize = new Vector2(520, 250);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("BG Database → AISO 변환기", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("출력 폴더 (Assets/...)", EditorStyles.label);
        EditorGUILayout.BeginHorizontal();
        m_outputFolder = EditorGUILayout.TextField(m_outputFolder);
        if (GUILayout.Button("폴더 선택", GUILayout.Width(100)))
        {
            string select = EditorUtility.OpenFolderPanel("Select output folder", Application.dataPath, "");
            if (!string.IsNullOrEmpty(select))
            {
                if (select.StartsWith(Application.dataPath))
                    m_outputFolder = "Assets" + select.Substring(Application.dataPath.Length);
                else
                    EditorUtility.DisplayDialog("오류", "선택한 폴더는 프로젝트의 Assets 폴더 내부에 있어야 합니다.", "확인");
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        m_targetDatabaseSo = (AIDatabaseSO)EditorGUILayout.ObjectField("Target AIDatabaseSO", m_targetDatabaseSo, typeof(AIDatabaseSO), false);

        m_addToDatabase = EditorGUILayout.ToggleLeft("생성된 SO를 Database 리스트에 자동 등록", m_addToDatabase);
        m_overwriteExisting = EditorGUILayout.ToggleLeft("기존 SO 덮어쓰기 (ID/이름 일치 시)", m_overwriteExisting);
        m_verboseLog = EditorGUILayout.ToggleLeft("상세 로그 출력", m_verboseLog);

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("변환 실행", GUILayout.Height(38)))
        {
            if (!Directory.Exists(m_outputFolder))
            {
                AssetDatabase.CreateFolder(Path.GetDirectoryName(m_outputFolder), Path.GetFileName(m_outputFolder));
                AssetDatabase.Refresh();
            }

            ConvertAllBgAiToSo();
        }

        if (GUILayout.Button("새 DatabaseSO 생성", GUILayout.Height(38), GUILayout.Width(180)))
        {
            CreateNewDatabaseSo();
        }
        EditorGUILayout.EndHorizontal();
    }

    #region 변환 로직

    private void ConvertAllBgAiToSo()
    {
        try
        {
            var entities = GatherAllAiEntities();

            if (entities.Count == 0)
            {
                EditorUtility.DisplayDialog("알림", "BG Database에서 AI 데이터를 찾지 못했습니다.\n데이터가 비어있는지 확인해주세요.", "확인");
                return;
            }

            if (m_addToDatabase && m_targetDatabaseSo == null)
            {
                if (EditorUtility.DisplayDialog("DatabaseSO 미지정", "Target DatabaseSO가 없습니다. 새로 생성하시겠습니까?", "네", "아니요"))
                    CreateNewDatabaseSo();
                else
                    m_addToDatabase = false;
            }

            int total = entities.Count;
            int processed = 0;
            List<AISO> createdSoList = new List<AISO>();

            for (int i = 0; i < total; i++)
            {
                EditorUtility.DisplayProgressBar("AISO 변환 중...", $"처리 중: {i + 1}/{total}", (float)i / total);
                var entity = entities[i];

                AISO tempSo = ScriptableObject.CreateInstance<AISO>();
                PopulateAiSOFromEntity(tempSo, entity);

                string typeFolder = Path.Combine(m_outputFolder, tempSo.roleType.ToString()).Replace("\\", "/");
                if (!Directory.Exists(typeFolder)) AssetDatabase.CreateFolder(m_outputFolder, tempSo.roleType.ToString());

                string fileName = SanitizeFileName($"{tempSo.AiID}_{tempSo.name}.asset");
                string assetPath = Path.Combine(typeFolder, fileName).Replace("\\", "/");

                AISO finalSo = null;
                AISO existing = AssetDatabase.LoadAssetAtPath<AISO>(assetPath);

                if (existing != null && m_overwriteExisting)
                {
                    finalSo = existing;
                    PopulateAiSOFromEntity(finalSo, entity); // 덮어쓰기
                }
                else if (existing != null && !m_overwriteExisting)
                {
                    finalSo = existing;
                    if (m_verboseLog) Debug.Log($"스킵됨(이미 존재): {assetPath}");
                }
                else
                {
                    finalSo = tempSo;
                    AssetDatabase.CreateAsset(finalSo, AssetDatabase.GenerateUniqueAssetPath(assetPath));
                }

                EditorUtility.SetDirty(finalSo);
                createdSoList.Add(finalSo);
                processed++;
            }

            // DB 리스트 갱신
            if (m_addToDatabase && m_targetDatabaseSo != null)
            {
                AddSOsToDatabase(createdSoList);
                EditorUtility.SetDirty(m_targetDatabaseSo);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("완료", $"변환 완료: 총 {processed}개 처리됨.", "확인");
        }
        catch (Exception ex)
        {
            EditorUtility.ClearProgressBar();
            Debug.LogError($"변환 중 오류 발생: {ex}");
        }
    }

    private void PopulateAiSOFromEntity(AISO so, DB_._AI entity)
    {
        if (so == null || entity == null) return;

        so.AiID = entity.AiID;
        so.name = entity.name;

        string roleStr = entity.Role;
        if (Enum.TryParse(roleStr, true, out RoleType parsedRole))
        {
            so.roleType = parsedRole;
        }
        else
        {
            Debug.LogWarning($"RoleType 파싱 실패 ({roleStr}). 기본값(0) 사용. ID: {entity.AiID}");
            so.roleType = RoleType.Tanker; 
        }

        so.Hp = entity.Hp;
        so.Atk = entity.Atk;
        so.Respawn = entity.Respawn;

        so.S1_Name = entity.S1_Name;
        so.S1_Val = entity.S1_Val;
        so.S1_Cool = entity.S1_Cool;

        so.S2_Name = entity.S2_Name;
        so.S2_Val = entity.S2_Val;
        so.S2_Cool = entity.S2_Cool;
    }

    private List<DB_._AI> GatherAllAiEntities()
    {
        var result = new List<DB_._AI>();
        try
        {
            int count = DB_._AI.CountEntities;
            for (int i = 0; i < count; i++)
            {
                result.Add(DB_._AI.GetEntity(i));
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"DB 조회 실패: {ex.Message}");
        }
        return result;
    }

    #endregion

    #region 유틸리티

    private void AddSOsToDatabase(List<AISO> newSOs)
    {
        if (m_targetDatabaseSo == null) return;

        Undo.RecordObject(m_targetDatabaseSo, "Update AI List");

        m_targetDatabaseSo.Initialize();

        foreach (var newSo in newSOs)
        {
            if (newSo == null) continue;

            var existing = m_targetDatabaseSo.GetItemById(newSo.AiID);

            if (existing != null)
            {
                int index = m_targetDatabaseSo.aiSO.IndexOf(existing);
                if (index >= 0) m_targetDatabaseSo.aiSO[index] = newSo;
            }
            else
            {
                m_targetDatabaseSo.aiSO.Add(newSo);
            }
        }
    }

    private void CreateNewDatabaseSo()
    {
        string path = EditorUtility.SaveFilePanelInProject("Create AIDatabaseSO", "AIDatabase", "asset", "Save Location", "Assets");
        if (string.IsNullOrEmpty(path)) return;

        var dbSo = ScriptableObject.CreateInstance<AIDatabaseSO>();
        AssetDatabase.CreateAsset(dbSo, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        m_targetDatabaseSo = dbSo;
    }

    private string SanitizeFileName(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name;
    }

    #endregion
}