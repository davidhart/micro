﻿using UnityEngine;
using UnityEngine.UI;

class Menu_Lobby : MonoBehaviour
{
    public ScrollRect ChatScrollRect = null;
    public Text ChatText = null;
    public InputField ChatInputField = null;
    
    public void Start()
    {
        Session.Instance.RegisterChatDelegate(OnChatReceived);

        ChatInputField.onEndEdit.AddListener(OnChatSubmit);
    }

    public void OnDestroy()
    {
        Session.Instance.DeregisterChatdelegate(OnChatReceived);
    }

    private void OnChatReceived(string chat)
    {
        ChatText.text += "\n";
        ChatText.text += chat;
    }

    public void SendChat()
    {
        Session.Instance.SendChat(ChatInputField.text);
        ChatInputField.text = string.Empty;
    }

    public void Update()
    {
        if (Session.Instance.Connected == false)
        {
            Application.LoadLevel("Menu_Connecting");

            this.enabled = false;
        }
    }

    private void OnChatSubmit(string chat)
    {
        if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
        {
            SendChat();
            ChatInputField.Select();
            ChatInputField.ActivateInputField();
        }
    }

    public void Disconnect()
    {
        Session.Instance.Disconnect();

        Application.LoadLevel("Menu_Connect");

        this.enabled = false;
    }
}
