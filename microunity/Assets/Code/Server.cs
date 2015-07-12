using Lidgren.Network;
using UnityEngine;

public class Server
{
    NetPeerConfiguration config;
    NetServer server;

    public delegate void LogDelegate(string log);
    private LogDelegate log = (s) => { };

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
        config.MaximumConnections = 8;
        config.ConnectionTimeout = 15.0f;

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
                        SendChatMessageToAll(0, string.Format("{0} Joined", PlayerIdentityGenerator.PlayerIDToName(im.SenderConnection.RemoteUniqueIdentifier)));
                    }
                    else if (status == NetConnectionStatus.Disconnected)
                    {
                        SendChatMessageToAll(0, string.Format("{0} Disconnected ({1})", PlayerIdentityGenerator.PlayerIDToName(im.SenderConnection.RemoteUniqueIdentifier), reason));
                    }

                    break;
                case NetIncomingMessageType.Data:
                    string chat = im.ReadString();

                    SendChatMessageToAll(im.SenderConnection.RemoteUniqueIdentifier, chat);

                    break;
                default:
                    log("Unhandled type: " + im.MessageType + " " + im.LengthBytes + " bytes");
                    break;
            }
            server.Recycle(im);
        }
    }

    private void SendChatMessageToAll(long sender, string message)
    {
        if (string.IsNullOrEmpty(message))
            return;

        if (server.ConnectionsCount == 0)
            return;

        log(message);
        NetOutgoingMessage msg = server.CreateMessage();
        msg.Write(sender);
        msg.Write(message);
        server.SendMessage(msg, server.Connections, NetDeliveryMethod.ReliableOrdered, 0);
    }
}