using UnityEngine;
using UnityEngine.UI;

class LobbySlot : MonoBehaviour
{
    public Text NameLabel = null;
    private int slot = -1;

    public void Setup(string playername, int slot)
    {
        this.slot = slot;
        if (string.IsNullOrEmpty(playername))
        {
            NameLabel.text = "-OPEN-";
            NameLabel.color = PlayerIdentityGenerator.unassignedColor;
        }
        else
        {
            NameLabel.text = playername;
            NameLabel.color = PlayerIdentityGenerator.PlayerSlotToColor(slot);
        }
    }

    public void OnPressed()
    {
        Session.Instance.JoinSlot(slot);
    }
}