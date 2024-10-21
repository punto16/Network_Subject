using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using TMPro;
using UnityEngine.SceneManagement;

public class ClientUDP : MonoBehaviour
{
    Socket socket;
    public GameObject UItextObj;
    TextMeshProUGUI UItext;
    string clientText;

    public string serverIP = "127.0.0.1";
    public int serverPort = 9050;

    public string userName = "User";

    // Start is called before the first frame update
    void Start()
    {
        UItext = UItextObj.GetComponent<TextMeshProUGUI>();

    }
    public void StartClient()
    {
        Thread mainThread = new Thread(() => Send(userName));
        mainThread.Start();
    }

    public void UpdateIPStringUDP(string ip)
    {
        this.serverIP = ip;
    }

    public void UpdateUserStringUDP(string user)
    {
        this.userName = user;
    }

    public void SendChatStringUDP(string text)
    {
        Thread mainThread = new Thread(() => Send(userName + ": " + text));
        mainThread.Start();
    }

    public void SwitchScene()
    {
        SceneManager.LoadScene("Exercise1_Server");
    }

    public void CloseApp()
    {
        Application.Quit();
    }

    void Update()
    {
        UItext.text = clientText;
    }

    void Send(string text)
    {
        //TO DO 2
        //Unlike with TCP, we don't "connect" first,
        //we are going to send a message to establish our communication so we need an endpoint
        //We need the server's IP and the port we've binded it to before
        //Again, initialize the socket
        try
        {
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            socket.Connect(ipep);

            //TO DO 2.1 
            //Send the Handshake to the server's endpoint.
            //This time, our UDP socket doesn't have it, so we have to pass it
            //as a parameter on it's SendTo() method

            byte[] data = new byte[1024];
            string handshake = text;

            socket.SendTo(Encoding.ASCII.GetBytes(handshake), ipep);

            //TO DO 5
            //We'll wait for a server response,
            //so you can already start the receive thread
            Thread receive = new Thread(Receive);
            receive.Start();
        }
        catch (System.Exception)
        {
            clientText += "Error Sending Message to server\n";
            throw;
        }

    }

    //TO DO 5
    //Same as in the server, in this case the remote is a bit useless
    //since we already know it's the server who's communicating with us
    void Receive()
    {
        IPEndPoint sender = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);
        EndPoint Remote = (EndPoint)sender;
        byte[] data = new byte[1024];
        int recv = socket.ReceiveFrom(data, ref Remote);

        clientText += ("Message received from {0}: " + Remote.ToString());
        clientText = clientText + "\nMessage: " + Encoding.ASCII.GetString(data, 0, recv);

    }

}

