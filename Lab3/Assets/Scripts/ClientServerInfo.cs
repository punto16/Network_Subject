using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientServerInfo : MonoBehaviour
{
    public string userName = "User";
    public string serverIP = "127.0.0.1";

    ClientManagerUDP clientScript;
    PlayerScript pScript;

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this);

        clientScript = GameObject.Find("ClientManager")?.GetComponent<ClientManagerUDP>();
        pScript = GameObject.Find("Player")?.GetComponent<PlayerScript>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeUserName(string name)
    {
        this.userName = name;
    }

    public void ChangeServerIP(string ip)
    {
        this.serverIP = ip;
    }

    public void ChangeUserAndIP()
    {
        if (clientScript == null || pScript == null)
        {
            clientScript = GameObject.Find("ClientManager")?.GetComponent<ClientManagerUDP>();
            pScript = GameObject.Find("Player")?.GetComponent<PlayerScript>();
        }
        clientScript?.ChangeName(this.userName);
        clientScript?.ChangeServerIP(this.serverIP);
        pScript?.ChangeName(this.userName);
    }

}
