using System.Net;
using System.Net.Sockets;
using UnityEngine;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using Unity.Mathematics;
using System;
using System.Linq;
using System.Text;

public class UserData
{
    public EndPoint ep;
    public Packet.RegularDataPacket data;

    public UserData(EndPoint ep, Packet.RegularDataPacket data)
    {
        this.ep = ep;
        this.data = data;
    }
}

public class ServerManagerUDP : MonoBehaviour
{
    Socket socket;

    public int serverPort = 9050;

    public List<UserData> connectedClients;

    public float updateToServerInSeconds = 1.0f;
    private float timer = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        connectedClients = new List<UserData>();

        StartServer();
    }

    public void StartServer()
    {
        //initialize everything in server
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, serverPort);
        socket.Bind(ipep);

        //on creating server, we are ready to receive data
        Thread newConnection = new Thread(Receive);
        newConnection.Start();
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;



        //for update packets -> maybe once per second we can call any Send info to server, and right after that receive info from server
        if (timer >= updateToServerInSeconds)
        {

            //on end, we restart the timer
            timer = 0.0f;
        }

        //but for example, on create/delete/text packets, we can send them asap
    }

    void Send(EndPoint Remote, Packet.Packet p)
    {
        try
        {
            socket.SendTo(p.GetMemoryStream().GetBuffer(), Remote);
        }
        catch (System.Exception)
        {
            Debug.Log("Error Sending Packet to Clients\n");
            throw;
        }
    }

    void Receive()
    {
        //receiving packet

        byte[] data = new byte[1024];
        int recv = 0;
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, serverPort);
        EndPoint Remote = (EndPoint)sender;

        while (true)
        {
            try
            {
                recv = socket.ReceiveFrom(data, ref Remote);
            }
            catch (System.Exception)
            {
                Debug.Log("Error Receiving from Client");
                break;
            }

            //deserializing packet
            Packet.Packet p = new Packet.Packet(data);
            p.Start();

            Packet.Packet newP = new Packet.Packet();
            newP.Start();

            try
            {
                //while (p.GetMemoryStream().Position < p.GetMemoryStream().Length)
                //{

                    Packet.Packet.PacketType pType = p.DeserializeGetType();

                    switch (pType)
                    {
                        case Packet.Packet.PacketType.CREATE:
                            {
                                //right now, at this point, server only needs to apply a random id, store the data and send the packet to other clients
                                Packet.RegularDataPacket dsData = p.DeserializeRegularDataPacket();
                                Debug.Log("Creation Data Deserialized");
                                //once we applied the unique id, we will store the info in a list of clients
                                if (!CheckConnectedPlayer(Remote, dsData.id))
                                {
                                    //if its new player, we add it to the list of connectedClients
                                    connectedClients.Add(new UserData(Remote, dsData));
                                }
                                else
                                {
                                    //if its already connected player, we update his info
                                    foreach (var client in connectedClients)
                                    {
                                        if (client.ep.Equals(Remote) && client.data.id == dsData.id)
                                        {
                                            client.data = dsData;
                                            break;
                                        }
                                    }
                                }
                                //once the server has stored the connectedClient, we can seralize the data
                                newP.Serialize(pType, dsData);
                                Debug.Log("Creation Data Serialized and ready to send");
                                break;
                        }
                        case Packet.Packet.PacketType.UPDATE:
                            {
                                //right now, at this point, server only needs to store the data and send the packet to other clients
                                Packet.RegularDataPacket dsData = p.DeserializeRegularDataPacket();
                                //once we applied the unique id, we will store the info in a list of clients
                                //this condition is for the same as before of error type may happen
                                if (!CheckConnectedPlayer(Remote, dsData.id))
                                {
                                    //if its new player, we add it to the list of connectedClients
                                    connectedClients.Add(new UserData(Remote, dsData));
                                }
                                else
                                {
                                    //if its already connected player, we update his info
                                    foreach (var client in connectedClients)
                                    {
                                        if (client.ep.Equals(Remote) && client.data.id == dsData.id)
                                        {
                                            client.data = dsData;
                                            //here we can maybe add a data veryfier
                                            //(for example, someone going real fast, someone changing from non impostor to impostor)
                                            //basically, prevent hacking
                                            break;
                                        }
                                    }
                                }
                                //once the server has stored the connectedClient, we can seralize the data
                                newP.Serialize(pType, dsData);
                                break;
                            }
                        case Packet.Packet.PacketType.DELETE:
                            {
                                //right now, at this point, server only needs to store the data and send the packet to other clients
                                Packet.DeleteDataPacket dsData = p.DeserializeDeleteDataPacket();

                                ////if its already connected player, we remove his info
                                //foreach (var client in connectedClients)
                                //{
                                //    if (client.ep.Equals(Remote) && client.data.id == dsData.id)
                                //    {
                                //        connectedClients.Remove(client);
                                //        toDestroyClients.Add(client);
                                //        break;
                                //    }
                                //}
                                //once the server has stored the connectedClient, we can seralize the data
                                newP.Serialize(pType, dsData);
                                break;
                            }
                        case Packet.Packet.PacketType.TEXT:
                            {
                                //server dont care about text messages, they will be more client to client
                                Packet.TextDataPacket dsData = p.DeserializeTextDataPacket();
                                newP.Serialize(pType, dsData);
                                break;
                            }
                        default:
                            break;
                    }
                //}
                //we dont want to send the same packet more than 1 time to each client
                List<EndPoint> sentEP = new List<EndPoint>();
                //once we analyzed and stored the data of the received packet, we can send it to all other clients
                foreach (var client in connectedClients)
                {
                    if (!client.ep.Equals(Remote) && !sentEP.Contains(client.ep))
                    {
                        Thread send = new Thread(() => Send(
                            client.ep,
                            newP
                            ));
                        send.Start();
                        sentEP.Add(client.ep);
                        Debug.Log("Sending Data to Clients");
                    }
                }
            }
            catch (EndOfStreamException)
            {
                Debug.Log("End Of Stream Exception");
            }
            p.Close();
            newP.Close();

            //Debug.Log("Message received from {0}: " + Remote.ToString());
            //Debug.Log("\nMessage: " + Encoding.ASCII.GetString(data, 0, recv));

        }
    }

    //return true if the player is already connected
    public bool CheckConnectedPlayer(EndPoint ep, int id)
    {

        return connectedClients.Any(client => client.ep.Equals(ep) && client.data.id == id);
    }
}
