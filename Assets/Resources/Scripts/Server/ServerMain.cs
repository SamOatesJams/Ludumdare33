using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Realms.Common.Packet;
using System.Collections.Generic;
using Realms.Client.Packet;

namespace Realms.Server
{
    public class ServerMain : MonoBehaviour
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
        /// The websocket transport id
        /// </summary>
        private int m_webSocketHostId = -1;

        /// <summary>
        /// The udp transport id
        /// </summary>
        private int m_genericHostId = -1;

        /// <summary>
        /// All packet handlers
        /// </summary>
        Dictionary<Type, System.Action<IPacket, int, int>> m_packetHandlers = new Dictionary<Type, Action<IPacket, int, int>>();
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
            m_webSocketHostId = NetworkTransport.AddWebsocketHost(topology, this.ServerPort, null);
            m_genericHostId = NetworkTransport.AddHost(topology, this.ServerPort, null);

            Debug.Log(string.Format("Server started.\nPort: {0}. Max Connections: {1}. WebSocketID: {2}.GenericHostID: {3}", 
                this.ServerPort,
                this.MaxConnections,
                m_webSocketHostId,
                m_genericHostId)
            );

            m_packetHandlers[typeof(PlayerConnectPacket)] = OnPlayerConnectPacket;
        }

        /// <summary>
        /// Handle new connections
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
                case NetworkEventType.ConnectEvent:
                    {
                        Debug.Log(string.Format("Server: ConnectEvent from host {0} connection {1}", hostId, connectionId));
                    }
                    break;

                case NetworkEventType.DataEvent:
                    {
                        Debug.Log(string.Format("Server: DataEvent from host {0} connection {1}", hostId, connectionId));
                        HandlePacketRecieved(hostId, connectionId, recBuffer, dataSize);
                    }
                    break;

                case NetworkEventType.DisconnectEvent:
                    {
                        Debug.Log(string.Format("Server: DisconnectEvent from host {0} connection {1}", hostId, connectionId));
                    }
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hostId"></param>
        /// <param name="connectionId"></param>
        /// <param name="data"></param>
        /// <param name="dataSize"></param>
        private void HandlePacketRecieved(int hostId, int connectionId, byte[] data, int dataSize)
        {
            var formatter = new BinaryFormatter();

            using (var stream = new MemoryStream(data))
            {
                IPacket packet = formatter.Deserialize(stream) as IPacket;
                if (packet != null)
                {
                    if (m_packetHandlers.ContainsKey(packet.PacketType))
                    {
                        m_packetHandlers[packet.PacketType](packet, hostId, connectionId);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rawPacket"></param>
        /// <param name="hostId"></param>
        /// <param name="connectionId"></param>
        private void OnPlayerConnectPacket(IPacket rawPacket, int hostId, int connectionId)
        {
            var packet = rawPacket as PlayerConnectPacket;
            if (packet == null)
            {
                return;
            }

            Debug.Log(string.Format("Server: Revieved player connection from {0}", packet.Username));
        }
    }
}

