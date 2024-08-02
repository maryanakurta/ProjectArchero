using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public int enemyCount;
    public GameObject winningDoor;
    public bool isFinished;

    public Button button;
    public Sprite playSprite;
    public Sprite pauseSprite;

    private bool isGamePaused = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        PlayerPrefs.SetInt("levelCoinCount", 0);
        PlayerPrefs.SetInt("levelEnemyCount", 0);


        isFinished = false;
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        enemyCount = enemies.Length;
    }

    public void IncreaseEnemyCount()
    {
        enemyCount++;
    }

    public void DecreaseEnemyCount()
    {
        enemyCount--;
        if (enemyCount <= 0)
        {
            isFinished = true;
        }
    }

    public void TogglePlayPause()
    {
        isGamePaused = !isGamePaused;

        if (isGamePaused)
        {
            // Pause the game
            Time.timeScale = 0f;
            button.image.sprite = playSprite;
        }
        else
        {
            // Resume the game
            Time.timeScale = 1f;
            button.image.sprite = pauseSprite;
        }
    }
}