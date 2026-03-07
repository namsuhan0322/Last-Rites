using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Add this namespace for the new Input System
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class SineCameraControllerCreativeLights : MonoBehaviour
{

    public new Camera camera;
    public Transform basePivot;
    public Transform farPivot;
    public float scrollSpeed = 10f;
    public float rotationSpeed = 10f;
    public float rotationAmount = 2f;
    [Range(10f, 40f)]
    public float maximumAngle = 20f;

    private float closeFar = 0.5f;
    private float closeFarLerp = 0.5f;
    private Vector3 mouseAxisToVector;
    private float x;
    private float y;
    private Quaternion rotation;
    private bool rotationPossible = false;

    void Start()
    {
        rotation = gameObject.transform.localRotation;
        mouseAxisToVector = new Vector3(0f, 0f, 0f);
    }

    void Update()
    {
        // Detect mouse scroll wheel input
#if ENABLE_INPUT_SYSTEM
        float scrollValue = Mouse.current.scroll.ReadValue().y;
#else
        float scrollValue = Input.GetAxis("Mouse ScrollWheel");
#endif

        if (scrollValue > 0f)
        {
            if (closeFar < 1f)
            {
                closeFar += 0.1f;
            }
            if (closeFar > 1f)
            {
                closeFar = 1f;
            }
        }
        else if (scrollValue < 0f)
        {
            if (closeFar > 0f)
            {
                closeFar -= 0.1f;
            }
            if (closeFar < 0f)
            {
                closeFar = 0f;
            }
        }
        closeFarLerp = Mathf.Lerp(closeFarLerp, closeFar, Time.deltaTime * scrollSpeed);
        camera.transform.position = Vector3.Lerp(farPivot.position, basePivot.position, closeFarLerp);

        // Detect mouse button input
#if ENABLE_INPUT_SYSTEM
        rotationPossible = Mouse.current.leftButton.isPressed;
#else
        rotationPossible = Input.GetMouseButton(0);
#endif

        if (rotationPossible)
        {
            rotation = gameObject.transform.localRotation;

#if ENABLE_INPUT_SYSTEM
            x = rotation.eulerAngles.x + Mouse.current.delta.ReadValue().y * rotationAmount * 0.1f;
            y = rotation.eulerAngles.y + Mouse.current.delta.ReadValue().x * rotationAmount * 0.1f;
#else
            x = rotation.eulerAngles.x + Input.GetAxis("Mouse Y") * rotationAmount;
            y = rotation.eulerAngles.y + Input.GetAxis("Mouse X") * rotationAmount;
#endif

            if (x > maximumAngle && x < 180)
            {
                x = maximumAngle;
            }
            if (x < 340f && x > 180f)
            {
                x = 340f;
            }
            mouseAxisToVector.Set(x, y, 0f);
            rotation.eulerAngles = mouseAxisToVector;
            gameObject.transform.localRotation = rotation;
        }
    }
}
