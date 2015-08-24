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
        
        private NavMeshAgent navAgent;
        private LocalClient m_localClient;
        private bool m_didPlayerClick = false;

        private bool m_playerWalkingToAttack = false;
        private bool m_playerAttacking = false;
        private float m_attackStartTime = 0.0f;
        private Common.Mob m_mobToAttack = null;

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
            if (m_mobToAttack != null)
            {
                // We are walking to an attack and we have stopped.
                var mobLocation = m_mobToAttack.transform.position;
                var myLocation = this.transform.position;

                if (m_playerWalkingToAttack)
                {
                    var distance = (mobLocation - myLocation).magnitude;
                    if (distance <= 2.0f)
                    {
                        m_playerWalkingToAttack = false;
                        m_playerAttacking = true;
                        m_attackStartTime = Time.time + 2.0f;
                    }
                    else
                    {
                        var dir = (mobLocation - myLocation).normalized;
                        var target = mobLocation - dir;
                        navAgent.SetDestination(target);
                        SendMovementPacket(target);
                    }
                }
                else if (m_playerAttacking)
                {
                    var dir = (mobLocation - myLocation).normalized;
                    var lookAt = new Vector3(dir.x, 0.0f, dir.z);
                    var rotation = Quaternion.LookRotation(lookAt);
                    this.transform.rotation = rotation;

                    var distance = (mobLocation - myLocation).magnitude;
                    if (distance <= 2.0f)
                    {
                        if (Time.time - m_attackStartTime >= 2.0f)
                        {
                            var attackPacket = new Client.Packet.PlayerAttackPacket(m_mobToAttack.ID, UnityEngine.Random.Range(0, 3));
                            m_localClient.QueuePacket(attackPacket);
                            m_attackStartTime = Time.time;
                        }
                    }
                    else
                    {
                        var target = mobLocation - dir;
                        navAgent.SetDestination(target);
                        SendMovementPacket(target);
                    }
                }
            }

            if (m_didPlayerClick)
            {
                m_didPlayerClick = false;

                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                Physics.Raycast(ray, out hit);

                if (hit.collider != null)
                {
                    var mob = hit.collider.GetComponent<Common.Mob>();
                    if (mob != null)
                    {
                        // Found a mob, walk to it and attack
                        var mobLocation = mob.transform.position;
                        var myLocation = this.transform.position;
                        var dir = (mobLocation - myLocation).normalized;

                        var target = mobLocation - dir;
                        navAgent.SetDestination(target);
                        SendMovementPacket(target);
                        m_playerWalkingToAttack = true;
                        m_mobToAttack = mob;
                    }
                    else
                    {
                        navAgent.SetDestination(hit.point);
                        SendMovementPacket(hit.point);
                        m_playerWalkingToAttack = false;
                        m_playerAttacking = false;
                        m_mobToAttack = null;
                    }
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
