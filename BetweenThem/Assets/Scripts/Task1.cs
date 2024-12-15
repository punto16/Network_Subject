using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Task1 : MonoBehaviour
{
    int num1 = 0;
    int num2 = 0;

    GameObject task1;
    public GameObject thumbsUpImage;
    public PlayerMovement movementScript;
    public PlayerScript playerScript;

    public GameManager gameManager;
    public ClientManagerUDP clientManager;

    public TMP_Text num1text;
    public TMP_Text num2text;

    // Start is called before the first frame update
    void Start()
    {
        this.task1 = gameObject;
        thumbsUpImage.SetActive(false);

        var r = new System.Random();
        num1 = r.Next(0, 9999);

        var r2 = new System.Random();
        num2 = r2.Next(0, 9999);

        num1text.SetText(num1.ToString());
        num2text.SetText(num2.ToString());
    }

    private void OnEnable()
    {
        Start();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Task1Answer(string answer)
    {
        int r = int.Parse(answer);
        Debug.Log($"{num1} + {num2} = {num1 + num2}, your answer is: {answer}");
        if (r == (num1 + num2))
        {
            thumbsUpImage.SetActive(true);
            //trigger sound of correct


            if (playerScript.inRangeTasks.Count > 0)
            {
                Vector3 playerPosition = playerScript.gameObject.transform.position;
                GameObject closestObject = null;
                float shortestDistance = Mathf.Infinity;

                foreach (GameObject obj in playerScript.inRangeTasks)
                {
                    if (obj == null) continue; 
                    float distance = Vector2.Distance(playerPosition, obj.transform.position);
                    if (distance < shortestDistance)
                    {
                        shortestDistance = distance;
                        closestObject = obj;
                    }
                }
                if (closestObject != null)
                {
                    closestObject.GetComponent<Task1Info>().EnableTask(false);
                }
            }
            playerScript.completedTasks++;
            clientManager.CompleteTask(gameObject, playerScript.completedTasks);
        }
        else
        {
            //maybe trigger sound of incorrect
        }
    }

    public void ExiTask1()
    {
        this.task1?.SetActive(false);
        this.movementScript.freezeMovement = false;
    }
}
