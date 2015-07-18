
public enum eServerToClientMessage
{
    PlayerJoined,
    PlayerLeft,
    PlayerSetSlot,
    PlayerSetStatus,

    Chat,

    GameModeLaunched,
    GameModeData,

    SessionInit,
    MAX
}

public enum eClientToServerMessage
{
    JoinSlot,
    SetStatus,

    Chat,

    StartGame,
    GameModeData,

    MAX
}