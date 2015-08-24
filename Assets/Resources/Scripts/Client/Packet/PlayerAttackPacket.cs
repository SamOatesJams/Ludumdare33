using UnityEngine;
using System.Collections;
using Realms.Common.Packet;
using System;

namespace Realms.Client.Packet
{
    [Serializable]
    public class PlayerAttackPacket : IPacket
    {
        /// <summary>
        /// The mob who was attacked
        /// </summary>
        public int MobID { get; private set; }

        /// <summary>
        /// The amount of damage caused
        /// </summary>
        public int Damage { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mobId"></param>
        /// <param name="damage"></param>
        public PlayerAttackPacket(int mobId, int damage) : base(typeof(PlayerAttackPacket))
        {
            this.MobID = mobId;
            this.Damage = damage;
        }
    }
}

