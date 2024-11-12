using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerScript : MonoBehaviour
{
    GameObject playerGO;
    Rigidbody2D playerPhy;
    float playerSpeed = 5.0f;

    public bool alive = true;

    //assassin stuff
    public bool impostor = false;       //is this player the impostor?
    public float killCooldown = 30.0f;  //kill cooldown
    public float killCdTimer = 0.0f;  //kill cooldown
    public List<GameObject> inRange;    //store a list of players in range

    // Start is called before the first frame update
    void Start()
    {
        playerGO = gameObject;
        this.playerPhy = playerGO.GetComponent<Rigidbody2D>();
        inRange = new List<GameObject>();
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

    public void KillEnemy()
    {
        if (!impostor) return;
        if (killCdTimer < 0.0f) return;
        if (inRange.Count == 0) return;

        
    }

    public void GetKilled()
    {
        //die animation
        //turn into ghost??? - maybe just dissable movement?
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (!impostor) return;
        if (collision.CompareTag("Enemy") && !inRange.Contains(collision.gameObject))
        {
            inRange.Add(collision.gameObject);
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (!impostor) return;
        if (collision.CompareTag("Enemy"))
        {
            inRange.Remove(collision.gameObject);
        }
    }
}
