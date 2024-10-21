﻿using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TMPro;
using System.Text;
using UnityEditor;
using System.Collections.Generic;

public class ServerTCP : MonoBehaviour
{
    Socket socket;
    Thread mainThread = null;

    public GameObject UItextObj;
    TextMeshProUGUI UItext;
    string serverText;

    public int serverPort = 9050;

    List<User> connectedUsers = new List<User>();

    public struct User
    {
        public string name;
        public Socket socket;
    }

    void Start()
    {
        UItext = UItextObj.GetComponent<TextMeshProUGUI>();

    }


    void Update()
    {
        UItext.text = serverText;

    }


    public void startServer()
    {
        serverText = "Starting TCP Server...";

        //TO DO 1
        //Create and bind the socket
        //Any IP that wants to connect to the port 9050 with TCP, will communicate with this socket
        //Don't forget to set the socket in listening mode
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


        // no me deja conectarme a otras ips, solo a la mia
        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, serverPort);
        socket.Bind(ipep);

        socket.Listen(10);
        //TO DO 3
        //TIme to check for connections, start a thread using CheckNewConnections
        serverText += "\nBind success";
        mainThread = new Thread(CheckNewConnections);
        mainThread.Start();
    }

    void CheckNewConnections()
    {
        serverText += "\nChecking for new Connections...";
        while (true)
        {
            try
            {
                User newUser = new User();
                newUser.name = "";
                //TO DO 3
                //TCP makes it so easy to manage conections, so we are going
                //to put it to use
                //Accept any incoming clients and store them in this user.
                //When accepting, we can now store a copy of our server socket
                //who has established a communication between a
                //local endpoint (server) and the remote endpoint(client)
                //If you want to check their ports and adresses, you can acces
                //the socket's RemoteEndpoint and LocalEndPoint
                //try printing them on the console

                newUser.socket = socket.Accept();//accept the socket

                IPEndPoint clientep = (IPEndPoint)newUser.socket.RemoteEndPoint;
                serverText = serverText + "\n" + "Connected with " + clientep.Address.ToString() + " at port " + clientep.Port.ToString();
                Debug.Log(serverText);
                //TO DO 5
                //For every client, we call a new thread to receive their messages. 
                //Here we have to send our user as a parameter so we can use it's socket.
                Thread newConnection = new Thread(() => Receive(newUser));
                newConnection.Start();

            }
            catch (System.Exception)
            {

                serverText += "\nSomething went wrong trying to connect new client";
            }
        }
        //This users could be stored in the future on a list
        //in case you want to manage your connections

    }

    void Receive(User user)
    {
        //TO DO 5
        //Create an infinite loop to start receiving messages for this user
        //You'll have to use the socket function receive to be able to get them.
        byte[] data = new byte[1024];
        int recv = 0;

        while (true)
        {
            data = new byte[1024];
            recv = user.socket.Receive(data);
            //socket.Receive(data);

            bool newUser = true;
            foreach (User u in connectedUsers)
            {
                if (u.socket.RemoteEndPoint == user.socket.RemoteEndPoint)
                {
                    newUser = false;
                }
            }

            //check if user is new

            if (recv == 0)
                break;
            else
            {
                serverText = serverText + "\n" + user.name + ": " + Encoding.ASCII.GetString(data, 0, recv);
            }

            //TO DO 6
            //We'll send a ping back every time a message is received
            //Start another thread to send a message, same parameters as this one.
            Thread answer = new Thread(() => Send(user, "\n - You connected to MantelServer - "));
            answer.Start();
        }
    }

    //TO DO 6
    //Now, we'll use this user socket to send a "ping".
    //Just call the socket's send function and encode the string.
    void Send(User user, string text)
    {
        byte[] data = new byte[1024];
        data = Encoding.ASCII.GetBytes(text);
        user.socket.Send(data);
    }
}
