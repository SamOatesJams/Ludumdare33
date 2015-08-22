using UnityEngine;
using System.Collections;
using System;
using System.Linq;

namespace Realms.Client.Chat
{
    public class ChatInputField : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        public UnityEngine.UI.InputField InputField = null;

        /// <summary>
        /// 
        /// </summary>
        public UnityEngine.UI.Text MessageBox = null;

        /// <summary>
        /// 
        /// </summary>
        private LocalClient m_localClient = null;

        /// <summary>
        /// 
        /// </summary>
        void Start()
        {
            m_localClient = GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<LocalClient>();
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnChatSubmit()
        {
            var chatMessage = this.InputField.text;
            this.InputField.text = string.Empty;

            AddMessage("You", chatMessage);

            var chatSendPacket = new Realms.Client.Packet.PlayerChatSendPacket(chatMessage);
            m_localClient.QueuePacket(chatSendPacket);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="chatMessage"></param>
        public void AddMessage(string username, string chatMessage)
        {
            var allText = this.MessageBox.text;
            allText += string.Format("{0}: {1}\n", username, chatMessage);

            // Todo support scroll bar so the number 8 can be like 100
            var lines = allText.Split('\n').ToList();
            if (lines.Count > 8)
            {
                lines.RemoveAt(0);
            }

            this.MessageBox.text = string.Join("\n", lines.ToArray());
        }
    }
}

