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

    public bool autoRight = false;

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
            v.y += playerSpeed;
        }
        if (Input.GetKey(KeyCode.S))
        {
            v.y -= playerSpeed;
        }
        if (Input.GetKey(KeyCode.A))
        {
            v.x -= playerSpeed;
        }
        if (Input.GetKey(KeyCode.D))
        {
            v.x += playerSpeed;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            freezeMovement = true;
            task1?.SetActive(true);
            exitButton?.SetActive(false);
        }


        //testing
        if (Input.GetKeyDown(KeyCode.P))
        {
            autoRight = !autoRight;
        }

        if (autoRight)
        {
            v.x += playerSpeed;
        }

        playerPhy.velocity = v;
    }
}
