using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

class Menu_Lobby : MonoBehaviour
{
    public ScrollRect ChatScrollRect = null;
    public Text ChatText = null;
    public InputField ChatInputField = null;

    public GameObject PlayerSlotPrototype = null;

    private List<LobbySlot> Slots = new List<LobbySlot>();
    
    public void Start()
    {
        ChatInputField.onEndEdit.AddListener(OnChatSubmit);

        Session.Instance.RegisterChatDelegate(OnChatReceived);
        Session.Instance.Players.OnNumSlotsChanged += SetupPlayerSlots;
        Session.Instance.Players.OnPlayerAdded += PlayerRefresh;
        Session.Instance.Players.OnPlayerRemoved += PlayerRefresh;
        Session.Instance.Players.OnPlayerSetSlot += PlayerRefresh;
        Session.Instance.Players.OnPlayerStatusChanged += PlayerRefresh;

        PlayerSlotPrototype.SetActive(false);
        SetupPlayerSlots(Session.Instance.Players.SlotsCount);
    }

    public void OnDestroy()
    {
        Session.Instance.DeregisterChatdelegate(OnChatReceived);

        Session.Instance.Players.OnNumSlotsChanged -= SetupPlayerSlots;
        Session.Instance.Players.OnPlayerAdded -= PlayerRefresh;
        Session.Instance.Players.OnPlayerRemoved -= PlayerRefresh;
        Session.Instance.Players.OnPlayerSetSlot -= PlayerRefresh;
    }

    public void PlayerRefresh(RemotePlayer player)
    {
        RefreshPlayers();
    }

    private void SetupPlayerSlots(int slots)
    {
        if (Slots.Count < slots)
        {
            for (int i = Slots.Count; i < slots; i++)
            {
                GameObject slotGO = GameObject.Instantiate(PlayerSlotPrototype) as GameObject;
                Slots.Add(slotGO.GetComponent<LobbySlot>());
                slotGO.transform.SetParent(PlayerSlotPrototype.transform.parent, false);
                slotGO.SetActive(true);
            }
        }
        else
        {
            for (int i = Slots.Count - 1; i >= slots; i--)
            {
                GameObject.Destroy(Slots[i].gameObject);
                Slots.RemoveAt(i);
            }
        }

        RefreshPlayers();
    }

    private void RefreshPlayers()
    {
        for (int i = 0; i < Session.Instance.Players.SlotsCount; ++i)
        {
            RemotePlayer player = Session.Instance.Players.GetPlayerInSlot(i);
            if (player == null)
            {
                Slots[i].SetupEmpty(i);
            }
            else
            {
                Slots[i].Setup(i, player.PlayerName, player.Status == RemotePlayerStatus.LobbyReady);
            }
        }
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
