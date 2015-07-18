using Lidgren.Network;
using System.Collections.Generic;

public enum RemotePlayerStatus
{
    LobbyNotReady = 0,
    LobbyReady = 1,
    MAX
}

public class RemotePlayer
{
    public string PlayerName { get; set; }
    public int PlayerSlot { get; set; }
    public readonly long UniqueID;
    public RemotePlayerStatus Status { get; set; }

    public RemotePlayer(long uniqueID, string name)
    {
        this.UniqueID = uniqueID;
        this.PlayerName = name;
        PlayerSlot = -1;
        Status = RemotePlayerStatus.LobbyNotReady;
    }
}

public class RemotePlayerSet
{
    public delegate void RemotePlayerDelegate(RemotePlayer player);
    public delegate void NumSlotsChangedDelegate(int slots);

    public RemotePlayerDelegate OnPlayerSetSlot = (p) => { };
    public RemotePlayerDelegate OnPlayerAdded = (p) => { };
    public RemotePlayerDelegate OnPlayerRemoved = (p) => { };
    public NumSlotsChangedDelegate OnNumSlotsChanged = (p) => { };
    public RemotePlayerDelegate OnPlayerStatusChanged = (p) => { };

    Dictionary<long, RemotePlayer> uniqueIDtoPlayer = new Dictionary<long, RemotePlayer>();
    List<RemotePlayer> slots = new List<RemotePlayer>();

    public int ConnectedCount
    {
        get
        {
            return uniqueIDtoPlayer.Count;
        }
    }

    public int SlotsCount
    {
        get
        {
            return slots.Count;
        }
    }

    public IEnumerable<RemotePlayer> ConnectedPlayers
    {
        get
        {
            return uniqueIDtoPlayer.Values;
        }
    }

    public RemotePlayer GetPlayerInSlot(int slot)
    {
        return slots[slot];
    }

    // Set the max number of available slots for players, additional players may connect as spectators, and will have a slot id of -1
    public void SetNumSlots(int num)
    {
        // Not enough slots, open more
        if (num > slots.Count)
        {
            for (int i = slots.Count; i < num; ++i)
            {
                slots.Add(null);
            }
        }
        // Too many open slots, eject players from the end of the list first
        else if (num < slots.Count)
        {
            for (int i = slots.Count - 1; i > num; --i)
            {
                if (slots[i] != null)
                {
                    MovePlayerToSlot(slots[i], -1);
                }

                slots.RemoveAt(i);
            }
        }

        OnNumSlotsChanged(num);
    }

    public void MovePlayerToSlot(RemotePlayer player, int targetSlot)
    {
        // Target slot is already occupied
        if (targetSlot >= 0 && slots[targetSlot] != null)
        {
            return;
        }

        // Already in a slot
        if (player.PlayerSlot >= 0)
        {
            slots[player.PlayerSlot] = null;
        }

        player.PlayerSlot = targetSlot;

        // Occupy new slot
        if (player.PlayerSlot >= 0)
        {
            slots[player.PlayerSlot] = player;
        }

        OnPlayerSetSlot(player);
    }

    public void AddPlayer(RemotePlayer player, bool occupyEmptySlot)
    {
        uniqueIDtoPlayer.Add(player.UniqueID, player);

        OnPlayerAdded(player);

        if (occupyEmptySlot)
        {
            for (int i = 0; i < slots.Count; ++i)
            {
                if (slots[i] == null)
                {
                    MovePlayerToSlot(player, i);
                    break;
                }
            }
        }
    }

    public void RemovePlayer(long uniqueID)
    {
        RemotePlayer player;
        if (uniqueIDtoPlayer.TryGetValue(uniqueID, out player))
        {
            RemovePlayer(player);
        }
    }

    public void RemovePlayer(RemotePlayer player)
    {
        MovePlayerToSlot(player, -1);

        uniqueIDtoPlayer.Remove(player.UniqueID);

        OnPlayerRemoved(player);
    }

    public RemotePlayer GetPlayer(long uniqueID)
    {
        RemotePlayer player;
        uniqueIDtoPlayer.TryGetValue(uniqueID, out player);
        return player;
    }

    public void SetStatus(RemotePlayer player, RemotePlayerStatus status)
    {
        player.Status = status;

        OnPlayerStatusChanged(player);
    }

    public void SetAllStatus(RemotePlayerStatus status)
    {
        foreach(RemotePlayer player in ConnectedPlayers)
        {
            player.Status = status;

            OnPlayerStatusChanged(player);
        }
    }
}
