using UnityEngine;
using System.Collections;

public class PlayerEmote : MonoBehaviour
{
    private Animator animator;
    private CharacterController controller;
    private bool isEmoting = false;

    [SerializeField] private float emoteDuration = 2.5f; // Set to match your emote animation

    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();

        if (animator == null) Debug.LogError("Animator not found!");
        if (controller == null) Debug.LogError("CharacterController not found!");
    }

    void Update()
    {
        // Trigger emote when E is pressed
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isEmoting)
            {
                PlayEmote();
            }
        }

        // Movement is only allowed when not emoting
        if (!isEmoting)
        {
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");
            Vector3 move = transform.right * x + transform.forward * z;
            controller.Move(move * 12f * Time.deltaTime);
        }
    }

    void PlayEmote()
    {
        Debug.Log("Playing emote");
        isEmoting = true;
        animator.SetTrigger("PlayEmote");
        StartCoroutine(ResetAfterEmote());
    }

    IEnumerator ResetAfterEmote()
    {
        yield return new WaitForSeconds(emoteDuration);

        isEmoting = false;
        animator.ResetTrigger("PlayEmote");
        animator.Play("Idle");

        // OPTIONAL: Snap player to ground using raycast (prevents floating or falling)
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, 10f))
        {
            Vector3 groundedPos = transform.position;
            groundedPos.y = hit.point.y;
            transform.position = groundedPos;
        }

        Debug.Log("Emote finished");
    }
}