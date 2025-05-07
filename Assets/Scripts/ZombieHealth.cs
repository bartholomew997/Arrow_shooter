using UnityEngine;

public class ZombieHealth : MonoBehaviour
{
    public int health = 100;
    public GameManager gameManager;  // Reference to the GameManager script

    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log(health);

        if (health <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
       
        if (gameManager != null)
        {
            gameManager.IncrementKillCount();
        }

        Destroy(gameObject);  
    }
}