using Lidgren.Network;
using UnityEngine;
using System.Collections.Generic;

public class Server
{
    NetPeerConfiguration config;
    NetServer server;

    public delegate void LogDelegate(string log);
    private LogDelegate log = (s) => { };

    private const int MaxNumConnectedClients = 10;
    private const int MaxSlots = 4;

    private RemotePlayerSet players = new RemotePlayerSet();

    public void RegisterLogDelegate(LogDelegate logDelegate)
    {
        log += logDelegate;
    }

    public void DeregisterLogdelegate(LogDelegate logDelegate)
    {
        log -= logDelegate;
    }

    public Server(int port)
    {
        config = new NetPeerConfiguration("cars");
        config.Port = port;
        config.MaximumConnections = MaxNumConnectedClients;
        config.ConnectionTimeout = 15.0f;

        players.SetNumSlots(MaxSlots);

        players.OnPlayerSetSlot += OnPlayerSetSlot;
        players.OnPlayerAdded += OnPlayerAdded;

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

    public void Update()
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
                    log(text);
                    break;
                case NetIncomingMessageType.StatusChanged:
                    NetConnectionStatus status = (NetConnectionStatus)im.ReadByte();

                    string reason = im.ReadString();

                    log(status.ToString() + ": " + reason);

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
                    log("Unhandled type: " + im.MessageType + " " + im.LengthBytes + " bytes");
                    break;
            }
            server.Recycle(im);
        }
    }

    private NetOutgoingMessage CreateMessage(ServerToClientMessageCategory category)
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

        NetOutgoingMessage msg = CreateMessage(ServerToClientMessageCategory.Chat);
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

        if (players.ConnectedCount >= MaxNumConnectedClients)
        {
            connection.Disconnect("Server is full");
            log(string.Format("player connection attempt by {0}{1} rejected, server full", name, NetUtility.ToHexString(connection.RemoteUniqueIdentifier)));
            return;
        }
        
        RemotePlayer player = new RemotePlayer(connection.RemoteUniqueIdentifier, name);

        NetOutgoingMessage msg = CreateMessage(ServerToClientMessageCategory.SessionInit);
        msg.Write(players.SlotsCount);
        msg.Write(players.ConnectedCount);
        foreach(RemotePlayer connectedPlayer in players.ConnectedPlayers)
        {
            msg.Write(connectedPlayer.UniqueID);
            msg.Write(connectedPlayer.PlayerName);
            msg.Write(connectedPlayer.PlayerSlot);
        }
        server.SendMessage(msg, connection, NetDeliveryMethod.ReliableOrdered, 0);

        // Add player and try to default assign a slot
        players.AddPlayer(player, true);

        log(string.Format("player ({0} {1}) joined, slot {2}", name, NetUtility.ToHexString(connection.RemoteUniqueIdentifier), player.PlayerSlot));
    }
    
    private void OnPlayerAdded(RemotePlayer player)
    {
        // Nofity everyone the player was added
        NetOutgoingMessage msg = CreateMessage(ServerToClientMessageCategory.PlayerJoined);
        msg.Write(player.UniqueID);
        msg.Write(player.PlayerName);
        server.SendToAll(msg, null, NetDeliveryMethod.ReliableOrdered, 0);
    }

    private void OnPlayerSetSlot(RemotePlayer player)
    {
        // Notify all players on slot assignement change
        NetOutgoingMessage msg = CreateMessage(ServerToClientMessageCategory.PlayerSetSlot);
        msg.Write(player.UniqueID);
        msg.Write(player.PlayerSlot);
        server.SendToAll(msg, null, NetDeliveryMethod.ReliableOrdered, 0);
    }

    private void HandlePlayerDisconnected(NetConnection connection)
    {
        players.RemovePlayer(connection.RemoteUniqueIdentifier);

        // Notify everyone else the player left
        NetOutgoingMessage msg = CreateMessage(ServerToClientMessageCategory.PlayerLeft);
        msg.Write(connection.RemoteUniqueIdentifier);
        server.SendToAll(msg, connection, NetDeliveryMethod.ReliableOrdered, 0);
    }

    private void HandeIncomingDataMessage(NetIncomingMessage msg)
    {
        ClientToServerMessageCategory cat = (ClientToServerMessageCategory)msg.ReadByte();

        if (cat >= ClientToServerMessageCategory.MAX)
        {
            DropClient(msg.SenderConnection, "Server error, unknown message category received: {0:n}", cat);
            return;
        }

        switch(cat)
        {
            case ClientToServerMessageCategory.Chat:
                HandleChatMessage(msg);
                break;

            case ClientToServerMessageCategory.JoinSlot:
                HandleAttemptToJoinSlot(msg);
                break;

            default:
                log(string.Format("Unhandled message category {0}", cat));
                break;
        }
    }

    private void HandleChatMessage(NetIncomingMessage msg)
    {
        RemotePlayer sender = players.GetPlayer(msg.SenderConnection.RemoteUniqueIdentifier);
        string chat = msg.ReadString();
        SendChatMessageToAll(sender, chat);
    }
    
    private void HandleAttemptToJoinSlot(NetIncomingMessage msg)
    {
        int slot = msg.ReadInt32();
        players.MovePlayerToSlot(players.GetPlayer(msg.SenderConnection.RemoteUniqueIdentifier), slot);
    }

    private void DropClient(NetConnection connection, string message, params object[] args)
    {
        message = string.Format(message, args);

        log(message);
        connection.Disconnect(message);
    }
}