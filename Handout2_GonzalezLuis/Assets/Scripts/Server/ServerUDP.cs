using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using TMPro;
using static ServerTCP;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
//using UnityEditorInternal.VersionControl;

public class UserData
{
    public UserData(string userName, EndPoint ep)
    {
        this.userName = userName;
        this.endPoint = ep;
    }

    public string userName;
    public EndPoint endPoint;
}

public class ServerUDP : MonoBehaviour
{
    Socket socket;

    public GameObject UItextObj;
    TextMeshProUGUI UItext;
    string serverText;

    string serverName = "MantelServer";

    public int serverPort = 9050;

    List<UserData> connectedUsers = new List<UserData>();

    void Start()
    {
        UItext = UItextObj.GetComponent<TextMeshProUGUI>();

    }
    public void startServer()
    {
        serverText = "Starting UDP Server...";

        //TO DO 1
        //UDP doesn't keep track of our connections like TCP
        //This means that we "can only" reply to other endpoints,
        //since we don't know where or who they are
        //We want any UDP connection that wants to communicate with 9050 port to send it to our socket.
        //So as with TCP, we create a socket and bind it to the 9050 port. 

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, serverPort);
        socket.Bind(ipep);

        //TO DO 3
        //Our client is sending a handshake, the server has to be able to recieve it
        //It's time to call the Receive thread
        Thread newConnection = new Thread(Receive);
        newConnection.Start();
    }

    public void SwitchScene()
    {
        SceneManager.LoadScene("Exercise1_Client");
    }

    public void CloseApp()
    {
        Application.Quit();
    }

    void Update()
    {
        UItext.text = serverText;

    }

 
    void Receive()
    {
        byte[] data = new byte[1024];
        int recv = 0;
        
        serverText = serverText + "\n" + "Waiting for new Client...";

        //TO DO 3
        //We don't know who may be comunicating with this server, so we have to create an
        //endpoint with any address and an IpEndpoint from it to reply to it later.
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        EndPoint Remote = (EndPoint)(sender);

        //Loop the whole process, and start receiveing messages directed to our socket
        //(the one we binded to a port before)
        //When using socket.ReceiveFrom, be sure send our remote as a reference so we can keep
        //this adress (the client) and reply to it on TO DO 4

        while (true)
        {
            recv = socket.ReceiveFrom(data, ref Remote);
            serverText = serverText + "\n" + "Message received from {0}:" + Remote.ToString();
            string incomingMessage = Encoding.ASCII.GetString(data, 0, recv);
            serverText = serverText + "\nMessage: " + incomingMessage;

            //if this remote is new (new user), we will assume its first message is the userName (client on click connect sends its userName to server)

            bool newUser = true;
            foreach (UserData item in connectedUsers)
            {
                if (item.endPoint.ToString() == Remote.ToString())
                {
                    newUser = false;
                    break;
                }
            }

            if (newUser)
            {
                //TO DO 4
                //When our UDP server receives a message from a random remote, it has to send a ping,
                //Call a send thread
                //send to new user the server name
                Thread answer = new Thread(() => Send(Remote, "ServerName: " + serverName));
                answer.Start();
                //tell to other clients a new client has connected
                foreach (UserData item in connectedUsers)
                {
                    Thread answer2 = new Thread(() => Send(item.endPoint, "--- User -" + incomingMessage + "- has connected ---"));
                    answer2.Start();
                }
                //add new client to connected clients list
                connectedUsers.Add(new UserData(incomingMessage, Remote));
            }
            else
            {
                //if it is not a new user, it means its a client sending a message, so it will be sent to all connected clients
                foreach (UserData item in connectedUsers)
                {
                    Thread answer2 = new Thread(() => Send(item.endPoint, item.userName + " :" + incomingMessage));
                    answer2.Start();
                }
            }
        }
    }

    void Send(EndPoint Remote, string text)
    {
        //TO DO 4
        //Use socket.SendTo to send a ping using the remote we stored earlier.
        byte[] data = new byte[1024];
        data = Encoding.ASCII.GetBytes(text);

        socket.SendTo(data, Remote);
    }
}
