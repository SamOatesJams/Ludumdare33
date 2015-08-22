using UnityEngine;
using InControl;
using System.Collections;
using System.Linq;

namespace Realms.Client.Player
{
    [RequireComponent(typeof(LocalClient))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class PlayerController : MonoBehaviour
    {
        public PlayerControls PlayerControls
        {
            get; set;
        }

        private Vector3 lastLocation;
        private NavMeshAgent navAgent;
        private LocalClient m_localClient;
        private bool m_didPlayerClick = false;

        // Use this for initialization
        void Start()
        {
            m_localClient = GetComponent<LocalClient>();

            navAgent = GetComponent<NavMeshAgent>();

            PlayerControls = new PlayerControls();
            PlayerControls.SetupControls();
        }

        void Update()
        {
            if (!m_didPlayerClick && !IsMouseOverUI())
            {
                m_didPlayerClick = PlayerControls.MouseLeft.WasPressed;
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
        /// 
        /// </summary>
        /// <returns></returns>
        private bool IsMouseOverUI()
        {
            var rects = GameObject.FindGameObjectsWithTag("UIPanel").Select(x => x.GetComponent<RectTransform>());

            var mousePosition = Input.mousePosition;
            var worldCorners = new Vector3[4];

            foreach (var rect in rects)
            {
                rect.GetWorldCorners(worldCorners);

                if (mousePosition.x >= worldCorners[0].x && mousePosition.x < worldCorners[2].x
                   && mousePosition.y >= worldCorners[0].y && mousePosition.y < worldCorners[2].y)
                {
                    return true;
                }
            }

            return false;
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
