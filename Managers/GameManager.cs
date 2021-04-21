using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Units;
using UnityEngine.Events;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject CastlePlayer1;
    public GameObject CastlePlayer2;

    public UnityEvent onEndGame = new UnityEvent();

    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private TextMeshProUGUI gameOverScreenText;
    [SerializeField] private GameObject UnitSettingsScreen;

    [SerializeField] private int unitsToSpawnPerWave;

    private void Awake()
    {
        if (instance == null)
        {
            Instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
        }
    }

    public void EndGame()
    {
        // When either castles are inactive, show the end screen with the correct text
        if (!CastlePlayer1.activeSelf || !CastlePlayer2.activeSelf)
        {
            if (CastlePlayer1.activeSelf) // If player 1 has an active castle
            {
                // Make castle 1 stop producing units
                CastlePlayer1.GetComponent<UnitCreator>().DisableSpawning();
                gameOverScreenText.text = "Player Won";
            }
             
            else // If player 1 does not have an active castle
            {
                CastlePlayer2.GetComponent<UnitCreator>().DisableSpawning();
                gameOverScreenText.text = "Player Lost";
            }

            // Deactivate settings screen and activate game over screen
            ToggleUnitSettingsScreen(false);
            gameOverScreen.SetActive(true);

            // Make all units idle
            Idle();
        }
    }

  
    public void Idle()
    {
        onEndGame.Invoke();
    }

    public void ToggleUnitSettingsScreen(bool deactivate = true)
    {
        if (!deactivate)
        {
            UnitSettingsScreen.SetActive(deactivate);
        }

        else
        {
            UnitSettingsScreen.SetActive(!UnitSettingsScreen.activeSelf);
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
