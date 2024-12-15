using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using TMPro;
using System.Collections.Concurrent;
using System.Collections.Generic;
using static ServerUDP;
using System;

public class ServerUDP : MonoBehaviour
{
    Socket socket;

    public GameObject functionalities;

    string username;
    string lobyName;

    private ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>();

    public class User
    {
        public EndPoint endpoint;
        public string name;
    }

    List<User> connectedUsers = new List<User>();

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

    public void startServer(string lobyName)
    {
        this.lobyName = lobyName;
        //username = functionalities.GetComponent<Functionalities>().userName;

        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        socket.Bind(ipep);

        Thread newConnection = new Thread(Receive);
        newConnection.Start();
    }

    void Receive()
    {
        int recv = 0;
        byte[] data = new byte[1024];

        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        EndPoint Remote = (EndPoint)(sender);

        while (true)
        {
            data = new byte[1024];
            recv = socket.ReceiveFrom(data, ref Remote);
            string receivedMessage = Encoding.ASCII.GetString(data, 0, recv);

            User user = connectedUsers.Find(u => u.endpoint.Equals(Remote));
            if (user == null)
            {
                user = new User { endpoint = Remote, name = "Unknown User" };
                connectedUsers.Add(user);
                Send(lobyName, user);
            }

            if (receivedMessage.StartsWith("Name: "))
            {
                user.name = receivedMessage.Substring(6);
                BroadcastMessageServer($"{user.name} has connected.", null);
                messageQueue.Enqueue($"{user.name} has connected.");
            }
            else
            {
                BroadcastMessageServer(receivedMessage, user);
                messageQueue.Enqueue(receivedMessage);
            }
        }
    }

    public void BroadcastMessageServer(string message, User sender)
    {
        byte[] buffer = Encoding.ASCII.GetBytes(message);

        foreach (User user in connectedUsers)
        {
            if (sender == null || user.endpoint != sender.endpoint)
            {
                socket.SendTo(buffer, user.endpoint);
            }
        }
    }

    void Send(string message, User user)
    {
        byte[] buffer = Encoding.ASCII.GetBytes(message);
        socket.SendTo(buffer, user.endpoint);
    }
}