using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ClientServerInfoStatic : MonoBehaviour
{
    public string userName = "User";
    public string serverIP = "127.0.0.1";

    public TMP_InputField serverIPInput;
    public TMP_InputField userNameInput;

    ClientServerInfo clientInfoScript;

    // Start is called before the first frame update
    void Start()
    {
        clientInfoScript = GameObject.Find("ClientServerInfo")?.GetComponent<ClientServerInfo>();

        if (clientInfoScript == null) return;
        this.userName = clientInfoScript.userName;
        this.serverIP = clientInfoScript.serverIP;
        this.userNameInput.characterLimit = 10;
        this.serverIPInput.characterLimit = 16;
        serverIPInput.text = this.serverIP;
        userNameInput.text = this.userName;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeUserName(string name)
    {
        this.userName = name;
        if (clientInfoScript == null) return;
        clientInfoScript.ChangeUserName(name);
    }

    public void ChangeServerIP(string ip)
    {
        this.serverIP = ip;
        if (clientInfoScript == null) return;
        clientInfoScript.ChangeServerIP(ip);
    }
}
