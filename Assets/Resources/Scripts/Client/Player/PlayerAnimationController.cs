using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Realms.Client.Player
{
    public enum PlayerEmote
    {
        Wave
    }

    [RequireComponent(typeof(NavMeshAgent))]
    public class PlayerAnimationController : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        public bool IsAttacking { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private NavMeshAgent m_navAgent = null;

        /// <summary>
        /// 
        /// </summary>
        private Animator m_animator = null;
        
        /// <summary>
        /// 
        /// </summary>
        private Dictionary<PlayerEmote, string> m_emoteToAnimationParameter = new Dictionary<PlayerEmote, string>();

        void Start()
        {
            m_navAgent = this.GetComponent<NavMeshAgent>();
            m_animator = this.GetComponentInChildren<Animator>();

            m_emoteToAnimationParameter[PlayerEmote.Wave] = "IsWaving";
        }

        /// <summary>
        /// 
        /// </summary>
        void Update()
        {
            bool isWalking = false;

            if (m_navAgent.velocity.sqrMagnitude > float.Epsilon)
            {
                isWalking = true;
            }

            m_animator.SetBool("IsWalking", isWalking);
        }

        void LateUpdate()
        {
            // Turn off all emotes
            foreach (var emote in Enum.GetValues(typeof(Player.PlayerEmote)).Cast<Player.PlayerEmote>())
            {
                m_animator.SetBool(m_emoteToAnimationParameter[emote], false);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="playerEmote"></param>
        public void PerformEmote(PlayerEmote playerEmote)
        {
            m_animator.SetBool(m_emoteToAnimationParameter[playerEmote], true);
        }
    }
}
