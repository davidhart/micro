using Lidgren.Network;
using System.Collections.Generic;

namespace Gamemodes.Cars
{
    class CarsClientGameMode : ClientGameMode
    {
        private SessionVehicles vehicles;

        private VehicleNetworkState localPlayerState;
        private List<VehicleNetworkState> vehcileNetworkStates = new List<VehicleNetworkState>();

        public CarsClientGameMode(SessionVehicles vehicles)
        {
            this.vehicles = vehicles;
        }

        public override void OnAttachedToClient(Client client)
        {
            base.OnAttachedToClient(client);

            vehicles.SetupVehicleSlots(client.Players.SlotsCount);

            for(int i = 0; i < client.Players.SlotsCount; ++i)
            {
                RemotePlayer player = client.Players.GetPlayerInSlot(i);

                VehicleNetworkState state = new VehicleNetworkState();
                vehcileNetworkStates.Add(state);

                if (player == null)
                    continue;

                if (player == client.LocalPlayer)
                {
                    vehicles.SpawnLocalVehicle(i);

                    localPlayerState = state;
                }
                else
                {
                    vehicles.SpawnRemotevehicle(i);
                }
            }
        }

        public override void Update()
        {
            int localSlot = Client.LocalPlayer.PlayerSlot;

            if (localSlot >= 0)
            {
                vehicles.GetVehicleState(localSlot).ToNetworkState(localPlayerState);

                NetOutgoingMessage msg = CreateMessage(eCarsClientToServerMessage.UpdatePlayerState);
                localPlayerState.Write(msg);

                Client.Send(msg, NetDeliveryMethod.UnreliableSequenced);
            }
        }

        public override void HandleIncomingMessage(NetIncomingMessage msg)
        {
            eCarsServerToClientMessage type = (eCarsServerToClientMessage)msg.ReadByte();

            if (type >= eCarsServerToClientMessage.Max)
            {
                Client.Log(string.Format("Invalid message type {0}", (byte)type));
                return;
            }

            switch(type)
            {
                case eCarsServerToClientMessage.UpdatePlayerState:
                    HandleStateUpdate(msg);
                    break;

                default:
                    Client.Log(string.Format("Unhandled message type {0}", type));
                    break;
            }
        }

        private void HandleStateUpdate(NetIncomingMessage msg)
        {
            for (int i = 0; i < vehcileNetworkStates.Count; ++i)
            {
                vehcileNetworkStates[i].Read(msg);

                VehicleState state = new VehicleState();
                state.FromNetworkState(vehcileNetworkStates[i]);

                vehicles.SetVehicleState(i, state);
            }
        }

        public NetOutgoingMessage CreateMessage(eCarsClientToServerMessage category)
        {
            NetOutgoingMessage msg = CreateMessage();
            msg.Write((byte)category);
            return msg;
        }
    }
}
