using Realms.Common.Packet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;

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

        /// <summary>
        /// 
        /// </summary>
        public string Username = null;

        /// <summary>
        /// The prefab to create when a player joins
        /// </summary>
        public GameObject RemotePlayerPrefab = null;

        /// <summary>
        /// 
        /// </summary>
        public Common.Mob[] MobPrefabs = null;
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

        /// <summary>
        /// All packet handlers
        /// </summary>
        private Dictionary<Type, System.Action<IPacket, int, int>> m_packetHandlers = new Dictionary<Type, Action<IPacket, int, int>>();

        /// <summary>
        /// All connected remote players
        /// </summary>
        private Dictionary<int, RemoteClient> m_remotePlayers = new Dictionary<int, RemoteClient>();

        /// <summary>
        /// All living mobs
        /// </summary>
        private Dictionary<int, Common.Mob> m_mobs = new Dictionary<int, Common.Mob>();
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
            m_genericHostId = NetworkTransport.AddHost(topology, 0);

            byte error;
            m_connectionId = NetworkTransport.Connect(m_genericHostId, this.ServerIP, this.ServerPort, 0, out error);
            if (error != 0 || m_connectionId == 0)
            {
                Debug.LogError(string.Format("Failed to connect to {0}:{1}", this.ServerIP, this.ServerPort));
            }

            m_packetHandlers[typeof(Server.Packet.PlayerHandshakePacket)] = OnPlayerHandshakePacket;
            m_packetHandlers[typeof(Server.Packet.PlayerJoinPacket)] = OnPlayerJoinPacket;
            m_packetHandlers[typeof(Server.Packet.PlayerMovePacket)] = OnPlayerMovePacket;
            m_packetHandlers[typeof(Server.Packet.PlayerDisconnectPacket)] = OnPlayerDisconnectPacket;
            m_packetHandlers[typeof(Server.Packet.PlayerChatPacket)] = OnPlayerChatPacket;
            m_packetHandlers[typeof(Server.Packet.MobSpawnPacket)] = OnMobSpawnPacket;
            m_packetHandlers[typeof(Server.Packet.MobMovePacket)] = OnMobMovePacket;

            SetupUsername();
        }

        /// <summary>
        /// 
        /// </summary>
        private void SetupUsername()
        {
            var usernameField = GameObject.FindGameObjectWithTag("MainMenu") as GameObject;
            if (usernameField != null && usernameField.GetComponent<MainMenu.MainMenuUI>())
            {
                var mainMenu = usernameField.GetComponent<MainMenu.MainMenuUI>();
                this.Username = mainMenu.Username;
            }
            else
            {
                // Random Name
                this.Username = string.Format("Player_{0}", UnityEngine.Random.Range(0, int.MaxValue));
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
                    }
                    break;

                case NetworkEventType.DataEvent:
                    {
                        Debug.Log(string.Format("Client: DataEvent from host {0} connection {1}", hostId, connectionId));
                        HandlePacketRecieved(hostId, connectionId, recBuffer, dataSize);
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

            SendQueuedPackets(m_genericHostId, m_connectionId);
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnApplicationQuit()
        {
            byte error;
            NetworkTransport.Disconnect(m_genericHostId, m_connectionId, out error);
            NetworkTransport.Shutdown();
        }

        /// <summary>
        /// 
        /// </summary>
        private void HandleClientConnectionEvent()
        {
            var packet = new Realms.Client.Packet.PlayerConnectPacket(this.Username);
            QueuePacket(packet);
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
                        Debug.LogError(string.Format("Client: Unknown packet type {0}", packet.PacketType.ToString()));
                        return;
                    }

                    m_packetHandlers[packet.PacketType](packet, hostId, connectionId);
                }
            }
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rawPacket"></param>
        /// <param name="hostId"></param>
        /// <param name="connectionId"></param>
        private void OnPlayerHandshakePacket(IPacket rawPacket, int hostId, int connectionId)
        {
            var packet = rawPacket as Server.Packet.PlayerHandshakePacket;
            if (packet == null)
            {
                return;
            }

            Debug.Log(string.Format("Client: Recieved PlayerHandshakePacket.\nAllowed: {0}, Error: {1}", packet.AllowConnection, packet.ErrorMessage));

            if (!packet.AllowConnection)
            {
                // TODO: Show error ui
                Application.LoadLevel("Menu");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rawPacket"></param>
        /// <param name="hostId"></param>
        /// <param name="connectionId"></param>
        private void OnPlayerJoinPacket(IPacket rawPacket, int hostId, int connectionId)
        {
            var packet = rawPacket as Server.Packet.PlayerJoinPacket;
            if (packet == null)
            {
                return;
            }

            var remotePlayer = (GameObject)GameObject.Instantiate(RemotePlayerPrefab, packet.GetPosition(), Quaternion.identity);
            remotePlayer.name = "RemotePlayer " + packet.Username;
            
            var remoteClient = remotePlayer.GetComponent<RemoteClient>();
            remoteClient.Username = packet.Username;
            
            m_remotePlayers[packet.ConnectionId] = remoteClient;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rawPacket"></param>
        /// <param name="hostId"></param>
        /// <param name="connectionId"></param>
        private void OnPlayerMovePacket(IPacket rawPacket, int hostId, int connectionId)
        {
            var packet = rawPacket as Server.Packet.PlayerMovePacket;
            if (packet == null)
            {
                return;
            }

            var remoteConnectionId = packet.ConnectionId;
            if (!m_remotePlayers.ContainsKey(remoteConnectionId))
            {
                return;
            }

            var remotePlayer = m_remotePlayers[remoteConnectionId];
            remotePlayer.HandleMovePacket(packet);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rawPacket"></param>
        /// <param name="hostId"></param>
        /// <param name="connectionId"></param>
        private void OnPlayerDisconnectPacket(IPacket rawPacket, int hostId, int connectionId)
        {
            var packet = rawPacket as Server.Packet.PlayerDisconnectPacket;
            if (packet == null)
            {
                return;
            }

            var remoteConnectionId = packet.ConnectionId;
            if (!m_remotePlayers.ContainsKey(remoteConnectionId))
            {
                return;
            }

            var remotePlayer = m_remotePlayers[remoteConnectionId];
            GameObject.Destroy(remotePlayer.gameObject);

            m_remotePlayers.Remove(remoteConnectionId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rawPacket"></param>
        /// <param name="hostId"></param>
        /// <param name="connectionId"></param>
        private void OnPlayerChatPacket(IPacket rawPacket, int hostId, int connectionId)
        {
            var packet = rawPacket as Server.Packet.PlayerChatPacket;
            if (packet == null)
            {
                return;
            }

            var remoteConnectionId = packet.ConnectionId;
            if (!m_remotePlayers.ContainsKey(remoteConnectionId))
            {
                return;
            }

            var remotePlayer = m_remotePlayers[remoteConnectionId];
            var chatBox = GameObject.FindObjectOfType<Chat.ChatInputField>();
            chatBox.HandleChatPacket(remotePlayer, packet.ChatMessage);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rawPacket"></param>
        /// <param name="hostId"></param>
        /// <param name="connectionId"></param>
        private void OnMobSpawnPacket(IPacket rawPacket, int hostId, int connectionId)
        {
            var packet = rawPacket as Server.Packet.MobSpawnPacket;
            if (packet == null)
            {
                return;
            }

            if (m_mobs.ContainsKey(packet.ID))
            {
                // This mob already exists
                return;
            }

            var mobTypePrefab = this.MobPrefabs.FirstOrDefault(x => x.name.Equals(packet.MobType, StringComparison.OrdinalIgnoreCase));
            if (mobTypePrefab == null)
            {
                // Mob type not registered on client
                return;
            }

            var spawnLocation = packet.GetPosition();
            var newMob = (Common.Mob)GameObject.Instantiate(mobTypePrefab, spawnLocation, Quaternion.identity); // Initial rotation should be in spawn packet
            newMob.ID = packet.ID;
            m_mobs[packet.ID] = newMob;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rawPacket"></param>
        /// <param name="hostId"></param>
        /// <param name="connectionId"></param>
        private void OnMobMovePacket(IPacket rawPacket, int hostId, int connectionId)
        {
            var packet = rawPacket as Server.Packet.MobMovePacket;
            if (packet == null)
            {
                return;
            }

            if (!m_mobs.ContainsKey(packet.MobID))
            {
                return;
            }

            var mob = m_mobs[packet.MobID];
            mob.HandleMovePacket(packet);
        }
    }
}
