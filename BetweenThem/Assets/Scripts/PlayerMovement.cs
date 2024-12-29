using UnityEngine;
using UnityEngine.UIElements;


//this script is only to convert input of user into movement of the player
//IT DEPENDS ON PlayerScript DATA!!!
public class PlayerMovement : MonoBehaviour
{
    GameObject playerGO;
    Rigidbody2D playerPhy;
    PlayerScript playerScript;
    float playerSpeed;
    public GameObject task1;
    public GameObject exitButton;
    public GameObject eButton;
    public GameObject qButton;

    public bool freezeMovement = false;

    // Start is called before the first frame update
    void Start()
    {
        playerGO = gameObject;
        this.playerPhy = playerGO.GetComponent<Rigidbody2D>();
        this.playerScript = playerGO.GetComponent<PlayerScript>();
        this.playerSpeed = playerScript.playerSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        if (this.freezeMovement) return;

        Vector2 v = new Vector2(0, 0);

        if (Input.GetKey(KeyCode.W))
        {
            v.y += 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            v.y -= 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            v.x -= 1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            v.x += 1;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            playerScript.TriggerReport();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (playerScript.impostor)
            {
                //kill
                playerScript.KillEnemy();
            }
            else if (playerScript.inRangeTasks.Count > 0)
            {
                freezeMovement = true;
                task1?.SetActive(true);
                ClearUI();
            }
        }

        if (v.magnitude > 1)
        {
            v = v.normalized * playerSpeed;
        }
        else
        {
            v *= playerSpeed;
        }

        playerPhy.velocity = v;
    }

    public void ClearUI()
    {
        exitButton?.SetActive(false);
        eButton?.SetActive(false);
        qButton?.SetActive(false);
    }

    public void ActiveUI()
    {
        exitButton?.SetActive(true);
        eButton?.SetActive(true);
        qButton?.SetActive(true);
    }
}
