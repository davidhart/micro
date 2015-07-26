using Lidgren.Network;

namespace Gamemodes
{
    public abstract class ClientGameMode
    {
        public Client Client { get; private set; }

        public virtual void OnAttachedToClient(Client client)
        {
            Client = client;
        }

        public abstract void HandleIncomingMessage(NetIncomingMessage msg);

        public NetOutgoingMessage CreateMessage()
        {
            NetOutgoingMessage message = Client.CreateMessage(eClientToServerMessage.GameModeData);

            return message;
        }
    }
}
