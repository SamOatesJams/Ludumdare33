using UnityEngine;
using System.Collections;
using Realms.Common.Packet;
using System;

namespace Realms.Server.Packet
{
    [Serializable]
    public class MobDeathPacket : IPacket
    {
        /// <summary>
        /// 
        /// </summary>
        public int MobID { get; private set; }

        /// <summary>
        /// Class constructor
        /// </summary>
        public MobDeathPacket(int mobId) : base(typeof(MobDeathPacket))
        {
            this.MobID = mobId;
        }
    }
}

