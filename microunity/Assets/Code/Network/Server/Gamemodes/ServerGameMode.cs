using Lidgren.Network;

namespace Gamemodes
{
    public abstract class ServerGameMode
    {
        public Server Server { get; private set; }
        public virtual bool AllowJoinInProgress { get { return false; } }

        public ServerGameMode(Server server)
        {
            this.Server = server;
        }

        public abstract void HandleIncomingMessage(NetIncomingMessage message);

        public NetOutgoingMessage CreateMessage()
        {
            NetOutgoingMessage message = Server.CreateMessage(eServerToClientMessage.GameModeData);

            return message;
        }

        public virtual void Update(float dt) { }

    }
}