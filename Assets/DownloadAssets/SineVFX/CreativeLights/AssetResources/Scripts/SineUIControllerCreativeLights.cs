using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Add this namespace for the new Input System
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class SineUIControllerCreativeLights : MonoBehaviour
{

    public Transform prefabHolder;
    public CanvasGroup canvasGroup;

    private Transform[] prefabs;
    private List<Transform> lt;
    private int activeNumber = 0;

    private bool firstUpdate = true;

    private void Start()
    {
        lt = new List<Transform>();
        prefabs = prefabHolder.GetComponentsInChildren<Transform>(true);

        foreach (Transform tran in prefabs)
        {
            if (tran.parent == prefabHolder)
            {
                lt.Add(tran);
            }
        }
        prefabs = lt.ToArray();
    }

    void Update()
    {
        // Detect key press for toggling UI visibility
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current.hKey.wasPressedThisFrame)
#else
        if (Input.GetKeyDown(KeyCode.H))
#endif
        {
            canvasGroup.alpha = 1f - canvasGroup.alpha;
        }

        // Detect key press for changing effects
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame)
#else
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
#endif
        {
            ChangeEffect(true);
        }

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame)
#else
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
#endif
        {
            ChangeEffect(false);
        }
    }

    private void LateUpdate()
    {
        if (firstUpdate == true)
        {
            EnableActive();
            firstUpdate = false;
        }
    }

    // Turn On active VFX Prefab
    public void EnableActive()
    {
        for (int i = 0; i < prefabs.Length; i++)
        {
            if (i == activeNumber)
            {
                prefabs[i].gameObject.SetActive(true);
            }
            else
            {
                prefabs[i].gameObject.SetActive(false);
            }
        }
    }

    // Change active VFX
    public void ChangeEffect(bool bo)
    {
        if (bo == true)
        {
            activeNumber++;
            if (activeNumber == prefabs.Length)
            {
                activeNumber = 0;
            }
        }
        else
        {
            activeNumber--;
            if (activeNumber == -1)
            {
                activeNumber = prefabs.Length - 1;
            }
        }

        EnableActive();
    }
}
