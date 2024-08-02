using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI coinText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        int coinCount = PlayerPrefs.GetInt("Coins");
        coinText.text = "Coins: " + coinCount.ToString();
    }

    public void OnExitClick()
    {
        Application.Quit();
    }

    public void OnPlayClick()
    {
        SceneManager.LoadScene("Level1");
    }

    public void IncreaseCoins(int amount)
    {
        int currentCoins = PlayerPrefs.GetInt("Coins");
        currentCoins += amount;
        PlayerPrefs.SetInt("Coins", currentCoins);
        coinText.text = currentCoins.ToString();
    }

    public void DecreaseCoins(int amount)
    {
        int currentCoins = PlayerPrefs.GetInt("Coins");
        currentCoins -= amount;
        if (currentCoins < 0)
        {
            currentCoins = 0; // Prevent negative coin balance
        }
        PlayerPrefs.SetInt("Coins", currentCoins);
        coinText.text = currentCoins.ToString();
    }
}
