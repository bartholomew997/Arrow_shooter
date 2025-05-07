using UnityEngine;

public class ArrowDrawController : MonoBehaviour
{
    [Header("Animation Settings")] 
    public Animator animator;
    public string drawArrowTrigger = "DrawArrow";
    public string resetTrigger = "ResetToIdle";

    [Header("Input Settings")] 
    public KeyCode drawKey = KeyCode.F;
    public bool lockMovementWhileDrawing = true;

    [Header("Rotation Settings")]
    public float rotationSpeed = 5f;
    public float verticalRotationSpeed = 2f;
    public float verticalClampMin = -60f;
    public float verticalClampMax = 60f;

    [Header("Arrow Settings")]
    public GameObject arrowPrefab; // Assign your arrow 3D prefab in the Inspector
    public Transform arrowSpawnPoint; // Assign the position from where the arrow should be fired
    public float arrowForce = 800f; // Speed of the arrow

    private AdvancedCharacterController movementController;
    private bool isDrawing = false;
    private float currentVerticalRotation = 0f;

    private void Start()
    {
        movementController = GetComponent<AdvancedCharacterController>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(drawKey) && !isDrawing)
        {
            StartDrawing();
        }

        if (Input.GetKeyUp(drawKey))
        {
            StopDrawing();
        }

        if (isDrawing)
        {
            RotateWithMouse();
        }
    }

    private void StartDrawing()
    {
        isDrawing = true;
        animator.SetTrigger(drawArrowTrigger);

        if (lockMovementWhileDrawing && movementController != null)
        {
            movementController.enabled = false;
        }
    }

    private void StopDrawing()
    {
        isDrawing = false;

        if (!string.IsNullOrEmpty(resetTrigger))
        {
            animator.SetTrigger(resetTrigger);
        }

        if (movementController != null)
        {
            movementController.enabled = true;
        }

        ShootArrow(); // Fire the arrow
    }

    private void RotateWithMouse()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        transform.Rotate(Vector3.up, mouseX * rotationSpeed);

        currentVerticalRotation -= mouseY * verticalRotationSpeed;
        currentVerticalRotation = Mathf.Clamp(currentVerticalRotation, verticalClampMin, verticalClampMax);

        Camera.main.transform.localRotation = Quaternion.Euler(currentVerticalRotation, 0f, 0f);
    }

    private void ShootArrow()
    {
        if (arrowPrefab != null && arrowSpawnPoint != null)
        {
            GameObject arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, arrowSpawnPoint.rotation);
            Rigidbody rb = arrow.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.AddForce(arrowSpawnPoint.forward * arrowForce);
            }
        }
    }
}