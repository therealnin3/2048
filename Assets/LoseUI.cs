using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoseUI : MonoBehaviour
{

    public GameObject loseMenu;

    public void retryGame()
    {
        loseMenu.SetActive(false);
    }

    public void quitGame()
    {
        loseMenu.SetActive(false);
        Application.Quit();
    }

    public void gotoMainMenu()
    {
        loseMenu.SetActive(false);
        SceneManager.LoadScene("MainMenu");
    }
}
