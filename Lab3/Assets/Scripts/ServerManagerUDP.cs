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
using System.Threading.Tasks;
using UnityEngine.Networking;

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
    private Socket socket;
    private IPEndPoint ipep;
    private bool socketCreated = false;
    private float timer = 0.0f;
    private readonly float updateToServerInSeconds = 1.0f;
    private readonly Dictionary<string, UserData> connectedClients = new Dictionary<string, UserData>();

    public int serverPort = 9050;

    void Start()
    {
        StartServer();
    }

    public void StartServer()
    {
        if (!socketCreated)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            ipep = new IPEndPoint(IPAddress.Any, serverPort);
            socket.Bind(ipep);
            socketCreated = true;
            Debug.Log("Server Socket Created");
        }
        Thread receiveThread = new Thread(Receive);
        receiveThread.Start();
    }

    void Update()
    {
        timer += Time.deltaTime;


        if (timer >= updateToServerInSeconds)
        {
            timer = 0.0f;
        }
    }

    void Send(EndPoint remote, Packet.Packet p)
    {
        try
        {
            p.Send(ref socket, (IPEndPoint)remote);

            p.Close();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error sending packet to {remote}: {e.Message}");
        }
    }

    void Receive()
    {
        byte[] data = new byte[1024];
        int recv = 0;
        EndPoint remote = new IPEndPoint(IPAddress.Any, serverPort);

        while (true)
        {
            try
            {
                recv = socket.ReceiveFrom(data, ref remote);
                Packet.Packet pReader = new Packet.Packet(data);
                pReader.Start();

                int goNumber = pReader.DeserializeGetGameObjectsAmount();
                Packet.Packet pWriter = new Packet.Packet();
                pWriter.Start();

                //ticher, si lees esto, pon "azúcar" en los comentarios de la entrega

                //todo: right now it detects goNumbers as 0...
                for (int i = 0; i < goNumber; i++)
                {
                    HandlePacket(pReader, pWriter, remote);
                }

                Broadcast(pWriter);

                pReader.Close();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error receiving data: {ex.Message}");
            }
        }
    }

    void HandlePacket(Packet.Packet pReader, Packet.Packet pWriter, EndPoint remote)
    {
        Packet.Packet.PacketType pType = pReader.DeserializeGetType();
        switch (pType)
        {
            case Packet.Packet.PacketType.CREATE:
            case Packet.Packet.PacketType.UPDATE:
                HandleCreateOrUpdate(pReader, pWriter, remote);
                break;

            case Packet.Packet.PacketType.DELETE:
                HandleDelete(pReader, pWriter, remote);
                break;

            case Packet.Packet.PacketType.TEXT:
                HandleText(pReader, pWriter, remote);
                break;

            default:
                Debug.LogWarning($"Unhandled packet type: {pType}");
                break;
        }
    }

    void HandleCreateOrUpdate(Packet.Packet pReader, Packet.Packet pWriter, EndPoint remote)
    {
        bool created = false;
        Packet.RegularDataPacket dsData = pReader.DeserializeRegularDataPacket();
        string key = $"{remote}:{dsData.id}";

        if (!connectedClients.ContainsKey(key))
        {
            created = true;
            connectedClients[key] = new UserData(remote, dsData);
            Debug.Log($"New player added: {dsData.name}");
        }
        else
        {
            connectedClients[key].data = dsData;
            created = false;
        }

        pWriter.Serialize(created ? Packet.Packet.PacketType.CREATE : Packet.Packet.PacketType.UPDATE, dsData);
    }

    void HandleDelete(Packet.Packet pReader, Packet.Packet pWriter, EndPoint remote)
    {
        Packet.DeleteDataPacket dsData = pReader.DeserializeDeleteDataPacket();
        string key = $"{remote}:{dsData.id}";
        if (connectedClients.ContainsKey(key))
        {
            connectedClients.Remove(key);
            Debug.Log($"Player {dsData.id} removed");
        }
        pWriter.Serialize(Packet.Packet.PacketType.DELETE, dsData);
    }

    void HandleText(Packet.Packet pReader, Packet.Packet pWriter, EndPoint remote)
    {
        Packet.TextDataPacket dsData = pReader.DeserializeTextDataPacket();
        //for de moment it does nothing
        pWriter.Serialize(Packet.Packet.PacketType.TEXT, dsData);
    }

    void Broadcast(Packet.Packet pWriter)
    {
        foreach (var client in connectedClients.Values)
        {
            Send(client.ep, pWriter);
            //Task.Run(() => Send(client.ep, pWriter));
        }
    }
}