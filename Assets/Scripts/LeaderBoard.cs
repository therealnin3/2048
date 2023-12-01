using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dan.Main;
using UnityEngine.Events;
using TMPro;

public class LeaderBoard : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private List<Row> Rows = new List<Row>();
    [SerializeField] private SceneController sceneController;
    [SerializeField] private TMP_Text Error_msg;
    [SerializeField] private GameObject LoadingStar;

    public static string USERNAME = "";
    public static float HIGHSCORE = 0;

    public static string PUBLICKEY_4X4 = "8d4cb0d227a2591be5c00fb1ebe2bd83d6f0721c2de3fc7c9434979c13c06e32"; //988c9873fbd75f146a9ad19afdb36f715dfa3f47fc69630e355d2fdac23352354879ee6861818d2b9109fa5bc30b224c6171ae96b81ef85cd599557e823fa335cec4a661105fa49bfe12cf311fd339a821dd74a840337c4fe1c271e68ecda130d34d543c6dc387d0fc1d2445f61ef0bb1fdbccafd3308d09a8c98477d217ced6

    private bool allowEnter;

    public void Awake()
    {
        for (int i = 0; i < 11; i++)
        {
            Rows[i].No_TMP.text = "";
            Rows[i].Name_TMP.text = "";
            Rows[i].Score_TMP.text = "";
        }

        if (USERNAME != "")
        {
            sceneController.LoginToMainMenu();
        }
    }

    public void Update()
    {
        if (allowEnter && (inputField.text.Length > 0) && (Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter)))
        {
            loginButton();
            allowEnter = false;
        }
        else
        {
            allowEnter = inputField.isFocused;
        }
    }

    public void uploadAndDisplayLeaderboard()
    {
        for (int i = 0; i < 11; i++)
        {
            Rows[i].No_TMP.text = "";
            Rows[i].Name_TMP.text = "";
            Rows[i].Score_TMP.text = "";
        }

        LoadingStar.SetActive(true);
        LeaderboardCreator.UploadNewEntry(PUBLICKEY_4X4, USERNAME, (int)HIGHSCORE, ((msg) =>
                {
                    displayLeaderBoard();
                }));
    }

    public void displayLeaderBoard()
    {
        LeaderboardCreator.GetLeaderboard(PUBLICKEY_4X4, ((msg) =>
        {
            Row personalRow = Rows[10];
            bool foundYellow = false;
            int loopLength = Mathf.Min(10, msg.Length);
            for (int i = 0; i < loopLength; i++)
            {
                // Yellow personal score
                if (msg[i].IsMine())
                {
                    foundYellow = true;
                    personalRow.No_TMP.text = msg[i].Rank + ".";
                    personalRow.Name_TMP.text = "YOU - " + msg[i].Username;
                    personalRow.Score_TMP.text = msg[i].Score.ToString();

                    HIGHSCORE = msg[i].Score;
                }

                // global leaderboard
                Rows[i].No_TMP.text = msg[i].Rank + ".";
                Rows[i].Name_TMP.text = msg[i].Username;
                Rows[i].Score_TMP.text = msg[i].Score.ToString();
            }

            // Look for yellow entry
            if (!foundYellow && msg.Length > 10)
            {
                for (int i = 10; i < msg.Length; i++)
                {
                    if (msg[i].IsMine())
                    {
                        personalRow.No_TMP.text = msg[i].Rank + ".";
                        personalRow.Name_TMP.text = "YOU - " + msg[i].Username;
                        personalRow.Score_TMP.text = msg[i].Score.ToString();

                        HIGHSCORE = msg[i].Score;
                        break;
                    }
                }
            }


            LoadingStar.SetActive(false);
        }));
    }

    public void loginButton()
    {
        string username = inputField.text;
        if (isValidUsername(username))
        {
            registerUsername(username);
        }
    }

    private bool isValidUsername(string username)
    {
        if (username.Length < 10)
        {
            if (!username.Contains(" ") && !username.Contains("\n") && username != "")
            {
                Error_msg.gameObject.SetActive(false);
                return true;
            }
            else
            {
                Error_msg.gameObject.SetActive(true);
                Error_msg.text = "Username cannot contain spaces, newlines or null.";
                return false;
            }
        }
        else
        {
            Error_msg.gameObject.SetActive(true);
            Error_msg.text = "Username must be < 10 character.";
            return false;
        }
    }

    public void registerUsername(string Username)
    {
        LeaderboardCreator.GetLeaderboard(PUBLICKEY_4X4, ((msg) =>
                     {
                         bool foundDuplicate = false;

                         for (int i = 0; i < msg.Length; i++)
                         {
                             // Found matching name
                             if (msg[i].Username == Username)
                             {
                                 if (msg[i].IsMine())
                                 {
                                     HIGHSCORE = msg[i].Score;
                                     break;
                                 }
                                 else
                                 {
                                     foundDuplicate = true;
                                     break;
                                 }
                             }
                         }

                         if (foundDuplicate)
                         {
                             Error_msg.gameObject.SetActive(true);
                             Error_msg.text = "Username already exists.";
                         }
                         else
                         {
                             // Successful login
                             sceneController.LoginToMainMenu();
                             USERNAME = Username;


                             Debug.Log("USERNAME = " + USERNAME + "\nHIGHSCORE = " + HIGHSCORE);
                         }
                     }));
    }

    // public void uploadLeaderBoardEntry(string leaderBoardKey, string Username, float Score)
    // {
    //     LeaderboardCreator.UploadNewEntry(leaderBoardKey, Username, (int)Score, ((msg) =>
    //     {
    //         displayLeaderBoard();
    //     }));
    // }
}
