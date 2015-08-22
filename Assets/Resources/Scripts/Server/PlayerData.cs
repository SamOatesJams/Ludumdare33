using UnityEngine;
using System.Collections;

namespace Realms.Server
{
    public class PlayerData
    {
        /// <summary>
        /// The players connection ID
        /// </summary>
        public int ConnectionId { get; private set; }

        /// <summary>
        /// The players username
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The players current position
        /// </summary>
        public Vector3 CurrentPosition { get; set; }

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="username"></param>
        public PlayerData(int connectionId, string username, Vector3 position)
        {
            this.ConnectionId = connectionId;
            this.Username = username;
            this.CurrentPosition = position;
        }
    }
}

