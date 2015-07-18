using UnityEngine;
using UnityEngine.UI;

class LobbySlot : MonoBehaviour
{
    public Text NameLabel = null;
    public GameObject ReadyParent = null;
    public GameObject ReadyToggle = null;
    private int slot = -1;

    public void SetupEmpty(int slot)
    {
        this.slot = slot;
        NameLabel.text = "-OPEN-";
        NameLabel.color = PlayerIdentityGenerator.unassignedColor;
        ReadyParent.SetActive(false);
    }

    public void Setup(int slot, string playername, bool ready)
    {
        this.slot = slot;
        NameLabel.text = playername;
        NameLabel.color = PlayerIdentityGenerator.PlayerSlotToColor(slot);
        ReadyParent.SetActive(true);
        ReadyToggle.SetActive(ready);
    }

    public void OnPressed()
    {
        Session.Instance.JoinSlot(slot);
    }

    public void OnToggleReady()
    {
        RemotePlayer localPlayer = Session.Instance.LocalPlayer;
        if( localPlayer.PlayerSlot == slot )
        {
            if (localPlayer.Status == RemotePlayerStatus.LobbyReady)
            {
                Session.Instance.SetStatus(RemotePlayerStatus.LobbyNotReady);
            }
            else
            {
                Session.Instance.SetStatus(RemotePlayerStatus.LobbyReady);
            }
        }
    }
}