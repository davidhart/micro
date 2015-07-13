using UnityEngine;
using UnityEngine.UI;

class Menu_Connect : MonoBehaviour
{
    public InputField AddressField = null;
    
    public void OnConnectPressed()
    {
        ServerSession.Instance.Stop();
        Session.Instance.Connect(AddressField.text);

        Application.LoadLevel("Menu_Connecting");
        this.enabled = false;
    }

    public void OnCreatePressed()
    {
        ServerSession.Instance.Start(ServerSession.DefaultPort);
        Session.Instance.Connect("localhost");

        Application.LoadLevel("Menu_Connecting");
        this.enabled = false;
    }
}