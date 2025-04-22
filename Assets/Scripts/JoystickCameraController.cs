using UnityEngine;

public class JoystickCameraController : MonoBehaviour
{
    public Transform cameraPivot;
    public float rotationSpeed = 50f;
    public float deadzone = 0.15f;
    private float currentPitch = 0f;

    void Update()
    {
        Vector2 input = InputDeviceManager.joystickInput;

        float horizontal = input.x;
        float vertical = input.y;

        if (Mathf.Abs(horizontal) > deadzone || Mathf.Abs(vertical) > deadzone)
        {
            float yaw = horizontal * rotationSpeed * Time.deltaTime;
            float pitch = -vertical * rotationSpeed * Time.deltaTime;

            transform.Rotate(0f, yaw, 0f); // ← ViewRoot rotates (not Player anymore)

            if (cameraPivot != null)
            {
                currentPitch -= pitch;
                currentPitch = Mathf.Clamp(currentPitch, -70f, 70f);
                cameraPivot.localEulerAngles = new Vector3(currentPitch, 0f, 0f);
            }
        }
    }
}
