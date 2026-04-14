using Project.Scripts.Fractures;
using UnityEngine;

public class FractureTest : MonoBehaviour
{
    // 파괴될 오브젝트 (FractureThis 스크립트가 붙어있는 놈)
    public FractureThis target;

    void Update()
    {
        // 스페이스바를 누르면 즉시 파괴
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TestFracture();
        }
    }

    public void TestFracture()
    {
        if (target != null)
        {
            target.FractureGameobject();
            Debug.Log("Fracture Executed!");
        }
    }
}