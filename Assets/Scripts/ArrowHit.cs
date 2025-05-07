using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowHit : MonoBehaviour
{
    public int damage = 50;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Zombie"))
        {
            ZombieHealth zombie = other.GetComponent<ZombieHealth>();
            if (zombie != null)
            {
                zombie.TakeDamage(damage);
            }

            Destroy(gameObject); 
        }
        else
        {
            // Optional: destroy arrow when hitting anything else
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Destroy(gameObject, 5f); // auto-destroy after 5 seconds
    }
}