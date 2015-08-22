using Realms.Common.Packet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;

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
        private Dictionary<Type, System.Action<IPacket, int, int>> m_packetHandlers = new Dictionary<Type, Action<IPacket, int, int>>();

        /// <summary>
        /// All connected players
        /// </summary>
        private HashSet<PlayerData> m_players = new HashSet<PlayerData>();

        /// <summary>
        /// All queued packets to be sent to players
        /// </summary>
        private HashSet<ServerPacket> m_packetQueue = new HashSet<ServerPacket>();
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

            m_packetHandlers[typeof(Client.Packet.PlayerConnectPacket)] = OnPlayerConnectPacket;
            m_packetHandlers[typeof(Client.Packet.PlayerMovePacket)] = OnPlayerMovePacket;
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
                        HandlePlayerDisconnect(hostId, connectionId);
                    }
                    break;

                default:
                    break;
            }

            SendQueuedPackets();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hostId"></param>
        /// <param name="connectionId"></param>
        private void HandlePlayerDisconnect(int hostId, int connectionId)
        {
            var player = m_players.FirstOrDefault(x => x.ConnectionId == connectionId);
            if (player != null)
            {
                m_players.Remove(player);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void SendQueuedPackets()
        {
            byte error;
            var formatter = new BinaryFormatter();

            foreach (var serverPacket in m_packetQueue)
            {
                var packet = serverPacket.Packet;
                var packetType = serverPacket.SendType;

                using (var stream = new MemoryStream())
                {
                    formatter.Serialize(stream, packet);
                    var data = stream.ToArray();

                    if (packetType == ServerPacketType.ToConnection)
                    {
                        // Send to specific player
                        NetworkTransport.Send(m_genericHostId, serverPacket.ConnectionId, m_communicationChannel, data, data.Length, out error);
                        Debug.Log(string.Format("Server: Sending Packet {0} to connection {1}", packet.PacketType.ToString(), serverPacket.ConnectionId));
                    }
                    else if (packetType == ServerPacketType.ToAllExcluding)
                    {
                        foreach (var player in m_players.Where(x => !serverPacket.Excluding.Any(y => y == x.ConnectionId)))
                        {
                            // Send to all players, excluding some listed
                            NetworkTransport.Send(m_genericHostId, player.ConnectionId, m_communicationChannel, data, data.Length, out error);
                            Debug.Log(string.Format("Server: Sending Packet {0} to connection {1}", packet.PacketType.ToString(), player.ConnectionId));
                        }
                    }
                    else if (packetType == ServerPacketType.ToAll)
                    {
                        foreach (var player in m_players)
                        {
                            // Send to all players
                            NetworkTransport.Send(m_genericHostId, player.ConnectionId, m_communicationChannel, data, data.Length, out error);
                            Debug.Log(string.Format("Server: Sending Packet {0} to connection {1}", packet.PacketType.ToString(), player.ConnectionId));
                        }
                    }
                }
            }

            m_packetQueue.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packet"></param>
        public void QueuePacket(IPacket packet, int connectionId)
        {
            m_packetQueue.Add(new ServerPacket(ServerPacketType.ToConnection, packet, connectionId));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packet"></param>
        public void QueuePacketAllExcluding(IPacket packet, IEnumerable<int> excludedConnectionIds)
        {
            m_packetQueue.Add(new ServerPacket(ServerPacketType.ToAllExcluding, packet, excludedConnectionIds));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packet"></param>
        public void QueuePacketAll(IPacket packet)
        {
            m_packetQueue.Add(new ServerPacket(ServerPacketType.ToAll, packet));
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
                    if (!m_packetHandlers.ContainsKey(packet.PacketType))
                    {
                        Debug.LogError(string.Format("Server: Unknown packet type {0}", packet.PacketType.ToString()));
                        return;
                    }

                    m_packetHandlers[packet.PacketType](packet, hostId, connectionId);
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
            var packet = rawPacket as Client.Packet.PlayerConnectPacket;
            if (packet == null)
            {
                return;
            }

            var allowConnection = true;
            var errorMessage = "None";
            if (m_players.Any(x => x.Username.Equals(packet.Username, StringComparison.OrdinalIgnoreCase)))
            {
                // Username already in use.
                allowConnection = false;
                errorMessage = string.Format("The username {0} is already in use.", packet.Username);
                Debug.LogError(string.Format("Server: {0}", errorMessage));
            }

            var handShakePacket = new Server.Packet.PlayerHandshakePacket(allowConnection, errorMessage);
            QueuePacket(handShakePacket, connectionId);

            if (allowConnection)
            {
                // Player is valid, store them
                var newPlayerData = new PlayerData(connectionId, packet.Username, new Vector3(-130.0f, 66.5f, 212.0f));
                
                // Player is valid, tell all other players about them.
                var playerJoinPacket = new Server.Packet.PlayerJoinPacket(connectionId, newPlayerData.Username, newPlayerData.CurrentPosition);
                QueuePacketAllExcluding(playerJoinPacket, new int[] { connectionId });

                foreach (var player in m_players)
                {
                    var otherPlayerJoinPack = new Server.Packet.PlayerJoinPacket(player.ConnectionId, player.Username, player.CurrentPosition);
                    QueuePacket(otherPlayerJoinPack, connectionId);
                }

                m_players.Add(newPlayerData);
            }

            Debug.Log(string.Format("Server: Revieved player connection from {0}", packet.Username));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rawPacket"></param>
        /// <param name="hostId"></param>
        /// <param name="connectionId"></param>
        private void OnPlayerMovePacket(IPacket rawPacket, int hostId, int connectionId)
        {
            var packet = rawPacket as Client.Packet.PlayerMovePacket;
            if (packet == null)
            {
                return;
            }

            var player = m_players.FirstOrDefault(x => x.ConnectionId == connectionId);
            if (player == null)
            {
                return;
            }

            player.CurrentPosition = packet.GetPosition();

            Debug.Log(string.Format("Server: Revieved player movement packet from {0}, target {1}", connectionId, packet.GetPosition().ToString()));

            var movementPacket = new Server.Packet.PlayerMovePacket(connectionId, packet.GetPosition());
            QueuePacketAllExcluding(movementPacket, new int[] { connectionId });
        }
    }
}

