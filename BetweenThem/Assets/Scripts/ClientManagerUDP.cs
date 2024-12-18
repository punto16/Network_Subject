using System.Net;
using System.Net.Sockets;
using UnityEngine;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using TMPro;
using System.Linq;
using System;

public class ClientManagerUDP : MonoBehaviour
{
    Socket socket;
    IPEndPoint ipep;

    Packet.Packet pReader;
    Packet.Packet pWriter;

    public string serverIP = "127.0.0.1";   //localhost as default
    public int serverPort = 9050;

    public string userName = "User";        //username of player

    public GameObject entitiesParent;
    public GameObject mapParent;

    public GameObject MainPlayer;

    public Dictionary<GameObject, int> entitiesGO;
    public Dictionary<GameObject, int> mapGO;

    public GameObject loadingObj;

    bool socketCreated = false;

    public TMP_Text tasksAmount;
    public TMP_Text ourPlayerName;
    public GameManager gm;
    public GameObject cameraTarget;

    public float updateToServerInSeconds = 0.05f;
    private float timer = 0.0f;

    private volatile bool _keepListening = true;

    private Queue<System.Action> mainThreadActions = new Queue<System.Action>();

    // Start is called before the first frame update
    void Start()
    {
        entitiesGO = new Dictionary<GameObject, int>();
        mapGO = new Dictionary<GameObject, int>();

        //we can assume that FIRST element on this dictionary will be player
        foreach (Transform child in entitiesParent.transform)
        {
            entitiesGO.Add(child.gameObject, GenerateRandomID());
        }

        //for the moment, map will stay still, it will have no changes
        foreach (Transform child in mapParent.transform)
        {
            mapGO.Add(child.gameObject, GenerateRandomID());
        }

        pReader = new Packet.Packet();
        pWriter = new Packet.Packet();

        UpdateTasksText();

        StartClient();
    }

    public void EnqueueMainThreadAction(System.Action action)
    {
        lock (mainThreadActions)
        {
            mainThreadActions.Enqueue(action);
        }
    }

    void Update()
    {
        while (mainThreadActions.Count > 0)
        {
            System.Action action;
            lock (mainThreadActions)
            {
                action = mainThreadActions.Dequeue();
            }
            action?.Invoke();
        }

        timer += Time.deltaTime;

        if (timer >= updateToServerInSeconds)
        {
            foreach (KeyValuePair<GameObject, int> entry in entitiesGO)
            {
                GameObject obj = entry.Key;
                int id = entry.Value;
                Vector2 pos = new Vector2(obj.GetComponent<Transform>().position.x, obj.GetComponent<Transform>().position.y);
                Vector2 vel = new Vector2(obj.GetComponent<Rigidbody2D>().velocity.x, obj.GetComponent<Rigidbody2D>().velocity.y);
                
                PlayerScript ps = obj.GetComponent<PlayerScript>();
                Serialize(
                    Packet.Packet.PacketType.UPDATE,
                    id,
                    ps.userName,
                    pos,
                    vel,
                    ps.orientation,
                    ps.impostor,
                    ps.alive
                    );


                //temporal break so it only sends player
                break;
            }
            Send();
            //Thread send = new Thread(Send);
            //send.Start();

            timer = 0.0f;
        }
    }

    public void StartClient()
    {
        //initialize socket
        if (!socketCreated)
        {
            ipep = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Connect(ipep);
            socket.Blocking = false;
            Debug.Log("Socket to Server created");
            socketCreated = true;
        }
        //we get gameobject obj, that is the main player gameobject
        GameObject obj = null;
        int id = 0;
        foreach (var kvp in entitiesGO)
        {
            obj = kvp.Key;
            id = kvp.Value;
            break;
        }
        if (obj == null)
        {
            Debug.Log("Error on Start Client: Player GameObject is null");
            return;
        }

        PlayerScript ps = obj.GetComponent<PlayerScript>();
        Vector2 pos = new Vector2(obj.GetComponent<Transform>().position.x, obj.GetComponent<Transform>().position.y);
        Vector2 vel = new Vector2(obj.GetComponent<Rigidbody2D>().velocity.x, obj.GetComponent<Rigidbody2D>().velocity.y);

        Serialize(
            Packet.Packet.PacketType.CREATE,
            id,
            ps.userName,
            pos,
            vel,
            ps.orientation,
            ps.impostor,
            ps.alive
            );

        Send();

        //Thread send = new Thread(Send);
        //send.Start();

        Thread receive = new Thread(Receive);
        receive.Start();
    }

    void Serialize(Packet.Packet.PacketType typeP, int id, string name, Vector2 pos, Vector2 vel, bool orientation, bool impostor, bool alive)
    {
        Packet.RegularDataPacket dataP = new Packet.RegularDataPacket(id, name, new System.Numerics.Vector2(pos.x, pos.y), new System.Numerics.Vector2(vel.x, vel.y), orientation, impostor, alive);
        pWriter.Serialize(typeP, dataP);
    }

    void Serialize(Packet.Packet.PacketType typeP, int id)
    {
        Packet.DeleteDataPacket dataP = new Packet.DeleteDataPacket(id);
        pWriter.Serialize(typeP, dataP);
    }

    void Serialize(Packet.Packet.PacketType typeP, int id, string name, string text)
    {
        Packet.TextDataPacket dataP = new Packet.TextDataPacket(id, name, text);
        pWriter.Serialize(typeP, dataP);
    }

    void Serialize(Packet.Packet.PacketType typeP, Packet.Packet.ActionType typeA, int idKiller, int idVictim = 0)
    {
        switch (typeA)
        {
            case Packet.Packet.ActionType.KILL:
                {
                    Packet.KillActionDataPacket dataP = new Packet.KillActionDataPacket(idKiller, idVictim);
                    pWriter.Serialize(typeP, dataP);
                    break;
                }
            case Packet.Packet.ActionType.COMPLETETASK:
                {
                    Packet.CompleteTaskActionDataPacket dataP = new Packet.CompleteTaskActionDataPacket(idKiller, idVictim);
                    pWriter.Serialize(typeP, dataP);
                    break;
                }
            case Packet.Packet.ActionType.TRIGGERREPORT:
                {
                    Packet.TriggerReportActionDataPacket dataP = new Packet.TriggerReportActionDataPacket(idKiller);
                    pWriter.Serialize(typeP, dataP);
                    break;
                }
            default:
                break;
        }
    }

    public void Kill(GameObject killer, GameObject victim)
    {
        int idKiller = 0;
        int idVictim = 0;
        foreach (KeyValuePair<GameObject, int> entry in entitiesGO)
        {
            GameObject obj = entry.Key;
            if (entry.Key == victim)
            {
                idVictim = entry.Value;
                if (idKiller != 0) break;
            }
            else if (entry.Key == killer)
            {
                idKiller = entry.Value;
                if (idVictim != 0) break;
            }
        }
        Serialize(Packet.Packet.PacketType.ACTION, Packet.Packet.ActionType.KILL, idKiller, idVictim);
    }

    public void CompleteTask(GameObject go, int playerTasksAmount)
    {
        foreach (KeyValuePair<GameObject, int> entry in entitiesGO)
        {
            GameObject obj = entry.Key;
            if (entry.Key == go)
            {
                Serialize(Packet.Packet.PacketType.ACTION, Packet.Packet.ActionType.COMPLETETASK, entry.Value, playerTasksAmount);
                break;
            }
        }
    }
    
    public void TriggerReport(GameObject go)
    {
        foreach (KeyValuePair<GameObject, int> entry in entitiesGO)
        {
            GameObject obj = entry.Key;
            if (entry.Key == go)
                Serialize(Packet.Packet.PacketType.ACTION, Packet.Packet.ActionType.TRIGGERREPORT, entry.Value);
        }
    }

    void Send()
    {
        this.pWriter.Send(ref socket, ipep);
        this.pWriter.Restart();
    }

    void Receive()
    {
        byte[] data = new byte[1024];
        EndPoint Remote = new IPEndPoint(IPAddress.Any, 0);

        if (socket == null)
        {
            Debug.Log("Socket is null");
            return;
        }

        while (_keepListening)
        {
            try
            {
                if (socket.Available > 0)
                {
                    int recv = socket.ReceiveFrom(data, ref Remote);

                    // Restart and Start the packet reader
                    pReader.Restart(data);
                    pReader.Start();

                    int gameObjectsAmount = pReader.DeserializeGetGameObjectsAmount();
                    for (int i = 0; i < gameObjectsAmount; i++)
                    {
                        Packet.Packet.PacketType pType = pReader.DeserializeGetType();

                        switch (pType)
                        {
                            case Packet.Packet.PacketType.CREATE:
                            case Packet.Packet.PacketType.UPDATE:
                                {
                                    Packet.RegularDataPacket dsData = pReader.DeserializeRegularDataPacket();

                                    EnqueueMainThreadAction(() =>
                                    {
                                        HandlePlayerData(dsData);
                                    });
                                    break;
                                }
                            case Packet.Packet.PacketType.DELETE:
                                {
                                    Packet.DeleteDataPacket dsData = pReader.DeserializeDeleteDataPacket();

                                    EnqueueMainThreadAction(() =>
                                    {
                                        HandlePlayerDelete(dsData);
                                    });
                                    break;
                                }
                            case Packet.Packet.PacketType.TEXT:
                                {
                                    Packet.TextDataPacket dsData = pReader.DeserializeTextDataPacket();

                                    // No function of writing
                                    break;
                                }
                            case Packet.Packet.PacketType.ACTION:
                                {
                                    Packet.Packet.ActionType aType = pReader.DeserializeGetActionType();

                                    switch (aType)
                                    {
                                        case Packet.Packet.ActionType.KILL:
                                            {
                                                Packet.KillActionDataPacket dsData = pReader.DeserializeKillActionDataPacket();

                                                EnqueueMainThreadAction(() =>
                                                {
                                                    HandleActionKill(dsData);
                                                });
                                                break;
                                            }
                                        case Packet.Packet.ActionType.COMPLETETASK:
                                            {
                                                Packet.CompleteTaskActionDataPacket dsData = pReader.DeserializeTaskActionDataPacket();

                                                EnqueueMainThreadAction(() =>
                                                {
                                                    HandleActionCompleteTask(dsData);
                                                });
                                                break;
                                            }
                                        case Packet.Packet.ActionType.TRIGGERREPORT:
                                            {
                                                Packet.TriggerReportActionDataPacket dsData = pReader.DeserializeReportActionDataPacket();

                                                EnqueueMainThreadAction(() =>
                                                {
                                                    HandleActionReport(dsData);
                                                });
                                                break;
                                            }
                                        case Packet.Packet.ActionType.VOTE:
                                            {
                                                Packet.VoteActionDataPacket dsData = pReader.DeserializeVoteActionDataPacket();

                                                EnqueueMainThreadAction(() =>
                                                {
                                                    HandleActionVote(dsData);
                                                });
                                                break;
                                            }
                                        case Packet.Packet.ActionType.STARTGAME:
                                            {
                                                Packet.StartGameActionDataPacket dsData = pReader.DeserializeStartGameActionDataPacket();

                                                EnqueueMainThreadAction(() =>
                                                {
                                                    HandleActionStartGame(dsData);
                                                });
                                                break;
                                            }
                                        default:
                                            break;
                                    }
                                    break;
                                }
                            default:
                                break;
                        }
                    }
                }
                else
                {
                    Thread.Sleep(1);
                }
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.WouldBlock)
            {
                // No data available; continue the loop
            }
            catch (SocketException ex)
            {
                Debug.Log($"Socket exception: {ex.Message}");
            }
            catch (EndOfStreamException)
            {
                Debug.Log("End Of Stream Exception");
            }
            catch (Exception ex)
            {
                Debug.Log($"Unexpected error: {ex.Message}");
            }
            finally
            {
                pReader.Close();
            }
        }

        Debug.Log("Receive thread has exited.");
    }

    public void StopReceiving()
    {
        _keepListening = false;

        // Close the socket if needed
        if (socket != null)
        {
            socket.Close();
        }
    }

    void HandlePlayerData(Packet.RegularDataPacket dsData)
    {
        bool create = true;
        foreach (KeyValuePair<GameObject, int> entry in entitiesGO)
        {
            GameObject obj = entry.Key;
            if (entry.Value == dsData.id)
            {
                PlayerScript playerScript = obj?.GetComponent<PlayerScript>();
                obj.transform.position = new Vector3(dsData.pos.X, dsData.pos.Y, obj.transform.position.z);
                obj.GetComponent<Rigidbody2D>().velocity = new Vector3(dsData.vel.X, dsData.vel.Y, 0);
                playerScript.ChangeName(dsData.name);
                playerScript.orientation = dsData.orientation;
                playerScript.impostor = dsData.impostor;
                playerScript.alive = dsData.alive;
                create = false;
                break;
            }
        }

        if (create)
        {
            GameObject obj = Instantiate(entitiesGO.Keys.First());
            obj.tag = "Enemy";
            PlayerMovement movementScript = obj.GetComponent<PlayerMovement>();
            if (movementScript != null) Destroy(movementScript);

            PlayerScript playerScript = obj?.GetComponent<PlayerScript>();
            obj.transform.position = new Vector3(dsData.pos.X, dsData.pos.Y, obj.transform.position.z);
                obj.GetComponent<Rigidbody2D>().velocity = new Vector3(dsData.vel.X, dsData.vel.Y, 0);
            playerScript.ChangeName(dsData.name);
            playerScript.orientation = dsData.orientation;
            playerScript.impostor = dsData.impostor;
            playerScript.alive = dsData.alive;

            gm.totalTasksAmount += gm.task1AmountPerPlayer;
            UpdateTasksText();

            entitiesGO.Add(obj, dsData.id);
        }
    }

    void HandlePlayerDelete(Packet.DeleteDataPacket dsData)
    {
        foreach (KeyValuePair<GameObject, int> entry in entitiesGO)
        {
            if (entry.Value == dsData.id)
            {
                Destroy(entry.Key);
                break;
            }
        }
    }

    void HandleActionKill(Packet.KillActionDataPacket dsData)
    {
        foreach (KeyValuePair<GameObject, int> entry in entitiesGO)
        {
            if (entry.Value == dsData.idVictim)
            {
                entry.Key.GetComponent<PlayerScript>().GetKilled();
                break;
            }
        }
    }

    void HandleActionCompleteTask(Packet.CompleteTaskActionDataPacket dsData)
    {
        gm.totalTasksCounter++;
        UpdateTasksText();
    }

    void HandleActionReport(Packet.TriggerReportActionDataPacket dsData)
    {
        //todo
    }

    void HandleActionVote(Packet.VoteActionDataPacket dsData)
    {
        //todo
    }

    void HandleActionStartGame(Packet.StartGameActionDataPacket dsData)
    {
        bool playerImpostor = true;
        bool ourPlayer = true;
        foreach (KeyValuePair<GameObject, int> entry in entitiesGO)
        {
            if (ourPlayer)
            {
                entry.Key.transform.position = new Vector3(0, 0, entry.Key.transform.position.z);
                ourPlayer = false;
            }
            if (entry.Value == dsData.idImpostor)
            {
                if (playerImpostor)
                {
                    ourPlayerName.color = new Color(1.0f, 0.0f, 0.0f);
                }
                entry.Key.GetComponent<PlayerScript>().impostor = true;
                gm.totalTasksAmount -= gm.task1AmountPerPlayer;
                UpdateTasksText();
            }
            playerImpostor = false;
        }
        cameraTarget.transform.position = new Vector3(0, 0, cameraTarget.transform.position.z);
        gm.ChangeGameState(GameManager.GameState.PLAYING);
    }

    public void UpdateTasksText()
    {
        if (this.tasksAmount == null) return;

        this.tasksAmount.SetText($"Completed Tasks: {gm.totalTasksCounter} / {gm.totalTasksAmount}");
    }

    public void ChangeName(string name)
    {
        this.userName = name;
    }

    public void ChangeServerIP(string ip)
    {
        this.serverIP = ip;
    }

    public void DeletePlayer()
    {
        foreach (KeyValuePair<GameObject, int> entry in entitiesGO)
        {
            GameObject obj = entry.Key;
            int id = entry.Value;
            pWriter.Serialize(Packet.Packet.PacketType.DELETE, new Packet.DeleteDataPacket(id));
            Send();
            break;
        }
    }

    //will generate a random id of 10 digits for the size of an integer
    public int GenerateRandomID()
    {
        var r = new System.Random();
        return r.Next(1000000000, 2147483647);
    }
}
