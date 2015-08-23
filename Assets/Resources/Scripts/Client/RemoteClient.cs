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

        /// <summary>
        /// 
        /// </summary>
        public UnityEngine.UI.Text ChatText = null;
        #endregion

        #region Private Members
        /// <summary>
        /// 
        /// </summary>
        private NavMeshAgent m_navAgent = null;

        /// <summary>
        /// 
        /// </summary>
        private float m_chatAddTime = 0.0f;
        #endregion

        /// <summary>
        /// 
        /// </summary>
        private void Start()
        {
            m_navAgent = GetComponent<NavMeshAgent>();
        }

        /// <summary>
        /// 
        /// </summary>
        private void FixedUpdate()
        {
            if (m_chatAddTime > 0.0f && Time.time - m_chatAddTime >= 6.0f)
            {
                ChatText.text = string.Empty;
                m_chatAddTime = 0.0f;
            }
        }

        /// <summary>
        /// Called when a player move packet is received
        /// </summary>
        /// <param name="packet"></param>
        public void HandleMovePacket(PlayerMovePacket packet)
        {
            m_navAgent.SetDestination(packet.GetPosition());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chatMessage"></param>
        public void SetChatLine(string chatMessage)
        {
            ChatText.text = string.Format("\n{0}", chatMessage);
            m_chatAddTime = Time.time;
        }
    }
}
