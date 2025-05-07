using UnityEngine;
using UnityEngine.AI; // Needed for NavMesh

public class ZombieFollow : MonoBehaviour
{
    public Transform player; // Assign the Player's Transform
    public float followRange = 20f; // Distance when zombie starts following
    private NavMeshAgent agent;
    private Animator animator;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= followRange)
        {
            agent.SetDestination(player.position);

            if (agent.velocity.magnitude > 0.1f)
            {
                animator.SetBool("isWalking", true); // Play walking animation
            }
            else
            {
                animator.SetBool("isWalking", false); // Stop walking
            }
        }
        else
        {
            animator.SetBool("isWalking", false);
            agent.ResetPath();
        }
    }
}