using UnityEngine;
using System.Collections;
using Lidgren.Network;
using System;

public class Session : MonoBehaviour
{
    public static Session Instance { get; private set; }

    private Server server = null;
    private Client client = null;

    const int defaultServerPort = 9995;

    public void Awake()
    {
        Instance = this;
    }

    public void Create()
    {
        Disconnect();

        int port = defaultServerPort;

        server = new Server(port);
        server.RegisterLogDelegate(logServer);
        server.Start();

        client = new Client();
        client.RegisterLogDelegate(logClient);
        client.Connect("localhost", port);
    }

    public void Connect(string addr)
    {
        Disconnect();

        string host;
        int port;
        ParseAddr(addr, out host, out port);

        client = new Client();
        client.RegisterLogDelegate(logClient);
        client.Connect(host, port);
    }

    public void Disconnect()
    {
        if (client != null)
        {
            client.Disconnect("Player disconnected");
        }

        if (server != null)
        {
            server.Stop("Server shutting down");
        }
    }

    private void ParseAddr(string addr, out string host, out int port)
    {
        string[] hostparts = addr.Split(':');

        if (hostparts.Length != 1 && hostparts.Length != 2)
            throw new ArgumentException("Expected \"host:port\" or \"host\"");

        host = hostparts[0];
        port = defaultServerPort;

        if (hostparts.Length == 2)
            port = System.Convert.ToInt32(hostparts[1]);
    }
    
    public void Update()
    {
        if (client != null)
            client.Update();

        if (server != null)
            server.Update();
    }

    public void RegisterChatDelegate(Client.ChatDelegate chatDelegate)
    {
        client.RegisterChatDelegate(chatDelegate);
    }

    public void DeregisterChatdelegate(Client.ChatDelegate chatDelegate)
    {
        client.DeregisterChatDelegate(chatDelegate);
    }

    public void SendChat(string chat)
    {
        client.SendChat(chat);
    }

    private void logClient(string log)
    {
        Debug.Log("<color=#FFFF00><b>[CLIENT]</b></color> " + log);
    }
    
    private void logServer(string log)
    {
        Debug.Log("<color=#FF00FF><b>[SERVER]</b></color> " + log);
    }

    public bool Connecting
    {
        get
        {
            return client != null && client.IsConnecting;
        }
    }

    public bool Connected
    {
        get
        {
            return client != null && client.IsConnected;
        }
    }

    public string LastClientStatusMessage
    {
        get
        {
            if (client == null)
                return string.Empty;

            return client.LastStatusMessage;
        }
    }
}
