using Lidgren.Network;
using System.Collections.Generic;

namespace Gamemodes.Cars
{
    public class ServerGamemode : ServerGameMode
    {
        private List<CarState> carSlots = new List<CarState>();

        public ServerGamemode(Server server)
            : base(server)
        {
            for (int i = 0; i < Server.Players.SlotsCount; ++i)
            {
                RemotePlayer player = server.Players.GetPlayerInSlot(i);

                CarState state = new CarState();

                carSlots.Add(state);

                if (player != null)
                {
                    SpawnPlayer(i, 0.0f, 0.0f, 0.0f);
                }
            }
        }

        public override void HandleIncomingMessage(NetIncomingMessage message)
        {
            eCarsClientToServerMessage category = (eCarsClientToServerMessage)message.ReadByte();

            if (category >= eCarsClientToServerMessage.Max)
            {
                Server.DropClient(message.SenderConnection, "Gamemode error, unknown message category received: {0:n}");
                return;
            }

            switch (category)
            {
                case eCarsClientToServerMessage.UpdatePlayerState:
                    HandleUpdatePlayerState(message);
                    break;

                default:
                    Server.Log("Unhandled message category {0}", category);
                    break;
            }
        }

        private void HandleUpdatePlayerState(NetIncomingMessage msg)
        {
            RemotePlayer player = Server.Players.GetPlayer(msg.SenderConnection.RemoteUniqueIdentifier);

            if (player.PlayerSlot < 0)
                return;

            CarState state = carSlots[player.PlayerSlot];

            state.ReadPlayer(msg);
            state.ReceiveTime = msg.ReceiveTime;
        }

        public override void Update(float dt)
        {
            NetOutgoingMessage msg =  CreateMessage(eCarsServerToClientMessage.UpdatePlayerState);

            for (int i = 0; i < carSlots.Count; ++i)
            {
                carSlots[i].Write(msg);
            }

            Server.SendToAll(msg, null, NetDeliveryMethod.UnreliableSequenced, 0);
        }

        private void SpawnPlayer(int slot, float positionX, float positionY, float rotation)
        {
            CarState state = carSlots[slot];
            state.Status = CarState.eStatus.Alive;
            state.PositionX = positionX;
            state.PositionY = positionY;
            state.Rotation = rotation;
            state.LocalVelocityX = 0.0f;
            state.LocalVelocityY = 0.0f;
        }

        private void KillPlayer(int slot)
        {
            carSlots[slot].Status = CarState.eStatus.Dead;
        }

        private NetOutgoingMessage CreateMessage(eCarsServerToClientMessage category)
        {
            NetOutgoingMessage message = CreateMessage();
            message.Write((byte)category);
            return message;
        }
    }
}