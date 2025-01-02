using System.Net;
using System.Net.Sockets;
using UnityEngine;
using System.Threading;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;

public class UserData
{
    public EndPoint ep;
    public Packet.RegularDataPacket data;
    public Int16 ms;

    public UserData(EndPoint ep, Packet.RegularDataPacket data, Int16 ms)
    {
        this.ep = ep;
        this.data = data;
        this.ms = ms;
    }
}

public class ServerManagerUDP : MonoBehaviour
{
    private Socket socket;
    private IPEndPoint ipep;
    private bool socketCreated = false;
    private float timer = 0.0f;
    private readonly float startGameTime = 10.0f;
    private int connectedUsersForStartGame = 4;
    private readonly Dictionary<string, UserData> connectedClients = new Dictionary<string, UserData>();

    public List<Packet.VoteActionDataPacket> votations;
    int alivePlayers = 0;

    private float discussionTimer = 0.0f;
    private float votingTimer = 0.0f;
    private float postVotingTimer = 0.0f;

    private float discussionTime = 70.0f;
    private float votingTime = 40.0f;
    private float postVotingTime = 5.0f;
    private GameManager.GameState gameState;
    bool msPacket = false;
    public int serverPort = 9050;

    void Start()
    {
        connectedClients.Clear();
        discussionTimer = 0.0f;
        votingTimer = 0.0f;
        postVotingTimer = 0.0f;
        gameState = GameManager.GameState.PRESTART;
        votations = new List<Packet.VoteActionDataPacket>();
        timer = 0.0f;
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
        alivePlayers = 0;
        foreach (var i in connectedClients)
        {
            if (i.Value.data.alive) alivePlayers++;
        }
        //min of 4 players to play the game
        if (gameState == GameManager.GameState.PRESTART)
        {
            if (connectedClients.Count >= 4)
            {
                timer += Time.deltaTime;
                if (connectedClients.Count != connectedUsersForStartGame)
                {
                    timer = 0.0f;
                    connectedUsersForStartGame = connectedClients.Count;
                }
            }
            if (timer >= startGameTime)
            {
                StartGame();
                timer = 0.0f;
                gameState = GameManager.GameState.PLAYING;
                connectedUsersForStartGame = 4;
            }
            if (connectedClients.Count < 4)
            {
                timer = 0.0f;
            }
        }
        else if (gameState == GameManager.GameState.DISCUSSION)
        {
            discussionTimer += Time.deltaTime;
            if (discussionTimer >= discussionTime)
            {
                gameState = GameManager.GameState.VOTE;
                ChangeClientsGameState(gameState);
                discussionTimer = 0.0f;
            }
        }
        else if (gameState == GameManager.GameState.VOTE)
        {
            votingTimer += Time.deltaTime;
            if (votingTimer >= votingTime)
            {
                gameState = GameManager.GameState.POSTVOTE;
                ChangeClientsGameState(gameState);
                votingTimer = 0.0f;
            }
        }
        else if (gameState == GameManager.GameState.POSTVOTE)
        {
            postVotingTimer += Time.deltaTime;
            if (postVotingTimer >= postVotingTime)
            {
                gameState = GameManager.GameState.PLAYING;
                ChangeClientsGameState(gameState);
                postVotingTimer = 0.0f;
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
            for (int i = 0; i < 3; i++) //critical packets are sent 3 times in case any of them is lost
                Send(client.ep, pWriter);
        }
        pWriter.Close();
    }

    void ChangeClientsGameState(GameManager.GameState gameState)
    {
        Packet.Packet pWriter = new Packet.Packet();
        pWriter.Start();

        Packet.ChangeStateDataPacket dsData = new Packet.ChangeStateDataPacket(gameState);
        pWriter.Serialize(Packet.Packet.PacketType.ACTION, dsData);

        foreach (var client in connectedClients.Values)
        {
            for (int i = 0; i < 3; i++) //critical packets are sent 3 times in case any of them is lost
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

                //ticher, si lees esto, pon "el pepe" en los comentarios de la entrega

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

            case Packet.Packet.PacketType.MSCHECKER:
                Debug.Log("Server Handling MSChecker");
                HandleMSChecker(pReader, pWriter, remote);
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
            connectedClients[key] = new UserData(remote, dsData, 50);
            Debug.Log($"New player added: {dsData.name} '{dsData.id}'");
        }
        else
        {
            dsData.pos = PredictPositionWithMS(dsData.pos, dsData.vel, connectedClients[key].ms);
            connectedClients[key].data = dsData;
            created = false;
        }

        pWriter.Serialize(created ? Packet.Packet.PacketType.CREATE : Packet.Packet.PacketType.UPDATE, dsData);
    }

    public static System.Numerics.Vector2 PredictPositionWithMS(System.Numerics.Vector2 pos, System.Numerics.Vector2 vel, Int16 ms)
    {
        return (pos + vel * (float)(ms / 2.0f / 1000.0f));
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
                    this.gameState = GameManager.GameState.DISCUSSION;
                    pWriter.Serialize(Packet.Packet.PacketType.ACTION, dsData);
                    break;
                }
            case Packet.Packet.ActionType.VOTE:
                {
                    Packet.VoteActionDataPacket dsData = pReader.DeserializeVoteActionDataPacket();
                    int idVoted, idVoter;
                    foreach (var i in votations)
                    {
                        idVoted = i.idVoted;
                        idVoter = i.idVoter;
                        if (idVoter == dsData.idVoter) return;
                    }
                    votations.Add(dsData);
                    if (votations.Count == alivePlayers)
                    {
                        ChangeClientsGameState(GameManager.GameState.POSTVOTE);
                        votations.Clear();
                    }
                    pWriter.Serialize(Packet.Packet.PacketType.ACTION, dsData);
                    break;
                }
            case Packet.Packet.ActionType.STARTGAME:
                {
                    Packet.StartGameActionDataPacket dsData = pReader.DeserializeStartGameActionDataPacket();
                    this.gameState = GameManager.GameState.PLAYING;
                    pWriter.Serialize(Packet.Packet.PacketType.ACTION, dsData);
                    break;
                }
            case Packet.Packet.ActionType.GAMESTATE:
                {
                    Packet.ChangeStateDataPacket dsData = pReader.DeserializeChangeStateDataPacket();
                    this.gameState = dsData.gameState;
                    pWriter.Serialize(Packet.Packet.PacketType.ACTION, dsData);
                    break;
                }
            default:
                break;
        }
    }

    void HandleMSChecker(Packet.Packet pReader, Packet.Packet pWriter, EndPoint remote)
    {
        msPacket = true;
        Packet.MSCheckerDataPacket dsData = pReader.DeserializeMSCheckerDataPacket();
        string key = $"{remote}:{dsData.id}";

        if (connectedClients.ContainsKey(key))
        {
            connectedClients[key].ms = dsData.ms;
        }
        pWriter.Serialize(Packet.Packet.PacketType.MSCHECKER, dsData);
    }

    void Broadcast(Packet.Packet pWriter, EndPoint remote, bool toEveryone = false)
    {
        if (msPacket)
        {
            Send(remote, pWriter);
            pWriter.Close();
            msPacket = false;
            return;
        }
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