using Lidgren.Network;

namespace Gamemodes.Cars
{

    public class VehicleNetworkState
    {
        public enum eStatus
        {
            Dead,
            Alive,
        }

        public eStatus Status = eStatus.Dead;

        public float PositionX;
        public float PositionY;
        public float Rotation;

        public float LocalVelocityX;
        public float LocalVelocityY;

        public float InputTurn;
        public float InputForward;
        public float InputBackward;

        public void Write(NetOutgoingMessage msg)
        {
            msg.Write((byte)Status);
            msg.Write(PositionX);
            msg.Write(PositionY);
            msg.Write(Rotation);
            msg.Write(LocalVelocityX);
            msg.Write(LocalVelocityY);
            msg.Write(InputTurn);
            msg.Write(InputForward);
            msg.Write(InputBackward);
        }

        public void Read(NetIncomingMessage msg)
        {
            Status = (eStatus)msg.ReadByte();
            PositionX = msg.ReadFloat();
            PositionY = msg.ReadFloat();
            Rotation = msg.ReadFloat();
            LocalVelocityX = msg.ReadFloat();
            LocalVelocityY = msg.ReadFloat();
            InputTurn = msg.ReadFloat();
            InputForward = msg.ReadFloat();
            InputBackward = msg.ReadFloat();
        }
    }
}