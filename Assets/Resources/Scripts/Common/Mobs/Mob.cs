using UnityEngine;
using System.Collections;
using Realms.Server.Packet;
using System;

namespace Realms.Common
{
    public enum MobState
    {
        Idle,
        Walk,
        Attack
    }

    public class Mob : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        public bool IsServer { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string MobType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private int m_health = -1;
        public int Health
        {
            get { return m_health; }
            set
            {
                if (m_health == -1)
                {
                    HealthSlider.maxValue = value;
                }

                m_health = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public UnityEngine.UI.Slider HealthSlider = null;

        /// <summary>
        /// 
        /// </summary>
        public BoxCollider SpawnArea { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private Animator m_animator = null;

        /// <summary>
        /// 
        /// </summary>
        private NavMeshAgent m_navAgent = null;

        /// <summary>
        /// 
        /// </summary>
        private MobState m_state = MobState.Idle;

        /// <summary>
        /// 
        /// </summary>
        private float m_lastAttackTime = 0.0f;

        /// <summary>
        /// 
        /// </summary>
        private Realms.Server.ServerMain m_server = null;

        public Mob()
        {
            this.ID = -1;
        }

        /// <summary>
        /// 
        /// </summary>
        private void Start()
        {
            var server = GameObject.FindGameObjectWithTag("Server");
            if (server != null)
            {
                this.IsServer = true;
                this.ID = this.gameObject.GetInstanceID();

                m_server = server.GetComponent<Realms.Server.ServerMain>();
                m_server.RegisterMob(this);
            }
            
            m_animator = this.GetComponent<Animator>();
            m_navAgent = this.GetComponent<NavMeshAgent>();
        }

        /// <summary>
        /// 
        /// </summary>
        private void FixedUpdate()
        {
            if (m_navAgent.velocity.sqrMagnitude > float.Epsilon && m_state == MobState.Idle)
            {
                m_state = MobState.Walk;
            }

            if (this.IsServer)
            {
                switch (m_state)
                {
                    case MobState.Idle:
                        {
                            if (UnityEngine.Random.Range(0, 100) == 0)
                            {
                                m_state = MobState.Walk;
                                var target = GetRandomPointInSpawnArea();
                                m_navAgent.SetDestination(target);

                                var mobMovePacket = new Realms.Server.Packet.MobMovePacket(this.ID, target);
                                m_server.QueuePacketAll(mobMovePacket);
                            }
                        }
                        break;

                    case MobState.Walk:
                        {
                            if (m_navAgent.velocity.sqrMagnitude <= float.Epsilon)
                            {
                                m_state = MobState.Idle;
                            }
                        }
                        break;
                    case MobState.Attack:
                        {
                            if (Time.time - m_lastAttackTime >= 10.0f)
                            {
                                m_state = MobState.Idle;
                            }
                        }
                        break;
                }
            }
            else
            {
                if (m_state == MobState.Attack)
                {
                    // Do Attack animation
                }
                else if (m_navAgent.velocity.sqrMagnitude > float.Epsilon)
                {
                    m_animator.SetBool("IsWalking", true);
                }
                else
                {
                    m_animator.SetBool("IsWalking", false);
                }
            }     
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Vector3 GetRandomPointInSpawnArea()
        {
            var point = new Vector3(
                UnityEngine.Random.Range(0, this.SpawnArea.bounds.size.x),
                0.0f,
                UnityEngine.Random.Range(0, this.SpawnArea.bounds.size.z)
            );

            point = point - this.SpawnArea.bounds.extents;
            return point + this.SpawnArea.transform.position;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packet"></param>
        public void HandleMovePacket(Server.Packet.MobMovePacket packet)
        {
            m_navAgent.SetDestination(packet.GetPosition());
            m_animator.SetBool("IsWalking", true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packet"></param>
        public void HandleDamagePacket(MobDamagedPacket packet)
        {
            m_state = MobState.Attack;
            m_lastAttackTime = Time.time;

            this.HealthSlider.gameObject.SetActive(true);
            this.HealthSlider.value = packet.NewHealth;

            this.Health = packet.NewHealth;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packet"></param>
        public void HandleDeathPacket(MobDeathPacket packet)
        {
            // Death animation
            GameObject.Destroy(this.gameObject);
        }
    }
}