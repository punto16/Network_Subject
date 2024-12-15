using System.Net;
using System.Net.Sockets;
using UnityEngine;
using System.Threading;
using System.Collections.Generic;
using System;

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
    private readonly float startGameTime = 5.0f;
    private readonly Dictionary<string, UserData> connectedClients = new Dictionary<string, UserData>();

    bool preGame = true;

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
        //min of 4 players to play the game
        if (preGame)
        {
            if (connectedClients.Count >= 4)
            {
                timer += Time.deltaTime;
            }
            if (timer >= startGameTime)
            {
                StartGame();
                timer = 0.0f;
                preGame = false;
            }
        }
    }

    void Send(EndPoint remote, Packet.Packet p)
    {
        try
        {
            p.Send(ref socket, (IPEndPoint)remote);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error sending packet to {remote}: {e.Message}");
        }
    }

    void StartGame()
    {
        Packet.Packet pWriter = new Packet.Packet();
        pWriter.Start();

        Packet.StartGameActionDataPacket dsData = new Packet.StartGameActionDataPacket(GetRandomClientId());
        Debug.Log($"User with ID {dsData.idImpostor} is now the Impostor");
        pWriter.Serialize(Packet.Packet.PacketType.ACTION, dsData);

        foreach (var client in connectedClients.Values)
        {
            Send(client.ep, pWriter);
        }
        pWriter.Close();
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

                for (int i = 0; i < goNumber; i++)
                {
                    HandlePacket(pReader, pWriter, remote);
                }

                Broadcast(pWriter, remote);

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

            case Packet.Packet.PacketType.ACTION:
                HandleAction(pReader, pWriter, remote);
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
            Debug.Log($"New player added: {dsData.name} '{dsData.id}'");
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

    void HandleAction(Packet.Packet pReader, Packet.Packet pWriter, EndPoint remote)
    {
        Packet.Packet.ActionType aType = pReader.DeserializeGetActionType();

        switch (aType)
        {
            case Packet.Packet.ActionType.KILL:
                {
                    Packet.KillActionDataPacket dsData = pReader.DeserializeKillActionDataPacket();

                    pWriter.Serialize(Packet.Packet.PacketType.ACTION, dsData);
                    break;
                }
            case Packet.Packet.ActionType.COMPLETETASK:
                {
                    Packet.CompleteTaskActionDataPacket dsData = pReader.DeserializeTaskActionDataPacket();

                    pWriter.Serialize(Packet.Packet.PacketType.ACTION, dsData);
                    break;
                }
            case Packet.Packet.ActionType.TRIGGERREPORT:
                {
                    Packet.TriggerReportActionDataPacket dsData = pReader.DeserializeReportActionDataPacket();

                    pWriter.Serialize(Packet.Packet.PacketType.ACTION, dsData);
                    break;
                }
            case Packet.Packet.ActionType.VOTE:
                {
                    Packet.VoteActionDataPacket dsData = pReader.DeserializeVoteActionDataPacket();

                    pWriter.Serialize(Packet.Packet.PacketType.ACTION, dsData);
                    break;
                }
            case Packet.Packet.ActionType.STARTGAME:
                {
                    Packet.StartGameActionDataPacket dsData = pReader.DeserializeStartGameActionDataPacket();

                    pWriter.Serialize(Packet.Packet.PacketType.ACTION, dsData);
                    break;
                }
            default:
                break;
        }
    }

    void Broadcast(Packet.Packet pWriter, EndPoint remote, bool toEveryone = false)
    {
        foreach (var client in connectedClients.Values)
        {
            if (toEveryone || !client.ep.Equals(remote))
            {
                Send(client.ep, pWriter);
            }
        }
        pWriter.Close();
    }

    public int GetRandomClientId()
    {
        if (connectedClients.Count == 0)
        {
            Debug.LogWarning("No clients connected.");
            return 0;
        }

        System.Random random = new System.Random();
        List<UserData> clients = new List<UserData>(connectedClients.Values);
        int randomIndex = random.Next(0, clients.Count);

        return clients[randomIndex].data.id;
    }
}