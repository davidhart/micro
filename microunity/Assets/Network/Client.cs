using Lidgren.Network;
using UnityEngine;

public class Client
{
    NetPeerConfiguration config;
    NetClient client = null;
    NetConnection connection = null;

    public delegate void LogDelegate(string log);
    private LogDelegate log = (s) => { };

    public delegate void ChatDelegate(string chat);
    private ChatDelegate chatHandler = (s) => { };

    private string lastStatusMessage = string.Empty;
    
    private bool hasReceivedStatus = false;
    private RemotePlayerSet players = null;

    public void RegisterLogDelegate(LogDelegate logDelegate)
    {
        log += logDelegate;
    }

    public void DeregisterLogDelegate(LogDelegate logDelegate)
    {
        log -= logDelegate;
    }

    public void RegisterChatDelegate(ChatDelegate chatDelegate)
    {
        chatHandler += chatDelegate;
    }

    public void DeregisterChatDelegate(ChatDelegate chatDelegate)
    {
        chatHandler -= chatDelegate;
    }

    public Client()
    {
        config = new NetPeerConfiguration("cars");
        config.ConnectionTimeout = 15.0f;

        client = new NetClient(config);
    }

    public void Connect(string host, int port)
    {
        client.Start();

        NetOutgoingMessage msg = client.CreateMessage();
        msg.Write(PlayerIdentityGenerator.PlayerIDToName(client.UniqueIdentifier));

        connection = client.Connect(host, port, msg);

        players = new RemotePlayerSet();
    }

    public void Disconnect(string reason)
    {
        client.Shutdown(reason);
        connection = null;

        players = null;
    }

    public bool IsConnecting
    {
        get
        {
            return connection != null && connection.Status != NetConnectionStatus.Disconnected && connection.Status != NetConnectionStatus.Connected && hasReceivedStatus == false;
        }
    }

    public bool IsConnected
    {
        get
        {
            return connection != null && client.ConnectionStatus == NetConnectionStatus.Connected;
        }
    }

    public string LastStatusMessage
    {
        get
        {
            return lastStatusMessage;
        }
    }

    public void Update()
    {
        NetIncomingMessage im;
        while ((im = client.ReadMessage()) != null)
        {
            // handle incoming message
            switch (im.MessageType)
            {
                case NetIncomingMessageType.DebugMessage:
                case NetIncomingMessageType.ErrorMessage:
                case NetIncomingMessageType.WarningMessage:
                case NetIncomingMessageType.VerboseDebugMessage:
                    string text = im.ReadString();
                    log(text);
                    break;

                case NetIncomingMessageType.StatusChanged:

                    NetConnectionStatus status = (NetConnectionStatus)im.ReadByte();
                    string reason = im.ReadString();
                    log(status.ToString() + ": " + reason);
                    lastStatusMessage = reason;

                    break;

                case NetIncomingMessageType.Data:

                    HandeIncomingDataMessage(im);
                    break;
                default:
                    log("Unhandled type: " + im.MessageType + " " + im.LengthBytes + " bytes");
                    break;
            }
            client.Recycle(im);
        }
    }

    private NetOutgoingMessage CreateMessage(ClientToServerMessageCategory category)
    {
        NetOutgoingMessage message = client.CreateMessage();
        message.Write((byte)category);
        return message;
    }

    public void SendChat(string chat)
    {
        NetOutgoingMessage msg = CreateMessage(ClientToServerMessageCategory.Chat);
        msg.Write(chat);
        client.SendMessage(msg, NetDeliveryMethod.ReliableOrdered);
    }

    private void HandeIncomingDataMessage(NetIncomingMessage msg)
    {
        ServerToClientMessageCategory cat = (ServerToClientMessageCategory)msg.ReadByte();
        
        if (cat >= ServerToClientMessageCategory.MAX)
        {
            client.Disconnect("server error");
            return;
        }

        switch(cat)
        {
            case ServerToClientMessageCategory.SessionInit:
                HandleSessionInit(msg);
                break;

            case ServerToClientMessageCategory.PlayerJoined:
                HandlePlayerJoined(msg);
                break;

            case ServerToClientMessageCategory.PlayerSetSlot:
                HandlePlayerSetSlot(msg);
                break;

            case ServerToClientMessageCategory.PlayerLeft:
                HandlePlayerLeft(msg);
                break;

            case ServerToClientMessageCategory.Chat:
                HandleChatMessage(msg);
                break;

            default:
                log(string.Format("Recieved message of unhandled category {0}", cat));
                break;
        }

    }

    private void HandleSessionInit(NetIncomingMessage msg)
    {
        int slots = msg.ReadInt32();
        players.SetNumSlots(slots);
        
        int numconnected = msg.ReadInt32();
        for (int i = 0; i < numconnected; ++i)
        {
            long uniqueID = msg.ReadInt64();
            string name = msg.ReadString();

            RemotePlayer player = new RemotePlayer(uniqueID, name);

            players.AddPlayer(player, false);

            int slot = msg.ReadInt32();
            players.MovePlayerToSlot(player, slot);
        }   
    }

    private void HandlePlayerJoined(NetIncomingMessage msg)
    {
        long id = msg.ReadInt64();
        string name = msg.ReadString();

        players.AddPlayer(new RemotePlayer(id, name), false);
    }

    private void HandlePlayerLeft(NetIncomingMessage msg)
    {
        long id = msg.ReadInt64();

        players.RemovePlayer(id);
    }

    private void HandlePlayerSetSlot(NetIncomingMessage msg)
    {
        long id = msg.ReadInt64();
        int slot = msg.ReadInt32();

        players.MovePlayerToSlot(players.GetPlayer(id), slot);
    }

    private void HandleChatMessage(NetIncomingMessage msg)
    {
        long playerID = msg.ReadInt64();
        string chat = msg.ReadString();

        string chatString;

        if (playerID == 0)
        {
            chatString = chat;
        }
        else
        {
            RemotePlayer sender = players.GetPlayer(playerID);
            Color nameColor = PlayerIdentityGenerator.PlayerIDToColor(sender.PlayerSlot);

            chatString = string.Format("<color=#{0}>[{1}]</color> {2}", nameColor.ToHexStringRGBA(), sender.PlayerName, chat);
        }

        log(chatString);
        chatHandler(chatString);
    }
}
