using UnityEngine;
using UnityEngine.UI;

class Menu_Connect : MonoBehaviour
{
    public InputField AddressField = null;

    public void Awake()
    {
        Session.Instance.Disconnect();
        ServerSession.Instance.StopServer();
    }
    
    public void OnConnectPressed()
    {
        ServerSession.Instance.StopServer();
        Session.Instance.Connect(AddressField.text);

        Application.LoadLevel("Menu_Connecting");
        this.enabled = false;
    }

    public void OnCreatePressed()
    {
        ServerSession.Instance.StartServer(ServerSession.DefaultPort);
        Session.Instance.Connect("localhost");

        Application.LoadLevel("Menu_Connecting");
        this.enabled = false;
    }
}