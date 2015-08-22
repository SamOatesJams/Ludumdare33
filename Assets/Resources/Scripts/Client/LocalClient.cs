using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using Realms.Common.Packet;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Realms.Client
{
    public class LocalClient : MonoBehaviour
    {
        #region Public Members
        /// <summary>
        /// The maximum number of connections
        /// </summary>
        public int MaxConnections = 50;

        /// <summary>
        /// The maximum number of connections
        /// </summary>
        public int ServerPort = 5598;

        /// <summary>
        /// The address of the server
        /// </summary>
        public string ServerIP = "5.189.158.88";
        #endregion

        #region Private Members
        /// <summary>
        /// The main connection config
        /// </summary>
        private ConnectionConfig m_configuration = null;

        /// <summary>
        /// The communication channel
        /// </summary>
        private byte m_communicationChannel = 0;

        /// <summary>
        /// The udp transport id
        /// </summary>
        private int m_genericHostId = -1;

        /// <summary>
        /// 
        /// </summary>
        private int m_connectionId = -1;

        /// <summary>
        /// 
        /// </summary>
        private Dictionary<Type, IPacket> m_packetQueue = new Dictionary<Type, IPacket>();
        #endregion

        /// <summary>
        /// 
        /// </summary>
        private void Start()
        {
            m_configuration = new ConnectionConfig();
            m_communicationChannel = m_configuration.AddChannel(QosType.Reliable);

            NetworkTransport.Init();

            var topology = new HostTopology(m_configuration, this.MaxConnections);
            m_genericHostId = NetworkTransport.AddHost(topology, 0);

            byte error;
            m_connectionId = NetworkTransport.Connect(m_genericHostId, this.ServerIP, this.ServerPort, 0, out error);
            if (error != 0 || m_connectionId == 0)
            {
                Debug.LogError(string.Format("Failed to connect to {0}:{1}", this.ServerIP, this.ServerPort));
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        private void FixedUpdate()
        {
            const int bufferSize = 1024;

            int hostId, connectionId, channelId, dataSize;
            byte[] recBuffer = new byte[bufferSize];
            byte error;

            var dataEvent = NetworkTransport.Receive(out hostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);

            switch (dataEvent)
            {
                case NetworkEventType.Nothing:
                    {
                        
                    }
                    break;

                case NetworkEventType.ConnectEvent:
                    {
                        HandleClientConnectionEvent();
                        SendQueuedPackets(hostId, connectionId);
                    }
                    break;

                case NetworkEventType.DataEvent:
                    {
                        Debug.Log(string.Format("Client: DataEvent from host {0} connection {1}", hostId, connectionId));
                    }
                    break;

                case NetworkEventType.DisconnectEvent:
                    {
                        Debug.Log(string.Format("Client: DisconnectEvent from host {0} connection {1}", hostId, connectionId));
                    }
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void HandleClientConnectionEvent()
        {
            var packet = new Realms.Client.Packet.PlayerConnectPacket(string.Format("Player_{0}", m_connectionId));
            QueuePacket(packet);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hostId"></param>
        /// <param name="connectionId"></param>
        private void SendQueuedPackets(int hostId, int connectionId)
        {
            byte error;
            var formatter = new BinaryFormatter();

            foreach (var packet in m_packetQueue.Values)
            {
                Debug.Log(string.Format("Client: Sending Packet {0}", packet.PacketType.ToString()));

                using (var stream = new MemoryStream())
                {
                    formatter.Serialize(stream, packet);
                    var data = stream.ToArray();
                    NetworkTransport.Send(hostId, connectionId, m_communicationChannel, data, data.Length, out error);
                }
            }

            m_packetQueue.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packet"></param>
        public void QueuePacket(IPacket packet)
        {
            m_packetQueue[packet.PacketType] = packet;
        }
    }
}
