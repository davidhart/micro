using UnityEngine;
using UnityEngine.UI;

class Menu_Connect : MonoBehaviour
{
    public InputField AddressField = null;
    
    public void OnConnectPressed()
    {
        Session.Instance.Connect(AddressField.text);

        Application.LoadLevel("Menu_Connecting");

        this.enabled = false;
    }

    public void OnCreatePressed()
    {
        Session.Instance.Create();

        Application.LoadLevel("Menu_Connecting");

        this.enabled = false;
    }
}