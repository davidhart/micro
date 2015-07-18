using Lidgren.Network;
using UnityEngine;
using System.Collections.Generic;
using Gamemodes;

public class Server
{
    NetPeerConfiguration config;
    NetServer server;
    ServerGameMode currentGameMode;

    public delegate void LogDelegate(string log);
    private LogDelegate logCallback = (s) => { };

    private const int MaxNumConnectedClients = 10;
    private const int MaxSlots = 4;

    public RemotePlayerSet Players { get; private set; }

    public void RegisterLogDelegate(LogDelegate logCallback)
    {
        this.logCallback += logCallback;
    }

    public void DeregisterLogdelegate(LogDelegate logCallback)
    {
        this.logCallback -= logCallback;
    }

    public Server(int port)
    {
        config = new NetPeerConfiguration("cars");
        config.Port = port;
        config.MaximumConnections = MaxNumConnectedClients;
        config.ConnectionTimeout = 15.0f;

        Players = new RemotePlayerSet();
        Players.SetNumSlots(MaxSlots);
        Players.OnPlayerSetSlot += OnPlayerSetSlot;
        Players.OnPlayerAdded += OnPlayerAdded;
        Players.OnPlayerStatusChanged += OnPlayerStatusChanged;

        server = new NetServer(config);
    }

    public void Start()
    {
        server.Start();
    }

    public void Stop(string reason)
    {
        server.Shutdown(reason);
    }

    public void Update(float dt)
    {
        HandleIncomingData();

        if (currentGameMode != null)
        {
            currentGameMode.Update(dt);
        }
    }

    private void HandleIncomingData()
    {
        NetIncomingMessage im;
        while ((im = server.ReadMessage()) != null)
        {
            // handle incoming message
            switch (im.MessageType)
            {
                case NetIncomingMessageType.DebugMessage:
                case NetIncomingMessageType.ErrorMessage:
                case NetIncomingMessageType.WarningMessage:
                case NetIncomingMessageType.VerboseDebugMessage:
                    string text = im.ReadString();
                    Log(text);
                    break;
                case NetIncomingMessageType.StatusChanged:
                    NetConnectionStatus status = (NetConnectionStatus)im.ReadByte();

                    string reason = im.ReadString();

                    Log(status.ToString() + ": " + reason);

                    if (status == NetConnectionStatus.Connected)
                    {
                        HandlePlayerConnected(im.SenderConnection);
                    }
                    else if (status == NetConnectionStatus.Disconnected)
                    {
                        HandlePlayerDisconnected(im.SenderConnection);
                    }

                    break;
                case NetIncomingMessageType.Data:
                    HandeIncomingDataMessage(im);
                    break;
                default:
                    Log("Unhandled type: " + im.MessageType + " " + im.LengthBytes + " bytes");
                    break;
            }
            server.Recycle(im);
        }
    }

    public NetOutgoingMessage CreateMessage(eServerToClientMessage category)
    {
        NetOutgoingMessage msg = server.CreateMessage();
        msg.Write((byte)category);
        return msg;
    }

    private void SendChatMessageToAll(RemotePlayer sender, string message)
    {
        if (string.IsNullOrEmpty(message))
            return;

        if (server.ConnectionsCount == 0)
            return;

        NetOutgoingMessage msg = CreateMessage(eServerToClientMessage.Chat);
        if (sender == null)
            msg.Write((long)0);
        else
            msg.Write(sender.UniqueID);

        msg.Write(message);
        server.SendMessage(msg, server.Connections, NetDeliveryMethod.ReliableOrdered, 0);
    }

    private void HandlePlayerConnected(NetConnection connection)
    {
        string name = connection.RemoteHailMessage.ReadString();

        if (Players.ConnectedCount >= MaxNumConnectedClients)
        {
            connection.Disconnect("Server is full");
            Log(string.Format("player connection attempt by {0}{1} rejected, server full", name, NetUtility.ToHexString(connection.RemoteUniqueIdentifier)));
            return;
        }

        if(currentGameMode != null && currentGameMode.AllowJoinInProgress == false)
        {
            connection.Disconnect("Cannot connect to game already in progress");
            Log(string.Format("player connection attempt by {0}{1} rejected, gamemode in progress", name, NetUtility.ToHexString(connection.RemoteUniqueIdentifier)));
            return;
        }
        
        RemotePlayer player = new RemotePlayer(connection.RemoteUniqueIdentifier, name);

        NetOutgoingMessage msg = CreateMessage(eServerToClientMessage.SessionInit);
        msg.Write(Players.SlotsCount);
        msg.Write(Players.ConnectedCount);
        foreach (RemotePlayer connectedPlayer in Players.ConnectedPlayers)
        {
            msg.Write(connectedPlayer.UniqueID);
            msg.Write(connectedPlayer.PlayerName);
            msg.Write(connectedPlayer.PlayerSlot);
            msg.Write((byte)connectedPlayer.Status);
        }
        server.SendMessage(msg, connection, NetDeliveryMethod.ReliableOrdered, 0);

        // Add player and try to default assign a slot
        Players.AddPlayer(player, true);

        Log(string.Format("player ({0} {1}) joined, slot {2}", name, NetUtility.ToHexString(connection.RemoteUniqueIdentifier), player.PlayerSlot));
    }
    
    private void OnPlayerAdded(RemotePlayer player)
    {
        // Nofity everyone the player was added
        NetOutgoingMessage msg = CreateMessage(eServerToClientMessage.PlayerJoined);
        msg.Write(player.UniqueID);
        msg.Write(player.PlayerName);
        msg.Write((byte)player.Status);
        server.SendToAll(msg, null, NetDeliveryMethod.ReliableOrdered, 0);

        LobbySettingsChanged();
    }

    private void OnPlayerSetSlot(RemotePlayer player)
    {
        // Notify all players on slot assignement change
        NetOutgoingMessage msg = CreateMessage(eServerToClientMessage.PlayerSetSlot);
        msg.Write(player.UniqueID);
        msg.Write(player.PlayerSlot);
        server.SendToAll(msg, null, NetDeliveryMethod.ReliableOrdered, 0);

        LobbySettingsChanged();
    }
    
    private void OnPlayerStatusChanged(RemotePlayer player)
    {
        NetOutgoingMessage msg = CreateMessage(eServerToClientMessage.PlayerSetStatus);
        msg.Write(player.UniqueID);
        msg.Write((byte)player.Status);
        server.SendToAll(msg, null, NetDeliveryMethod.ReliableOrdered, 0);
    }

    private void HandlePlayerDisconnected(NetConnection connection)
    {
        Players.RemovePlayer(connection.RemoteUniqueIdentifier);

        // Notify everyone else the player left
        NetOutgoingMessage msg = CreateMessage(eServerToClientMessage.PlayerLeft);
        msg.Write(connection.RemoteUniqueIdentifier);
        server.SendToAll(msg, connection, NetDeliveryMethod.ReliableOrdered, 0);

        LobbySettingsChanged();
    }

    private void HandeIncomingDataMessage(NetIncomingMessage msg)
    {
        eClientToServerMessage cat = (eClientToServerMessage)msg.ReadByte();

        if (cat >= eClientToServerMessage.MAX)
        {
            DropClient(msg.SenderConnection, "Server error, unknown message category received: {0:n}", cat);
            return;
        }

        switch(cat)
        {
            case eClientToServerMessage.Chat:
                HandleChatMessage(msg);
                break;

            case eClientToServerMessage.JoinSlot:
                HandleAttemptToJoinSlot(msg);
                break;

            case eClientToServerMessage.SetStatus:
                HandleSetStatus(msg);
                break;

            case eClientToServerMessage.StartGame:
                HandleLaunchGame(msg);
                break;

            default:
                Log(string.Format("Unhandled message category {0}", cat));
                break;
        }
    }

    private void HandleChatMessage(NetIncomingMessage msg)
    {
        RemotePlayer sender = Players.GetPlayer(msg.SenderConnection.RemoteUniqueIdentifier);
        string chat = msg.ReadString();
        SendChatMessageToAll(sender, chat);
    }
    
    private void HandleAttemptToJoinSlot(NetIncomingMessage msg)
    {
        if (currentGameMode != null && currentGameMode.AllowJoinInProgress == false)
            return;

        int slot = msg.ReadInt32();
        Players.MovePlayerToSlot(Players.GetPlayer(msg.SenderConnection.RemoteUniqueIdentifier), slot);
    }

    private void HandleSetStatus(NetIncomingMessage msg)
    {
        if (currentGameMode != null && currentGameMode.AllowJoinInProgress == false)
            return;

        RemotePlayerStatus status = (RemotePlayerStatus)msg.ReadByte();

        RemotePlayer sender = Players.GetPlayer(msg.SenderConnection.RemoteUniqueIdentifier);

        Players.SetStatus(sender, status);
    }

    private void HandleLaunchGame(NetIncomingMessage msg)
    {
        if (currentGameMode != null)
            return;

        if (Players.AllPlayerStatusEquals(RemotePlayerStatus.LobbyReady))
        {
            LaunchGameMode("cars");
        }
    }

    public void DropClient(NetConnection connection, string message, params object[] args)
    {
        message = string.Format(message, args);

        Log(message);
        connection.Disconnect(message);
    }

    private void LobbySettingsChanged()
    {
        Players.SetAllStatus(RemotePlayerStatus.LobbyNotReady);
    }

    private void LaunchGameMode(string name)
    {
        NetOutgoingMessage message = CreateMessage(eServerToClientMessage.GameModeLaunched);
        message.Write(name);
        server.SendToAll(message, null, NetDeliveryMethod.ReliableOrdered, 0);

        currentGameMode = new Gamemodes.Cars.ServerGamemode(this);
    }

    public void Log(string message, params object[] args)
    {
        string logMessage = string.Format(message, args);

        logCallback(logMessage);
    }

    public void SendToAll(NetOutgoingMessage message, NetConnection except, NetDeliveryMethod method, int sequenceChannel)
    {
        server.SendToAll(message, except, method, sequenceChannel);
    }
}