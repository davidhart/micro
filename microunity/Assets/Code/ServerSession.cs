using UnityEngine;

class ServerSession : MonoBehaviour
{
    public static ServerSession Instance { get; private set; }

    private Server server;

    public const int DefaultPort = 9995;

    public void Awake()
    {
        Instance = this;
    }

    public void StartServer(int port)
    {
        StopServer();
        server = new Server(port);
        server.RegisterLogDelegate(logServer);
        server.Start();
    }

    public void StopServer()
    {
        if (server != null)
        {
            server.Stop("Server shutting down");
        }
    }
    
    public void Update()
    {
        if (server != null)
        {
            server.Update(Time.deltaTime);
        }
    }

    private void logServer(string log)
    {
        Debug.Log("<color=#FF00FF><b>[SERVER]</b></color> " + log);
    }
}
