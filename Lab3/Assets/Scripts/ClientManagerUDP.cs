using System.Net;
using System.Net.Sockets;
using UnityEngine;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class ClientManagerUDP : MonoBehaviour
{
    Socket socket;
    IPEndPoint ipep;

    public string serverIP = "127.0.0.1";   //localhost as default
    public int serverPort = 9050;

    public string userName = "User";        //username of player

    public GameObject entitiesParent;
    public GameObject mapParent;

    public GameObject MainPlayer;

    public Dictionary<GameObject, int> entitiesGO;     //parent of entities go
    public Dictionary<GameObject, int> mapGO;          //parent of map go

    public GameObject loadingObj;

    bool socketCreated = false;

    public float updateToServerInSeconds = 0.2f;
    private float timer = 0.0f;

    private Queue<System.Action> mainThreadActions = new Queue<System.Action>();  // Cola de tareas para el hilo principal

    // Start is called before the first frame update
    void Start()
    {
        entitiesGO = new Dictionary<GameObject, int>();
        mapGO = new Dictionary<GameObject, int>();

        //first setting id 0, server will decide its real id
        //we can assume that FIRST element on this dictionary will be player
        foreach (Transform child in entitiesParent.transform)
        {
            entitiesGO.Add(child.gameObject, GenerateRandomID());
        }

        //first setting id 0, server will decide its real id
        //for the moment, map will stay still, it will have no changes
        foreach (Transform child in mapParent.transform)
        {
            mapGO.Add(child.gameObject, GenerateRandomID());
        }

        StartClient();
    }

    // Método para agregar tareas al hilo principal
    public void EnqueueMainThreadAction(System.Action action)
    {
        lock (mainThreadActions)
        {
            mainThreadActions.Enqueue(action);
        }
    }

    // Método Update que procesará las acciones en la cola
    void Update()
    {
        // Procesar todas las acciones que fueron encoladas desde otros hilos
        while (mainThreadActions.Count > 0)
        {
            System.Action action;
            lock (mainThreadActions)
            {
                action = mainThreadActions.Dequeue();
            }
            action?.Invoke();
        }

        // El resto de tu código de Update
        timer += Time.deltaTime;

        if (timer >= updateToServerInSeconds)
        {
            foreach (KeyValuePair<GameObject, int> entry in entitiesGO)
            {
                GameObject obj = entry.Key;
                int id = entry.Value;
                Vector2 pos = new Vector2(obj.GetComponent<Transform>().position.x, obj.GetComponent<Transform>().position.y);

                PlayerScript ps = obj.GetComponent<PlayerScript>();
                Thread mainThread = new Thread(() => Send(
                    Packet.Packet.PacketType.UPDATE,
                    id,
                    ps.userName,
                    pos,
                    ps.orientation,
                    ps.impostor,
                    ps.alive
                    ));
                mainThread.Start();
            }

            timer = 0.0f;
        }
    }

    public void StartClient()
    {
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

        // Iniciar el cliente y enviar el paquete de creación en el hilo secundario
        Thread mainThread = new Thread(() => Send(
            Packet.Packet.PacketType.CREATE,
            id,
            ps.userName,
            pos,
            ps.orientation,
            ps.impostor,
            ps.alive
            ));
        mainThread.Start();

        // Hilo de recepción de datos
        Thread receive = new Thread(Receive);
        receive.Start();
    }

    //RIGHT NOW THIS FUNCTION CREATES AND SENDS PACKETS OF ONLY 1 PLAYER, TODO: IMPROVE IT SO IT CAN PACKET INFO OF MORE SIZE
    void Send(Packet.Packet.PacketType typeP, int id, string name, Vector2 pos, bool orientation, bool impostor, bool alive)
    {
        try
        {
            //serializing packet
            Packet.Packet p = new Packet.Packet();
            p.Start();
            Packet.RegularDataPacket dataP = new Packet.RegularDataPacket(id, name, new System.Numerics.Vector2(pos.x, pos.y), orientation, impostor, alive);
            p.Serialize(typeP, dataP);

            //sending
            if (!socketCreated)
            {
                ipep = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);

                socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                socket.Connect(ipep);
                socketCreated = true;
            }

            socket.SendTo(p.GetMemoryStream().GetBuffer(), ipep);
            p.Close();
        }
        catch (System.Exception)
        {
            Debug.Log("Error Sending Regular Packet to server\n");
            throw;
        }
    }

    void Send(Packet.Packet.PacketType typeP, int id)
    {
        try
        {
            //serializing packet
            Packet.Packet p = new Packet.Packet();
            p.Start();
            Packet.DeleteDataPacket dataP = new Packet.DeleteDataPacket(id);
            p.Serialize(typeP, dataP);

            //sending
            if (!socketCreated)
            {
                ipep = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);

                socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                socket.Connect(ipep);
                socketCreated = true;
            }

            socket.SendTo(p.GetMemoryStream().GetBuffer(), ipep);
            p.Close();
        }
        catch (System.Exception)
        {
            Debug.Log("Error Sending Regular Packet to server\n");
            throw;
        }
    }

    void Send(Packet.Packet.PacketType typeP, int id, string name, string text)
    {
        try
        {
            //serializing packet
            Packet.Packet p = new Packet.Packet();
            p.Start();
            Packet.TextDataPacket dataP = new Packet.TextDataPacket(id, name, text);
            p.Serialize(typeP, dataP);

            //sending
            if (!socketCreated)
            {
                ipep = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);

                socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                socket.Connect(ipep);
                socketCreated = true;
            }

            socket.SendTo(p.GetMemoryStream().GetBuffer(), ipep);
            p.Close();
        }
        catch (System.Exception)
        {
            Debug.Log("Error Sending Regular Packet to server\n");
            throw;
        }
    }

    void Receive()
    {
        while (true)
        {
            byte[] data = new byte[1024];
            IPEndPoint sender = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);
            EndPoint Remote = (EndPoint)sender;

            if (socket == null)
            {
                Debug.Log("Socket is null");
            }
            int recv = socket.ReceiveFrom(data, ref Remote);

            // Deserializar los datos recibidos
            Packet.Packet p = new Packet.Packet(data);
            p.Start();

            try
            {
                Packet.Packet.PacketType pType = p.DeserializeGetType();

                switch (pType)
                {
                    case Packet.Packet.PacketType.CREATE:
                    case Packet.Packet.PacketType.UPDATE:
                        {
                            Packet.RegularDataPacket dsData = p.DeserializeRegularDataPacket();

                            // Enviar la tarea de actualización al hilo principal
                            EnqueueMainThreadAction(() =>
                            {
                                HandlePlayerData(dsData);
                            });

                            break;
                        }
                    case Packet.Packet.PacketType.DELETE:
                        {
                            Packet.DeleteDataPacket dsData = p.DeserializeDeleteDataPacket();

                            // Enviar la tarea de eliminación al hilo principal
                            EnqueueMainThreadAction(() =>
                            {
                                HandlePlayerDelete(dsData);
                            });

                            break;
                        }
                    case Packet.Packet.PacketType.TEXT:
                        {
                            Packet.TextDataPacket dsData = p.DeserializeTextDataPacket();
                            // Procesar el mensaje
                            break;
                        }
                    default:
                        break;
                }
            }
            catch (EndOfStreamException)
            {
                Debug.Log("End Of Stream Exception");
            }
            p.Close();
        }
    }

    // Método para manejar la actualización de los jugadores
    void HandlePlayerData(Packet.RegularDataPacket dsData)
    {
        bool create = true;
        foreach (KeyValuePair<GameObject, int> entry in entitiesGO)
        {
            GameObject obj = entry.Key;
            if (entry.Value == dsData.id)
            {
                PlayerScript playerScript = obj?.GetComponent<PlayerScript>();
                obj.transform.position = new Vector3(dsData.pos.X, dsData.pos.Y, 0);
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
            obj.transform.position = new Vector3(dsData.pos.X, dsData.pos.Y, 0);
            playerScript.ChangeName(dsData.name);
            playerScript.orientation = dsData.orientation;
            playerScript.impostor = dsData.impostor;
            playerScript.alive = dsData.alive;

            entitiesGO.Add(obj, dsData.id);
        }
    }

    // Método para manejar la eliminación de jugadores
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

    public void ChangeName(string name)
    {
        this.userName = name;
    }

    public void ChangeServerIP(string ip)
    {
        this.serverIP = ip;
    }

    //will generate a random id of 10 digits for the size of an integer
    public int GenerateRandomID()
    {
        var r = new System.Random();
        return r.Next(1000000000, 2147483647);
    }
}
