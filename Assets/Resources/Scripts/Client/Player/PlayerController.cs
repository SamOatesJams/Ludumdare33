using UnityEngine;
using InControl;
using System.Collections;

namespace Realms.Client.Player
{
    [RequireComponent(typeof(LocalClient))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class PlayerController : MonoBehaviour
    {
        private Vector3 lastLocation;
        private NavMeshAgent navAgent;
        private PlayerControls playerControls;
        private LocalClient m_localClient;
        private bool m_didPlayerClick = false;

        // Use this for initialization
        void Start()
        {
            m_localClient = GetComponent<LocalClient>();

            navAgent = GetComponent<NavMeshAgent>();

            playerControls = new PlayerControls();
            playerControls.SetupControls();
        }

        void Update()
        {
            if (!m_didPlayerClick)
            {
                m_didPlayerClick = playerControls.MouseLeft.WasPressed;
            }
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (m_didPlayerClick)
            {
                m_didPlayerClick = false;

                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                Physics.Raycast(ray, out hit);

                if (hit.collider)
                {
                    navAgent.destination = hit.point;
                    SendMovementPacket(hit.point);
                }
            }
        }

        /// <summary>
        /// Send a movement packet to the server
        /// </summary>
        private void SendMovementPacket(Vector3 position)
        {
            var packet = new Realms.Client.Packet.PlayerMovePacket(position);
            m_localClient.QueuePacket(packet);
        }
    }
}
