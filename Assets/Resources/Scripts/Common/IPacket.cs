using System;

namespace Realms.Common.Packet
{
    [Serializable]
    public abstract class IPacket
    {
        /// <summary>
        /// The type this packet is
        /// </summary>
        public Type PacketType { get; set; }

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="packetType">The type of packet this packet actually is</param>
        public IPacket(Type packetType)
        {
            this.PacketType = packetType;
        }
    }
}

