using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    GameObject playerGO;
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

    // Start is called before the first frame update
    void Start()
    {
        playerGO = gameObject;
        //this.playerPhy = playerGO.GetComponent<Rigidbody2D>();
        inRange = new List<GameObject>();
        tmp.SetText(this.userName);
    }


    // Update is called once per frame
    void Update()
    {

    }

    public void ChangeName(string name)
    {
        this.userName = name;
        this.tmp.SetText(name);
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
