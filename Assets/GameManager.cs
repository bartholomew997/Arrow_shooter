using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI killscore;  
    private int killnumber = 0;

    public KeyCode Restart = KeyCode.R; // default restart key
    public GameObject deathScreen;

    void Update()
    {
        // Update kill score UI
        killscore.text = "Zombies Killed: " + killnumber.ToString();
// Listen for restart key
            if (Input.GetKeyDown(Restart))
            {
                SceneManager.LoadScene(0); 
            }
        // Show death screen if enough zombies are killed
        if (killnumber >= 8)
        {
            deathScreen.SetActive(true);

            
        }
    }

    public void IncrementKillCount()
    {
        killnumber++;
    }
}