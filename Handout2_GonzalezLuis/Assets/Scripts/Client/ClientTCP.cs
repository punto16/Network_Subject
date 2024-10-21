using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using TMPro;
//using UnityEngine.tvOS;
//using UnityEditor.PackageManager.UI;

public class ClientTCP : MonoBehaviour
{
    public GameObject UItextObj;
    TextMeshProUGUI UItext;
    string clientText;
    Socket server;

    public string serverIP = "127.0.0.1";
    public int serverPort = 9050;

    public string userName = "User";

    // Start is called before the first frame update
    void Start()
    {
        UItext = UItextObj.GetComponent<TextMeshProUGUI>();

    }

    // Update is called once per frame
    void Update()
    {
        UItext.text = clientText;

    }

    public void UpdateIPStringTCP(string ip)
    {
        this.serverIP = ip;
    }

    public void UpdateUserStringTCP(string user)
    {
        this.userName = user;
    }

    public void SendChatStringTCP(string text)
    {
        Thread mainThread = new Thread(() => Send(text));
        mainThread.Start();
    }

    public void StartClient()
    {
        Thread connect = new Thread(Connect);
        connect.Start();
    }
    void Connect()
    {
        //TO DO 2
        //Create the server endpoint so we can try to connect to it.
        //You'll need the server's IP and the port we binded it to before
        //Also, initialize our server socket.
        //When calling connect and succeeding, our server socket will create a
        //connection between this endpoint and the server's endpoint

        IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);
        server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        server.Connect(ipep);

        //TO DO 4
        //With an established connection, we want to send a message so the server aacknowledges us
        //Start the Send Thread
        Thread sendThread = new Thread(() => Send(userName));
        sendThread.Start();

        //TO DO 7
        //If the client wants to receive messages, it will have to start another thread. Call Receive()
        Thread receiveThread = new Thread(Receive);
        receiveThread.Start();

    }
    void Send(string text)
    {
        //TO DO 4
        //Using the socket that stores the connection between the 2 endpoints, call the TCP send function with
        //an encoded message
        byte[] data = new byte[1024];
        data = Encoding.ASCII.GetBytes(text);
        server.Send(data);

    }

    //TO DO 7
    //Similar to what we already did with the server, we have to call the Receive() method from the socket.
    void Receive()
    {
        while (true)
        {
            byte[] data = new byte[1024];
            int recv = server.Receive(data);

            clientText += "\n" + Encoding.ASCII.GetString(data, 0, recv);
        }
    }
}
