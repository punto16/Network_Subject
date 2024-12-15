using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject task1Parent;
    public int task1AmountPerPlayer = 5;

    public int totalTasksAmount = 0;

    public int totalTasksCounter = 0;

    public GameState gameState = GameState.PRESTART;

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
        gameState = GameState.PRESTART;
        totalTasksAmount = task1AmountPerPlayer;

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

    }

    public void ChangeGameState(GameState state)
    {
        switch (state)
        {
            case GameState.PRESTART:
                {
                    this.gameState = GameState.PRESTART;
                    break;
                }
            case GameState.PLAYING:
                {
                    this.gameState = GameState.PLAYING;
                    break;
                }
            case GameState.DISCUSSION:
                {
                    this.gameState = GameState.DISCUSSION;
                    break;
                }
            case GameState.VOTE:
                {
                    this.gameState = GameState.VOTE;
                    break;
                }
            case GameState.POSTVOTE:
                {
                    this.gameState = GameState.POSTVOTE;
                    break;
                }
            case GameState.ENDGAME:
                {
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
