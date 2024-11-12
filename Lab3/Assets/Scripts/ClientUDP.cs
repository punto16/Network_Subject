using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using TMPro;
using System.Collections.Concurrent;

public class ClientUDP : MonoBehaviour
{
    Socket socket;
    string clientText;
    IPEndPoint serverIpep;

    public GameObject functionalities;

    private ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>();

    string username;
    public string lobyName;

    void Start()
    {

    }

    void Update()
    {
        while (messageQueue.TryDequeue(out string message))
        {
            //functionalities.GetComponent<Functionalities>().InstanciateMessage(message);
        }
    }

    public void StartClient(string ipAddress)
    {
        serverIpep = new IPEndPoint(IPAddress.Parse(ipAddress), 9050);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        socket.Connect(serverIpep);
        string usernamemsg = "Name: " + username;
        Send(usernamemsg);

        byte[] data = new byte[1024];
        EndPoint remote = (EndPoint)serverIpep;
        int recv = socket.ReceiveFrom(data, ref remote);

        if (recv > 0)
        {
            string serverName = Encoding.ASCII.GetString(data, 0, recv);
            lobyName = serverName;
        }
        else
        {
            Debug.LogWarning("No servername received from the host.");
            lobyName = "Unknown Lobby";
        }

        Thread receiveThread = new Thread(Receive);
        receiveThread.Start();
    }

    public void Send(string message)
    {
        byte[] data = Encoding.ASCII.GetBytes(message);
        socket.SendTo(data, serverIpep);
    }

    void Receive()
    {
        byte[] data = new byte[1024];
        EndPoint remote = (EndPoint)serverIpep;

        while (true)
        {
            int recv = socket.ReceiveFrom(data, ref remote);
            string receivedMessage = Encoding.ASCII.GetString(data, 0, recv);

            messageQueue.Enqueue(receivedMessage);
        }
    }
}
