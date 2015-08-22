using UnityEngine;
using System.Collections;

namespace Realms.Client.Player
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class PlayerAnimationController : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        private NavMeshAgent m_navAgent = null;

        /// <summary>
        /// 
        /// </summary>
        private Animator m_animator = null;

        void Start()
        {
            m_navAgent = this.GetComponent<NavMeshAgent>();
            m_animator = this.GetComponentInChildren<Animator>();
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
    }
}
