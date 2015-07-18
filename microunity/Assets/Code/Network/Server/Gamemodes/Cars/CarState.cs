using Lidgren.Network;

namespace Gamemodes.Cars
{

    public class CarState
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

        public double ReceiveTime = 0.0f;

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
            msg.Write(ReceiveTime);
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
            ReceiveTime = msg.ReadDouble();
        }

        public void WritePlayer(NetIncomingMessage msg)
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

        public void ReadPlayer(NetIncomingMessage msg)
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
            ReceiveTime = msg.ReceiveTime;
        }
    }
}