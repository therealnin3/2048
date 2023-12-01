using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject PauseUI;
    [SerializeField] GameObject LoseUI;

    public void displayPauseMenu()
    {
        PauseUI.SetActive(true);
    }

    public void PauseUIresume()
    {
        PauseUI.SetActive(false);
    }

    public void PauseUInewgmae()
    {
        PauseUI.SetActive(false);
    }

    public void gotoMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void quitGame()
    {
        Application.Quit();
    }

    public void LoseUIretry()
    {
        LoseUI.SetActive(false);
    }

    public void displayLoseScreen()
    {
        PauseUI.SetActive(false);
        LoseUI.SetActive(true);
    }
}
