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
        /// All offline players
        /// </summary>
        private HashSet<PlayerData> m_offlinePlayers = new HashSet<PlayerData>();

        /// <summary>
        /// All queued packets to be sent to players
        /// </summary>
        private HashSet<ServerPacket> m_packetQueue = new HashSet<ServerPacket>();

        /// <summary>
        /// All registered mobs
        /// </summary>
        private HashSet<Realms.Common.Mob> m_mobs = new HashSet<Common.Mob>();
        #endregion

        /// <summary>
        /// 
        /// </summary>
        private void Start()
        {
            // Multiplayer game, don't allow pausing
            Application.runInBackground = true;

            m_configuration = new ConnectionConfig();
            m_communicationChannel = m_configuration.AddChannel(QosType.Reliable);

            NetworkTransport.Init();

            var topology = new HostTopology(m_configuration, this.MaxConnections);
            m_webSocketHostId = NetworkTransport.AddWebsocketHost(topology, this.ServerPort, null);

#if !UNITY_WEBGL
            m_genericHostId = NetworkTransport.AddHost(topology, this.ServerPort, null);
#endif

            Debug.Log(string.Format("Server started.\nPort: {0}. Max Connections: {1}. WebSocketID: {2}.GenericHostID: {3}", 
                this.ServerPort,
                this.MaxConnections,
                m_webSocketHostId,
                m_genericHostId)
            );

            m_packetHandlers[typeof(Client.Packet.PlayerConnectPacket)] = OnPlayerConnectPacket;
            m_packetHandlers[typeof(Client.Packet.PlayerMovePacket)] = OnPlayerMovePacket;
            m_packetHandlers[typeof(Client.Packet.PlayerChatSendPacket)] = OnPlayerChatSendPacket;
            m_packetHandlers[typeof(Client.Packet.PlayerAttackPacket)] = OnPlayerAttackPacket;
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
                    }
                    break;

                case NetworkEventType.DataEvent:
                    {
                        HandlePacketRecieved(hostId, connectionId, recBuffer, dataSize);
                    }
                    break;

                case NetworkEventType.DisconnectEvent:
                    {
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
                m_offlinePlayers.Add(player);

                var disconnectPacket = new Server.Packet.PlayerDisconnectPacket(player.ConnectionId);
                QueuePacketAll(disconnectPacket);
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

            // Check for already existing username
            if (m_players.Any(x => x.Username.Equals(packet.Username, StringComparison.OrdinalIgnoreCase)))
            {
                // Username already in use.
                allowConnection = false;
                errorMessage = string.Format("The username {0} is already in use.", packet.Username);
                Debug.LogError(string.Format("Server: {0}", errorMessage));
            }

            var spawnPoint = new Vector3(-280.0f, 47.5f, 338.0f);

            if (allowConnection)
            {
                // Try get an offline player and get their location
                var offlinePlayer = m_offlinePlayers.FirstOrDefault(x => x.Username.Equals(packet.Username, StringComparison.OrdinalIgnoreCase));
                if (offlinePlayer != null)
                {
                    spawnPoint = offlinePlayer.CurrentPosition;
                }

                // Player is valid, store them
                var newPlayerData = new PlayerData(connectionId, packet.Username, spawnPoint);
                
                // Player is valid, tell all other players about them.
                var playerJoinPacket = new Server.Packet.PlayerJoinPacket(connectionId, newPlayerData.Username, newPlayerData.CurrentPosition);
                QueuePacketAllExcluding(playerJoinPacket, new int[] { connectionId });

                foreach (var player in m_players)
                {
                    var otherPlayerJoinPack = new Server.Packet.PlayerJoinPacket(player.ConnectionId, player.Username, player.CurrentPosition);
                    QueuePacket(otherPlayerJoinPack, connectionId);
                }

                m_players.Add(newPlayerData);

                if (offlinePlayer != null)
                {
                    m_offlinePlayers.Remove(offlinePlayer);
                }

                SendMobSpawnPackets(newPlayerData.ConnectionId);
            }

            var handShakePacket = new Server.Packet.PlayerHandshakePacket(allowConnection, errorMessage, spawnPoint);
            QueuePacket(handShakePacket, connectionId);

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

            var movementPacket = new Server.Packet.PlayerMovePacket(connectionId, packet.GetPosition());
            QueuePacketAllExcluding(movementPacket, new int[] { connectionId });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rawPacket"></param>
        /// <param name="hostId"></param>
        /// <param name="connectionId"></param>
        private void OnPlayerChatSendPacket(IPacket rawPacket, int hostId, int connectionId)
        {
            var packet = rawPacket as Client.Packet.PlayerChatSendPacket;
            if (packet == null)
            {
                return;
            }

            var chatPacket = new Server.Packet.PlayerChatPacket(connectionId, packet.ChatMessage);
            QueuePacketAllExcluding(chatPacket, new int[] { connectionId });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rawPacket"></param>
        /// <param name="hostId"></param>
        /// <param name="connectionId"></param>
        private void OnPlayerAttackPacket(IPacket rawPacket, int hostId, int connectionId)
        {
            var packet = rawPacket as Client.Packet.PlayerAttackPacket;
            if (packet == null)
            {
                return;
            }

            Debug.Log("Server: Player Attack Packet Received - 1");

            var mob = m_mobs.FirstOrDefault(x => x.ID == packet.MobID);
            if (mob == null)
            {
                return;
            }

            Debug.Log("Server: Player Attack Packet Received - 2");

            mob.Health -= packet.Damage;

            if (mob.Health > 0)
            {
                Debug.Log("Server: Sending Damage Packet");
                var mobDamagePacket = new Server.Packet.MobDamagedPacket(mob.ID, mob.Health);
                QueuePacketAll(mobDamagePacket);
            }
            else
            {
                Debug.Log("Server: Sending Death Packet");

                // It's dead mate
                var mobDeathPacket = new Server.Packet.MobDeathPacket(mob.ID);
                QueuePacketAll(mobDeathPacket);

                m_mobs.Remove(mob);
                var spawner = mob.SpawnArea.GetComponent<Server.SpawnArea>();
                spawner.RequestSpawn();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mob"></param>
        public void RegisterMob(Realms.Common.Mob mob)
        {
            m_mobs.Add(mob);

            var mobSpawnPacket = new Server.Packet.MobSpawnPacket(mob);
            QueuePacketAll(mobSpawnPacket);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionId"></param>
        private void SendMobSpawnPackets(int connectionId)
        {
            foreach (var mob in m_mobs)
            {
                var mobSpawnPacket = new Server.Packet.MobSpawnPacket(mob);
                QueuePacket(mobSpawnPacket, connectionId);
            }
        }
    }
}

