using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    GameObject playerGO;
    public ClientManagerUDP clientManager;
    //Rigidbody2D playerPhy;
    public float playerSpeed = 5.0f;

    public string userName = "User";
    public TMP_Text tmp;

    public bool orientation = true;     //true = right | false = left
    public bool alive = true;

    //assassin stuff
    public bool impostor = false;       //is this player the impostor?
    public float killCooldown = 30.0f;  //kill cooldown
    public float killCdTimer = 0.0f;    //kill timer
    public List<GameObject> inRange;    //store a list of players in range
    public List<GameObject> inRangeTasks;
    public List<GameObject> inRangeReport;

    public int completedTasks = 0;

    PlayerMovement playerMovement;

    // Start is called before the first frame update
    void Start()
    {
        playerGO = gameObject;
        //this.playerPhy = playerGO.GetComponent<Rigidbody2D>();
        inRange = new List<GameObject>();
        tmp.SetText(this.userName);
        playerMovement = playerGO?.GetComponent<PlayerMovement>();
    }


    // Update is called once per frame
    void Update()
    {
        if (impostor && killCdTimer > 0.0f)
        {
            killCdTimer -= Time.deltaTime;
        }
    }

    public void ChangeName(string name)
    {
        this.userName = name;
        this.tmp.SetText(name);
    }

    public void KillEnemy()
    {
        if (!impostor) return;
        if (killCdTimer > 0.0f) return;
        if (inRange.Count == 0) return;

        Vector3 playerPosition = gameObject.transform.position;
        GameObject closestObject = null;
        float shortestDistance = Mathf.Infinity;

        foreach (GameObject obj in inRange)
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
            closestObject.GetComponent<PlayerScript>().GetKilled();
            clientManager.Kill(gameObject, closestObject);
        }

        killCdTimer = killCooldown;
    }

    public void GetKilled()
    {
        playerGO.GetComponent<Rigidbody2D>().transform.Rotate(0, 0, 90);
        playerGO.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        playerGO.tag = "EmergencyButton";
        alive = false;

        if (playerMovement != null)
        {
            playerMovement.freezeMovement = true;
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("EmergencyButton") && !inRangeReport.Contains(collision.gameObject))
        {
            inRangeReport.Add(collision.gameObject);
        }
        if (!impostor)
        {
            if (collision.CompareTag("TaskMachine") && !inRangeTasks.Contains(collision.gameObject) && collision.gameObject.GetComponent<Task1Info>().GetEnabledTask())
            {
                inRangeTasks.Add(collision.gameObject);
            }
        }
        else if (impostor)
        {
            if (collision.CompareTag("Enemy") && !inRange.Contains(collision.gameObject))
            {
                inRange.Add(collision.gameObject);
            }
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("EmergencyButton"))
        {
            inRangeReport.Remove(collision.gameObject);
        }
        if (!impostor)
        {
            if (collision.CompareTag("TaskMachine"))
            {
                inRangeTasks.Remove(collision.gameObject);
            }
        }
        else if (impostor)
        {
            if (collision.CompareTag("Enemy"))
            {
                inRange.Remove(collision.gameObject);
            }
        }
    }
}
