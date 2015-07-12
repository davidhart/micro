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
        connection = client.Connect(host, port);
    }

    public void Disconnect(string reason)
    {
        client.Shutdown(reason);
        connection = null;
    }

    public bool IsConnecting
    {
        get
        {
            return connection != null && connection.Status != NetConnectionStatus.Disconnected && connection.Status != NetConnectionStatus.Connected;
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

                    long playerId = im.ReadInt64();
                    string chat = im.ReadString();

                    string chatString;

                    if (playerId == 0)
                        chatString = chat;
                    else
                        chatString = string.Format("[{0}] {1}", PlayerIdentityGenerator.PlayerIDToColorNameString(playerId), chat);

                    log(chatString);

                    chatHandler(chatString);
                    break;
                default:
                    log("Unhandled type: " + im.MessageType + " " + im.LengthBytes + " bytes");
                    break;
            }
            client.Recycle(im);
        }
    }

    public void SendChat(string chat)
    {
        NetOutgoingMessage msg = client.CreateMessage(chat);
        client.SendMessage(msg, NetDeliveryMethod.ReliableOrdered);
    }
}
