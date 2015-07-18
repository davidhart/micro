
public enum ServerToClientMessageCategory
{
    Chat = 0,
    GameMode = 1,
    SessionInit = 3,
    PlayerJoined = 4,
    PlayerLeft = 5,
    PlayerSetSlot = 6,
    PlayerSetStatus = 7,
    MAX
}

public enum ClientToServerMessageCategory
{
    Chat = 0,
    GameMode = 1,
    JoinSlot = 2,
    SetStatus = 3,
    MAX
}