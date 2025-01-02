using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;

namespace Packet
{
    public class RegularDataPacket
    {
        public int id;
        public string name;
        public Vector2 pos;
        public Vector2 vel;
        public bool orientation;
        public bool impostor;
        public bool alive;

        public RegularDataPacket(int id, string name, Vector2 pos, Vector2 vel, bool orientation, bool impostor, bool alive)
        {
            this.id = id;
            this.name = name;
            this.pos = pos;
            this.vel = vel;
            this.orientation = orientation;
            this.impostor = impostor;
            this.alive = alive;
        }
    }

    public class DeleteDataPacket
    {
        public int id;

        public DeleteDataPacket(int id)
        {
            this.id = id;
        }
    }

    public class TextDataPacket
    {
        public int id;
        public string name;
        public string text;

        public TextDataPacket(int id, string name, string text)
        {
            this.id = id;
            this.name = name;
            this.text = text;
        }
    }

    public class KillActionDataPacket
    {
        public int idKiller;
        public int idVictim;

        public KillActionDataPacket(int idKiller, int idVictim)
        {
            this.idKiller = idKiller;
            this.idVictim = idVictim;
        }
    }

    public class CompleteTaskActionDataPacket
    {
        public int id;
        public int playerCompletedTasks;

        public CompleteTaskActionDataPacket(int id, int playerCompletedTasks)
        {
            this.id = id;
            this.playerCompletedTasks = playerCompletedTasks;
        }
    }

    public class TriggerReportActionDataPacket
    {
        public int id;

        public TriggerReportActionDataPacket(int id)
        {
            this.id = id;
        }
    }

    public class VoteActionDataPacket
    {
        public int idVoter;
        public int idVoted;

        public VoteActionDataPacket(int idVoter, int idVoted)
        {
            this.idVoter = idVoter;
            this.idVoted = idVoted;
        }
    }

    public class StartGameActionDataPacket
    {
        public int idImpostor;

        public StartGameActionDataPacket(int idImpostor)
        {
            this.idImpostor = idImpostor;
        }
    }

    public class ChangeStateDataPacket
    {
        public GameManager.GameState gameState;

        public ChangeStateDataPacket(GameManager.GameState gameState)
        {
            this.gameState = gameState;
        }   
    }

    public class MSCheckerDataPacket
    {
        public int id;
        public Int16 ms;
        
        public MSCheckerDataPacket(int id, Int16 ms)
        {
            this.id = id;
            this.ms = ms;
        }
    }

    public class Packet
    {
        public enum PacketType : Int16
        {
            CREATE,
            //int16 packet type
            //int id
            //string name
            //vec2 pos
            //bool orientation
            //bool impostor
            //bool alive
            UPDATE,
            //int16 packet type
            //int id
            //string name
            //vec2 pos
            //bool orientation
            //bool impostor
            //bool alive
            DELETE,
            //int16 packet type
            //int id
            TEXT,
            //int16 packet type
            //int id
            //string userName
            //string text
            ACTION,
            //int16 packet type
            //actiontype data
            MSCHECKER,
            //int16 packet type
            //int16 ms
        }

        public enum ActionType : Int16
        {
            KILL,
            //int16 packet type
            //int id killer
            //int id victim
            COMPLETETASK,
            //int16 packet type
            //int id
            //int player completed tasks
            TRIGGERREPORT,
            //int16 packet type
            //int id
            VOTE,
            //int16 packet type
            //int id voter
            //int id voted
            STARTGAME,
            //int16 packet type
            //int id impostor
            GAMESTATE,
            //int16 packet type
            //GameState new game state
        }

        private MemoryStream ms;
        private BinaryReader reader;
        private BinaryWriter writer;

        private Int16 goNumber = 0;

        public Packet()
        {
            this.ms = new MemoryStream();
            this.writer = new BinaryWriter(ms);
            this.goNumber = 0;
        }

        public Packet(byte[] data)
        {
            this.ms = new MemoryStream(data);
            this.reader = new BinaryReader(ms);
            this.goNumber = 0;
        }

        public void Start()
        {
            ms.Seek(0, SeekOrigin.Begin);
        }

        public void Init()
        {
            this.ms = new MemoryStream();
            this.writer = new BinaryWriter(ms);
            this.goNumber = 0;
        }

        public void Init(byte[] data)
        {
            this.ms = new MemoryStream(data);
            this.reader = new BinaryReader(ms);
            this.goNumber = 0;
        }

        public void Close()
        {
            ms?.Close();
            reader?.Close();
            writer?.Close();
            goNumber = 0;
        }

        public void Restart()
        {
            Close();
            Init();
        }

        public void Restart(byte[] data)
        {
            Close();
            Init(data);
        }

        //deserializing
        public int DeserializeGetGameObjectsAmount()
        {
            return (int)reader.ReadInt16();
        }
        
        public PacketType DeserializeGetType()
        {
            return (PacketType)reader.ReadInt16();
        }

        public RegularDataPacket DeserializeRegularDataPacket()
        {
            int id = reader.ReadInt32();
            string name = reader.ReadString();
            Vector2 pos = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            Vector2 vel = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            bool orientation = reader.ReadBoolean();
            bool impostor = reader.ReadBoolean();
            bool alive = reader.ReadBoolean();

            return new RegularDataPacket(id, name, pos, vel, orientation, impostor, alive);
        }

        public DeleteDataPacket DeserializeDeleteDataPacket()
        {
            int id = reader.ReadInt32();

            return new DeleteDataPacket(id);
        }

        public TextDataPacket DeserializeTextDataPacket()
        {
            int id = reader.ReadInt32();
            string name = reader.ReadString();
            string text = reader.ReadString();

            return new TextDataPacket(id, name, text);
        }

        //actions
        public ActionType DeserializeGetActionType()
        {
            return (ActionType)reader.ReadInt16();
        }

        public KillActionDataPacket DeserializeKillActionDataPacket()
        {
            int idKiller = reader.ReadInt32();
            int idVictim = reader.ReadInt32();

            return new KillActionDataPacket(idKiller, idVictim);
        }

        public CompleteTaskActionDataPacket DeserializeTaskActionDataPacket()
        {
            int id = reader.ReadInt32();
            int playerCompletedTasks = reader.ReadInt32();

            return new CompleteTaskActionDataPacket(id, playerCompletedTasks);
        }

        public TriggerReportActionDataPacket DeserializeReportActionDataPacket()
        {
            int id = reader.ReadInt32();

            return new TriggerReportActionDataPacket(id);
        }

        public VoteActionDataPacket DeserializeVoteActionDataPacket()
        {
            int idVoter = reader.ReadInt32();
            int idVoted = reader.ReadInt32();

            return new VoteActionDataPacket(idVoter, idVoted);
        }

        public StartGameActionDataPacket DeserializeStartGameActionDataPacket()
        {
            int idImpostor = reader.ReadInt32();

            return new StartGameActionDataPacket(idImpostor);
        }

        public ChangeStateDataPacket DeserializeChangeStateDataPacket()
        {
            GameManager.GameState gameManager = (GameManager.GameState)reader.ReadInt16();

            return new ChangeStateDataPacket(gameManager);
        }

        public MSCheckerDataPacket DeserializeMSCheckerDataPacket()
        {
            int id = reader.ReadInt32();
            Int16 ms = reader.ReadInt16();

            return new MSCheckerDataPacket(id, ms);
        }

        //serializing
        public void Serialize(PacketType type, RegularDataPacket data)
        {
            if (ms.Position == 0)
            {
                // Reserve 2 bytes for the goNumber (Int16)
                writer.Write((Int16)goNumber);
            }
            writer.Write((Int16)type);
            writer.Write(data.id);
            writer.Write(data.name);
            writer.Write(data.pos.X);
            writer.Write(data.pos.Y);
            writer.Write(data.vel.X);
            writer.Write(data.vel.Y);
            writer.Write(data.orientation);
            writer.Write(data.impostor);
            writer.Write(data.alive);

            this.goNumber++;
        }

        public void Serialize(PacketType type, DeleteDataPacket data)
        {
            if (ms.Position == 0)
            {
                // Reserve 2 bytes for the goNumber (Int16)
                writer.Write((Int16)goNumber);
            }
            writer.Write((Int16)type);
            writer.Write(data.id);

            this.goNumber++;
        }

        public void Serialize(PacketType type, TextDataPacket data)
        {
            if (ms.Position == 0)
            {
                // Reserve 2 bytes for the goNumber (Int16)
                writer.Write((Int16)goNumber);
            }
            writer.Write((Int16)type);
            writer.Write(data.id);
            writer.Write(data.name);
            writer.Write(data.text);

            this.goNumber++;
        }

        //actions
        public void Serialize(PacketType type, KillActionDataPacket data)
        {
            if (ms.Position == 0)
            {
                // Reserve 2 bytes for the goNumber (Int16)
                writer.Write((Int16)goNumber);
            }
            writer.Write((Int16)type);
            writer.Write((Int16)ActionType.KILL);
            writer.Write(data.idKiller);
            writer.Write(data.idVictim);

            this.goNumber++;
        }

        public void Serialize(PacketType type, CompleteTaskActionDataPacket data)
        {
            if (ms.Position == 0)
            {
                // Reserve 2 bytes for the goNumber (Int16)
                writer.Write((Int16)goNumber);
            }
            writer.Write((Int16)type);
            writer.Write((Int16)ActionType.COMPLETETASK);
            writer.Write(data.id);
            writer.Write(data.playerCompletedTasks);

            this.goNumber++;
        }

        public void Serialize(PacketType type, TriggerReportActionDataPacket data)
        {
            if (ms.Position == 0)
            {
                // Reserve 2 bytes for the goNumber (Int16)
                writer.Write((Int16)goNumber);
            }
            writer.Write((Int16)type);
            writer.Write((Int16)ActionType.TRIGGERREPORT);
            writer.Write(data.id);

            this.goNumber++;
        }

        public void Serialize(PacketType type, VoteActionDataPacket data)
        {
            if (ms.Position == 0)
            {
                // Reserve 2 bytes for the goNumber (Int16)
                writer.Write((Int16)goNumber);
            }
            writer.Write((Int16)type);
            writer.Write((Int16)ActionType.VOTE);
            writer.Write(data.idVoter);
            writer.Write(data.idVoted);

            this.goNumber++;
        }

        public void Serialize(PacketType type, StartGameActionDataPacket data)
        {
            if (ms.Position == 0)
            {
                // Reserve 2 bytes for the goNumber (Int16)
                writer.Write((Int16)goNumber);
            }
            writer.Write((Int16)type);
            writer.Write((Int16)ActionType.STARTGAME);
            writer.Write(data.idImpostor);

            this.goNumber++;
        }

        public void Serialize(PacketType type, ChangeStateDataPacket data)
        {
            if (ms.Position == 0)
            {
                // Reserve 2 bytes for the goNumber (Int16)
                writer.Write((Int16)goNumber);
            }
            writer.Write((Int16)type);
            writer.Write((Int16)ActionType.GAMESTATE);
            writer.Write((Int16)data.gameState);

            this.goNumber++;
        }

        public void Serialize(PacketType type, MSCheckerDataPacket data)
        {
            if (ms.Position == 0)
            {
                // Reserve 2 bytes for the goNumber (Int16)
                writer.Write((Int16)goNumber);
            }
            writer.Write((Int16)type);
            writer.Write(data.id);
            writer.Write((Int16)data.ms);
        }

        //send
        public void Send(ref Socket socket, IPEndPoint ipep)
        {
            Start();
            writer.Write((Int16)this.goNumber);
            socket.SendTo(this.ms.GetBuffer(), ipep);
        }

        //getters
        public MemoryStream GetMemoryStream() { return this.ms; }

        public BinaryReader GetBinaryReader() { return this.reader; }

        public BinaryWriter GetBinaryWriter() { return this.writer; }

        public int GetGameObjectsAmount() { return (int)this.goNumber; }
    }
}