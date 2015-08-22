using UnityEngine;
using System.Collections;
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
        #endregion

        /// <summary>
        /// 
        /// </summary>
        void Start()
        {
            m_configuration = new ConnectionConfig();
            m_communicationChannel = m_configuration.AddChannel(QosType.Reliable);

            NetworkTransport.Init();

            var topology = new HostTopology(m_configuration, this.MaxConnections);
            m_genericHostId = NetworkTransport.AddHost(topology, 0);

            byte error;
            m_connectionId = NetworkTransport.Connect(m_genericHostId, this.ServerIP, this.ServerPort, 0, out error);
        }

        /// <summary>
        /// 
        /// </summary>
        void FixedUpdate()
        {
            const int bufferSize = 1024;

            int recHostId, connectionId, channelId, dataSize;
            byte[] recBuffer = new byte[bufferSize];
            byte error;

            var dataEvent = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);

            switch (dataEvent)
            {
                case NetworkEventType.ConnectEvent:
                    {
                        Debug.Log(string.Format("Client: ConnectEvent from host {0} connection {1}", recHostId, connectionId));
                    }
                    break;

                case NetworkEventType.DataEvent:
                    {
                        Debug.Log(string.Format("Client: DataEvent from host {0} connection {1}", recHostId, connectionId));
                    }
                    break;

                case NetworkEventType.DisconnectEvent:
                    {
                        Debug.Log(string.Format("Client: DisconnectEvent from host {0} connection {1}", recHostId, connectionId));
                    }
                    break;

                default:
                    break;
            }
        }
    }
}
