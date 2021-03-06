﻿using Lidgren.Network;
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

    private Gamemodes.ClientGameMode currentGameMode;

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

    public delegate void GameModeLaunchedDelegate(string mode);
    private GameModeLaunchedDelegate gameModeLaunchedHandler = (s) => { };

    public void RegisterGameModeLaunchDelegate(GameModeLaunchedDelegate gameModeLaunchedDelegate)
    {
        gameModeLaunchedHandler += gameModeLaunchedDelegate;
    }

    public void DeregisterGameModeLaunchDelegate(GameModeLaunchedDelegate gameModeLaunchedDelegate)
    {
        gameModeLaunchedHandler += gameModeLaunchedDelegate;
    }

    public RemotePlayerSet Players
    {
        get;
        private set;
    }

    public RemotePlayer LocalPlayer
    {
        get
        {
            return Players.GetPlayer(RemoteUniqueIdentifier);
        }
    }

    public long RemoteUniqueIdentifier
    {
        get
        {
            if (connection == null)
                return 0;

            return client.UniqueIdentifier;
        }
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

        Players = new RemotePlayerSet();
    }

    public void Disconnect(string reason)
    {
        client.Shutdown(reason);
        connection = null;

        Players = null;
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

        if (currentGameMode != null)
            currentGameMode.Update();
    }

    public NetOutgoingMessage CreateMessage(eClientToServerMessage category)
    {
        NetOutgoingMessage message = client.CreateMessage();
        message.Write((byte)category);
        return message;
    }

    public void SendChat(string chat)
    {
        NetOutgoingMessage msg = CreateMessage(eClientToServerMessage.Chat);
        msg.Write(chat);
        client.SendMessage(msg, NetDeliveryMethod.ReliableOrdered);
    }

    private void HandeIncomingDataMessage(NetIncomingMessage msg)
    {
        eServerToClientMessage cat = (eServerToClientMessage)msg.ReadByte();
        
        if (cat >= eServerToClientMessage.MAX)
        {
            client.Disconnect("server error");
            return;
        }

        switch(cat)
        {
            case eServerToClientMessage.SessionInit:
                HandleSessionInit(msg);
                break;

            case eServerToClientMessage.PlayerJoined:
                HandlePlayerJoined(msg);
                break;

            case eServerToClientMessage.PlayerSetSlot:
                HandlePlayerSetSlot(msg);
                break;

            case eServerToClientMessage.PlayerLeft:
                HandlePlayerLeft(msg);
                break;

            case eServerToClientMessage.PlayerSetStatus:
                HandlePlayerSetStatus(msg);
                break;

            case eServerToClientMessage.Chat:
                HandleChatMessage(msg);
                break;

            case eServerToClientMessage.GameModeLaunched:
                HandleGameModeLaunched(msg);
                break;

            case eServerToClientMessage.GameModeData:
                HandleGameModeData(msg);
                break;

            default:
                log(string.Format("Recieved message of unhandled category {0}", cat));
                break;
        }

    }

    private void HandleSessionInit(NetIncomingMessage msg)
    {
        int slots = msg.ReadInt32();
        Players.SetNumSlots(slots);
        
        int numconnected = msg.ReadInt32();
        for (int i = 0; i < numconnected; ++i)
        {
            long uniqueID = msg.ReadInt64();
            string name = msg.ReadString();
            int slot = msg.ReadInt32();
            RemotePlayerStatus status = (RemotePlayerStatus)msg.ReadByte();

            RemotePlayer player = new RemotePlayer(uniqueID, name);

            Players.AddPlayer(player, false);
            Players.MovePlayerToSlot(player, slot);
            Players.SetStatus(player, status);
        }   
    }

    private void HandlePlayerJoined(NetIncomingMessage msg)
    {
        long id = msg.ReadInt64();
        string name = msg.ReadString();
        RemotePlayerStatus status = (RemotePlayerStatus)msg.ReadByte();

        RemotePlayer player = new RemotePlayer(id, name);
        Players.AddPlayer(new RemotePlayer(id, name), false);
        Players.SetStatus(player, status);

        chatHandler(string.Format("{0} joined...", name));
    }

    private void HandlePlayerLeft(NetIncomingMessage msg)
    {
        long id = msg.ReadInt64();

        chatHandler(string.Format("{0} disconnected...", Players.GetPlayer(id).PlayerName));

        Players.RemovePlayer(id);
    }

    private void HandlePlayerSetSlot(NetIncomingMessage msg)
    {
        long id = msg.ReadInt64();
        int slot = msg.ReadInt32();

        Players.MovePlayerToSlot(Players.GetPlayer(id), slot);
    }

    private void HandlePlayerSetStatus(NetIncomingMessage msg)
    {
        long id = msg.ReadInt64();
        RemotePlayerStatus status = (RemotePlayerStatus)msg.ReadByte();

        Players.SetStatus(Players.GetPlayer(id), status);
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
            RemotePlayer sender = Players.GetPlayer(playerID);
            Color nameColor = PlayerIdentityGenerator.PlayerSlotToColor(sender.PlayerSlot);

            chatString = string.Format("<color=#{0}>[{1}]</color> {2}", nameColor.ToHexStringRGBA(), sender.PlayerName, chat);
        }

        log(chatString);
        chatHandler(chatString);
    }

    private void HandleGameModeLaunched(NetIncomingMessage msg)
    {
        string gameModeName = msg.ReadString();

        gameModeLaunchedHandler(gameModeName);
    }

    private void HandleGameModeData(NetIncomingMessage msg)
    {
        if (currentGameMode != null)
            currentGameMode.HandleIncomingMessage(msg);
    }

    public void JoinSlot(int slot)
    {
        NetOutgoingMessage msg = CreateMessage(eClientToServerMessage.JoinSlot);
        msg.Write(slot);
        client.SendMessage(msg, NetDeliveryMethod.ReliableOrdered);
    }

    public void SetStatus(RemotePlayerStatus status)
    {
        NetOutgoingMessage msg = CreateMessage(eClientToServerMessage.SetStatus);
        msg.Write((byte)status);
        client.SendMessage(msg, NetDeliveryMethod.ReliableOrdered);
    }

    public void LaunchGame()
    {
        NetOutgoingMessage msg = CreateMessage(eClientToServerMessage.StartGame);
        client.SendMessage(msg, NetDeliveryMethod.ReliableOrdered);
    }

    public void AttachGameMode(Gamemodes.ClientGameMode gameMode)
    {
        currentGameMode = gameMode;
        gameMode.OnAttachedToClient(this);
    }

    public void Send(NetOutgoingMessage message, NetDeliveryMethod method)
    {
        client.SendMessage(message, method);
    }

    public void Log(string output)
    {
        log(output);
    }
}
