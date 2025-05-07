using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class CrouchController : MonoBehaviour
{
    // Crouch Settings
    public float crouchSpeed = 1.5f;
    public float crouchHeight = 1f;
    public float standHeight = 2f;
    public float crouchTransitionSpeed = 10f;
    public LayerMask groundMask; // For ground checking

    // Animation Parameters
    public string speedParam = "Speed";
    public string crouchingParam = "IsCrouching";
    public string crouchTriggerParam = "CrouchTrigger";
    public string standTriggerParam = "StandTrigger";

    // Internal References
    private CharacterController controller;
    private Animator animator;
    private float currentHeight;
    private bool isCrouching;
    private bool isTransitioningHeight;

    public bool IsCrouching => isCrouching;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        currentHeight = standHeight;
        controller.height = standHeight;
        controller.center = new Vector3(0, standHeight / 2, 0);
        animator.SetBool(crouchingParam, false);
        SnapToGround(); // Initial grounding
        Debug.Log("CrouchController initialized");
    }

    private void Update()
    {
        // Get player input
        bool crouchPressed = Input.GetKeyDown(KeyCode.C);
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 moveDirection = new Vector3(horizontal, 0, vertical).normalized;

        // Handle crouching
        if (crouchPressed)
        {
            if (isCrouching)
            {
                RaycastHit hit;
                if (!Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.up, out hit, standHeight - crouchHeight + 0.5f, groundMask))
                {
                    Debug.Log("Setting StandTrigger for Crouch to Stand transition");
                    animator.SetTrigger(standTriggerParam);
                    StartCoroutine(StandUp());
                }
                else
                {
                    Debug.Log($"Cannot stand: Ceiling detected at {hit.point}, hit object: {hit.collider.name}");
                }
            }
            else
            {
                Debug.Log("Setting CrouchTrigger for Stand to Crouch transition");
                isCrouching = true;
                animator.SetBool(crouchingParam, true);
                animator.SetTrigger(crouchTriggerParam);
                SnapToGround();
            }
        }

        // Apply crouch movement if crouching
        if (isCrouching && moveDirection.magnitude >= 0.1f && !isTransitioningHeight)
        {
            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
            float angle = Mathf.LerpAngle(transform.eulerAngles.y, targetAngle, 10f * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0, angle, 0);
            Vector3 moveDir = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
            controller.Move(moveDir.normalized * crouchSpeed * Time.deltaTime);
            animator.SetFloat(speedParam, 0.5f);
        }
        else if (isCrouching)
        {
            animator.SetFloat(speedParam, 0f);
        }

        // Smoothly transition between crouch/stand heights
        float targetHeight = isCrouching ? crouchHeight : standHeight;
        if (Mathf.Abs(currentHeight - targetHeight) > 0.01f)
        {
            currentHeight = Mathf.Lerp(currentHeight, targetHeight, crouchTransitionSpeed * Time.deltaTime);
            controller.height = currentHeight;
            controller.center = new Vector3(0, currentHeight / 2, 0);
            SnapToGround();
            isTransitioningHeight = true;
            Debug.Log($"Transitioning height to {currentHeight}, center Y: {controller.center.y}");
        }
        else
        {
            isTransitioningHeight = false;
            if (isCrouching)
                SnapToGround();
        }

        // Always snap to ground when crouching
        if (isCrouching)
            SnapToGround();

        // Debug current animator state
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        string stateName = stateInfo.fullPathHash == Animator.StringToHash("Base Layer.Crouch to Stand") ? "Crouch to Stand" :
                           stateInfo.fullPathHash == Animator.StringToHash("Base Layer.Crouch Idle") ? "Crouch Idle" :
                           stateInfo.fullPathHash == Animator.StringToHash("Base Layer.Stand to Crouch") ? "Stand to Crouch" :
                           stateInfo.fullPathHash == Animator.StringToHash("Base Layer.Crouch Walk") ? "Crouch Walk" :
                           stateInfo.fullPathHash == Animator.StringToHash("Base Layer.Stand Idle") ? "Stand Idle" : "Other";
        Debug.Log($"CrouchController State: {stateName} (Hash: {stateInfo.fullPathHash})");
    }

    private void SnapToGround()
    {
        // Raycast downward to find ground or water surface
        Vector3 raycastOrigin = transform.position + Vector3.up * 0.5f; // Start above character
        if (Physics.Raycast(raycastOrigin, Vector3.down, out RaycastHit hit, 3f, groundMask))
        {
            Vector3 position = transform.position;
            float targetY = hit.point.y;
            // Adjust Y to keep controller's bottom at ground
            position.y = targetY + controller.skinWidth; // Account for skin width
            transform.position = position;
            Debug.Log($"Snapped to ground: Y set to {targetY}, adjusted to {position.y} with skin width");
        }
        else
        {
            Debug.LogWarning("No ground or water surface detected below character!");
        }
    }

    private IEnumerator StandUp()
    {
        float animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animationLength + 0.1f);
        Debug.Log("Stand animation complete, setting IsCrouching to false");
        isCrouching = false;
        animator.SetBool(crouchingParam, false);
        FinalizeStandHeight();
    }

    public void FinalizeStandHeight()
    {
        Debug.Log("Finalizing stand height via Animation Event");
        currentHeight = standHeight;
        controller.height = currentHeight;
        controller.center = new Vector3(0, currentHeight / 2, 0);
        isTransitioningHeight = false;
        SnapToGround();
    }

    public void ResetCrouch()
    {
        Debug.Log("Resetting crouch state");
        isCrouching = false;
        animator.SetBool(crouchingParam, false);
        currentHeight = standHeight;
        controller.height = currentHeight;
        controller.center = new Vector3(0, currentHeight / 2, 0);
        isTransitioningHeight = false;
        SnapToGround();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position + Vector3.up * 0.5f, transform.position + Vector3.up * (standHeight - crouchHeight + 0.5f));
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position + Vector3.up * 0.5f, transform.position + Vector3.down * 3f); // Ground check
    }
}