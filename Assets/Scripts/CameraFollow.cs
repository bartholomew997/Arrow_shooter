using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = new Vector3(0f, 2f, -5f);
    public float smoothSpeed = 0.125f;
    public float mouseSensitivity = 100f;
    public float pitchMin = -30f;
    public float pitchMax = 60f;

    private float yaw;
    private float pitch;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // Lock cursor for free-look
    }

    private void LateUpdate()
    {
        if (player == null)
        {
            Debug.LogWarning("Player Transform is not assigned in CameraFollow script!");
            return;
        }

        // Mouse input
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        // Camera rotation
        Quaternion targetRotation = Quaternion.Euler(pitch, yaw, 0);

        // Camera position
        Vector3 desiredPosition = player.position + targetRotation * offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Apply rotation
        transform.rotation = targetRotation;

        // DO NOT rotate player here anymore
    }

    // Provide access to yaw (for player to align with camera)
    public float GetYaw()
    {
        return yaw;
    }
}