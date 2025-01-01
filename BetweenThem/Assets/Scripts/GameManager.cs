using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject task1Parent;
    public int alivePlayers;

    public int task1AmountPerPlayer = 5;

    public int totalTasksAmount = 0;

    public int totalTasksCounter = 0;
    public GameObject ourPlayer;
    public GameObject emergencyMeeting;
    public GameObject discussingUI;
    public GameObject votingUI;

    public GameObject postVotingUI;

    public GameObject task1UI;
    public GameObject exitTask1Button;

    public ClientManagerUDP clientManager;

    private float postVoteTimer = 0.0f;
    public float postVoteTime = 5.0f;


    public GameState gameState = GameState.PRESTART;

    private PlayerScript pScript;
    private PlayerMovement pMovement;

    public enum GameState
    {
        PRESTART,
        PLAYING,
        DISCUSSION,
        VOTE,
        POSTVOTE,
        ENDGAME
    }

    // Start is called before the first frame update
    void Start()
    {
        alivePlayers = 1;
        gameState = GameState.PRESTART;
        totalTasksAmount = task1AmountPerPlayer;

        pScript = ourPlayer.GetComponent<PlayerScript>();
        pMovement = ourPlayer.GetComponent<PlayerMovement>();

        //enable random tasks
        List<GameObject> task1Children = new List<GameObject>();
        foreach (Transform child in task1Parent.transform)
        {
            task1Children.Add(child.gameObject);
        }

        if (task1AmountPerPlayer > task1Children.Count)
        {
            task1AmountPerPlayer = task1Children.Count;
        }

        ShuffleList(task1Children);

        for (int i = 0; i < task1AmountPerPlayer; i++)
        {
            Task1Info taskInfo = task1Children[i].GetComponent<Task1Info>();
            if (taskInfo != null)
            {
                taskInfo.EnableTask(true);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (gameState == GameManager.GameState.POSTVOTE)
        {
            postVoteTimer += Time.deltaTime;

            if (postVoteTimer >= postVoteTime)
            {
                ChangeGameState(GameManager.GameState.PLAYING);
                postVoteTimer = 0.0f;
            }
        }
    }

    public void ChangeGameState(GameState state)
    {
        switch (state)
        {
            case GameState.PRESTART:
                {
                    pMovement.freezeMovement = false;
                    this.gameState = GameState.PRESTART;
                    break;
                }
            case GameState.PLAYING:
                {
                    alivePlayers = clientManager.entitiesGO.Count;
                    postVotingUI.SetActive(false);
                    pMovement.ActiveUI();
                    pMovement.freezeMovement = false;
                    this.gameState = GameState.PLAYING;
                    break;
                }
            case GameState.DISCUSSION:
                {
                    exitTask1Button.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
                    pMovement.ClearUI();
                    emergencyMeeting.SetActive(true);
                    discussingUI.SetActive(true);
                    votingUI.SetActive(false);
                    pMovement.freezeMovement = true;
                    this.gameState = GameState.DISCUSSION;
                    break;
                }
            case GameState.VOTE:
                {
                    pMovement.freezeMovement = true;
                    discussingUI.SetActive(false);
                    votingUI.SetActive(true);
                    this.gameState = GameState.VOTE;
                    break;
                }
            case GameState.POSTVOTE:
                {
                    discussingUI.SetActive(true);
                    votingUI.SetActive(false);
                    emergencyMeeting.SetActive(false);
                    postVotingUI.SetActive(true);

                    foreach (KeyValuePair<GameObject, int> entry in clientManager.entitiesGO)
                    {
                        if (entry.Key.GetComponent<PlayerScript>().alive)
                        {
                            entry.Key.transform.position = new Vector3(0, 0, entry.Key.transform.position.z);
                        }
                        else
                        {
                            entry.Key.tag = "Enemy";
                        }
                    }

                    pMovement.freezeMovement = true;
                    this.gameState = GameState.POSTVOTE;
                    break;
                }
            case GameState.ENDGAME:
                {
                    pMovement.freezeMovement = true;
                    this.gameState = GameState.ENDGAME;
                    break;
                }
            default:
                break;
        }
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
