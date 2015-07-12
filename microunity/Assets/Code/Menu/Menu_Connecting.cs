using UnityEngine;
using UnityEngine.UI;

class Menu_Connecting : MonoBehaviour
{
    public GameObject ConnectingParent = null;
    public GameObject DisconnectedParent = null;
    public Text DisconnectReasonText = null;

    private bool waitingToConnect;

    public void Start()
    {
        waitingToConnect = true;
    }

    public void Update()
    {
        if (waitingToConnect)
        {
            // Still waiting
            if (Session.Instance.Connecting == false)
            {
                if (Session.Instance.Connected)
                {
                    Application.LoadLevel("Menu_Lobby");

                    this.enabled = false;
                }
                else
                {
                    waitingToConnect = false;
                }
            }
            
        }

        if (waitingToConnect)
        {
            ConnectingParent.gameObject.SetActive(true);
            DisconnectedParent.gameObject.SetActive(false);
        }
        else
        {
            ConnectingParent.gameObject.SetActive(false);
            DisconnectedParent.gameObject.SetActive(true);
            DisconnectReasonText.text = Session.Instance.LastClientStatusMessage;
        }
    }

    public void OnOkButtonPressed()
    {
        Application.LoadLevel("Menu_Connect");

        this.enabled = false;
    }
}
