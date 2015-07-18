
namespace Gamemodes.Cars
{
    public enum eCarsClientToServerMessage
    {
        UpdatePlayerState,

        Max
    }

    public enum eCarsServerToClientMessage
    {
        SpawnPlayer,
        UpdatePlayerState,
        KillPlayer,

        Max
    }
}