using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using System.Collections;


[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class AdvancedCharacterController : MonoBehaviour
{
    // Movement Settings
    public PostProcessVolume volume;
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float crouchSpeed = 1.5f;
    public float swimSpeed = 4f;
    public float rotationSpeed = 10f;
    public float jumpForce = 8f;
    public float gravity = -25f;
    public float swimGravity = -5f;
    
    // Crouch Settings
    public float crouchHeight = 1f;
    public float standHeight = 2f;
    public float crouchTransitionSpeed = 5f;
    private float currentHeight;

    // Ground Check
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    // Animation Parameters
    public string speedParam = "Speed";
    public string jumpTriggerParam = "JumpTrigger";
    public string groundedParam = "IsGrounded";
    public string swimmingParam = "IsSwimming";
    public string crouchingParam = "IsCrouching";
    public string crouchTriggerParam = "CrouchTrigger";
    public string standTriggerParam = "StandTrigger";

    // Internal References
    private CharacterController controller;
    private Animator animator;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isSwimming;
    private bool wasSwimming;
    private bool isCrouching;
    private bool isTransitioningHeight;

    private readonly float waterSurfaceHeight = -4.4f; // Fixed water surface Y-position

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        currentHeight = standHeight;

        // Ensure PostProcessVolume is set up
        if (volume != null)
        {
            volume.isGlobal = true;
            volume.enabled = false;
        }
        else
        {
            Debug.LogWarning("PostProcessVolume is not assigned in the Inspector!");
        }
    }

    private void Update()
    {
        // Check if character is in water (Y-position < -4.4)
        isSwimming = transform.position.y < waterSurfaceHeight;
        animator.SetBool(swimmingParam, isSwimming);

        // Update PostProcessVolume based on swimming state
        if (volume != null && isSwimming != wasSwimming)
        {
            volume.enabled = isSwimming;
            wasSwimming = isSwimming;
            Debug.Log($"PostProcessVolume enabled: {volume.enabled}");
        }

        // Ground check (only when not swimming)
        isGrounded = !isSwimming && Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        animator.SetBool(groundedParam, isGrounded);

        // Reset vertical velocity when grounded
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Get player input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool isRunning = Input.GetKey(KeyCode.LeftShift) && !isSwimming && !isCrouching;
        bool jumpPressed = Input.GetKeyDown(KeyCode.Space) && isGrounded && !isSwimming && !isCrouching;
        bool crouchPressed = Input.GetKeyDown(KeyCode.C);
        bool standPressed = Input.GetKeyDown(KeyCode.LeftShift) && isCrouching; // New key for standing up

        // Handle crouching
        if (crouchPressed && isGrounded && !isSwimming)
        {
            if (isCrouching)
            {
                // Check if there's room to stand up
                if (!Physics.Raycast(transform.position, Vector3.up, standHeight - crouchHeight))
                {
                    Debug.Log("Setting StandTrigger");
                    animator.SetTrigger(standTriggerParam);
                    StartCoroutine(StandUp());
                }
                else
                {
                    Debug.Log("Cannot stand: ceiling detected");
                }
            }
            else
            {
                Debug.Log("Setting CrouchTrigger");
                isCrouching = true;
                animator.SetBool(crouchingParam, true);
                animator.SetTrigger(crouchTriggerParam);
            }
        }

       
        if (standPressed)
        {
            Debug.Log("Manually standing up!");
            animator.SetTrigger(standTriggerParam);
            StartCoroutine(StandUp());
        }

       
        float targetHeight = isCrouching ? crouchHeight : standHeight;
        if (Mathf.Abs(currentHeight - targetHeight) > 0.01f)
        {
            currentHeight = Mathf.Lerp(currentHeight, targetHeight, crouchTransitionSpeed * Time.deltaTime);
            controller.height = currentHeight;
            controller.center = new Vector3(0, currentHeight / 2, 0);
            isTransitioningHeight = true;
        }
        else
        {
            isTransitioningHeight = false;
        }

        // Movement direction
        Vector3 moveDirection = new Vector3(horizontal, 0, vertical).normalized;

        // Handle movement and rotation
        if (moveDirection.magnitude >= 0.1f && !isTransitioningHeight)
        {
            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
            float angle = Mathf.LerpAngle(transform.eulerAngles.y, targetAngle, rotationSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0, angle, 0);

            Vector3 moveDir = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
            float speed;
            
            if (isSwimming)
            {
                speed = swimSpeed;
            }
            else if (isCrouching)
            {
                speed = crouchSpeed;
            }
            else
            {
                speed = isRunning ? runSpeed : walkSpeed;
            }
            
            controller.Move(moveDir.normalized * speed * Time.deltaTime);

            // Set animation speed parameter
            if (isSwimming)
            {
                animator.SetFloat(speedParam, moveDirection.magnitude);
            }
            else if (isCrouching)
            {
                animator.SetFloat(speedParam, 0.5f); // Special value for crouch walk
            }
            else
            {
                animator.SetFloat(speedParam, isRunning ? 2f : 1f);
            }
        }
        else
        {
            animator.SetFloat(speedParam, 0f);
        }

        // Handle jumping (only when not swimming or crouching)
        if (isGrounded && jumpPressed)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            animator.SetTrigger(jumpTriggerParam);
            Debug.Log("Jump triggered!");
        }

        // Apply appropriate gravity
        float currentGravity = isSwimming ? swimGravity : gravity;
        velocity.y += currentGravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private IEnumerator StandUp()
    {
        
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("CrouchToStand"));

        float duration = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(duration);

        // Now safely update crouch flags
        isCrouching = false;
        animator.SetBool(crouchingParam, false);

        Debug.Log("Stand animation done. Player is no longer crouching.");
    }

   
    public void FinalizeStandHeight()
    {
        Debug.Log("Finalizing stand height");
        currentHeight = standHeight;
        controller.height = currentHeight;
        controller.center = new Vector3(0, currentHeight / 2, 0);
        isTransitioningHeight = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(new Vector3(-100, waterSurfaceHeight, 0), new Vector3(100, waterSurfaceHeight, 0));
    }
}