using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{

    [SerializeField] private GameObject Leaderboards;
    [SerializeField] private GameObject MainMenu;
    [SerializeField] private GameObject LoginMenu;

    public void Awake()
    {
        // if (LeaderBoard.usernameAlreadySubmitted)
        // {
        //     MainMenu.SetActive(true);
        //     LoginMenu.SetActive(false);
        // }
        // else
        // {
        //     MainMenu.SetActive(false);
        //     LoginMenu.SetActive(true);
        // }
    }



    public void playGame()
    {
        SceneManager.LoadScene("Game");
    }

    public void MainMenuToRankings()
    {
        MainMenu.SetActive(false);
        Leaderboards.SetActive(true);
    }

    public void RankingsToMainMenu()
    {
        MainMenu.SetActive(true);
        Leaderboards.SetActive(false);
    }

    public void LoginToMainMenu()
    {
        MainMenu.SetActive(true);
        LoginMenu.SetActive(false);
    }

    public void quitGame()
    {
        Application.Quit();
    }
}
