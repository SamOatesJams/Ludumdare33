using UnityEngine;
using System.Collections;
using Realms.Server.Packet;
using System;

namespace Realms.Client
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class RemoteClient : MonoBehaviour
    {
        #region Public Members
        /// <summary>
        /// 
        /// </summary>
        private string m_username = string.Empty;
        public string Username
        {
            get { return m_username; }
            set
            {
                m_username = value;
                UsernameText.text = m_username;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public UnityEngine.UI.Text UsernameText = null;
        #endregion

        #region Private Members
        /// <summary>
        /// 
        /// </summary>
        private NavMeshAgent m_navAgent = null;
        #endregion

        /// <summary>
        /// 
        /// </summary>
        private void Start()
        {
            m_navAgent = GetComponent<NavMeshAgent>();
        }

        /// <summary>
        /// Called when a player move packet is received
        /// </summary>
        /// <param name="packet"></param>
        public void HandleMovePacket(PlayerMovePacket packet)
        {
            m_navAgent.SetDestination(packet.GetPosition());
        }
    }
}
