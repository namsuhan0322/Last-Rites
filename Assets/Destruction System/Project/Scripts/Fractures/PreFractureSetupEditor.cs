#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Project.Scripts.Fractures
{
    [CustomEditor(typeof(PreFractureSetup))]
    public class PreFractureSetupEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            PreFractureSetup setup = (PreFractureSetup)target;

            GUILayout.Space(10);
            if (GUILayout.Button("에디터에서 미리 부수기 (Bake Fracture)", GUILayout.Height(40)))
            {
                // 에디터 상에서 Fracture 로직 실행
                Fracture.FractureGameObject(
                    setup.gameObject, setup.anchor, setup.seed, setup.totalChunks,
                    setup.insideMaterial, setup.outsideMaterial, setup.jointBreakForce, setup.density
                );

                // 원본 메쉬 숨기기
                setup.GetComponent<MeshRenderer>().enabled = false;
                var coll = setup.GetComponent<Collider>();
                if (coll) coll.enabled = false;

                // 씬 수정 사항 저장 알림
                EditorUtility.SetDirty(setup.gameObject);
                Debug.Log("성공적으로 파편이 생성되었습니다! 이 상태를 프리팹으로 저장하세요.");
            }
        }
    }
}
#endif