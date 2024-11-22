using UnityEngine;


//this script is only to convert input of user into movement of the player
//IT DEPENDS ON PlayerScript DATA!!!
public class PlayerMovement : MonoBehaviour
{
    GameObject playerGO;
    Rigidbody2D playerPhy;
    PlayerScript playerScript;
    float playerSpeed;
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

        playerPhy.velocity = v;
    }
}
