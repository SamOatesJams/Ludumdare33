using UnityEngine;
using System.Collections;
using Realms.Common.Packet;
using System.Collections.Generic;

namespace Realms.Server
{
    public enum ServerPacketType
    {
        ToAll,
        ToAllExcluding,
        ToConnection
    }

    public class ServerPacket
    {
        /// <summary>
        /// 
        /// </summary>
        public ServerPacketType SendType {get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public IPacket Packet { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public int ConnectionId { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<int> Excluding { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="packet"></param>
        public ServerPacket(ServerPacketType type, IPacket packet)
        {
            this.SendType = type;
            this.Packet = packet;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="packet"></param>
        /// <param name="connectionId"></param>
        public ServerPacket(ServerPacketType type, IPacket packet, int connectionId) : this(type, packet)
        {
            this.ConnectionId = connectionId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="packet"></param>
        /// <param name="connectionId"></param>
        public ServerPacket(ServerPacketType type, IPacket packet, IEnumerable<int> excluding) : this(type, packet)
        {
            this.Excluding = excluding;
        }
    }
}

