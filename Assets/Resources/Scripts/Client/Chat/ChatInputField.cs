using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;

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
        private Player.PlayerAnimationController m_localAnimationController = null;

        /// <summary>
        /// 
        /// </summary>
        private Dictionary<string, Player.PlayerEmote> m_customEmotes = new Dictionary<string, Player.PlayerEmote>();

        /// <summary>
        /// 
        /// </summary>
        void Start()
        {
            var localPlayer = GameObject.FindGameObjectWithTag("LocalPlayer");
            m_localClient = localPlayer.GetComponent<LocalClient>();
            m_localAnimationController = localPlayer.GetComponent<Player.PlayerAnimationController>();

            foreach (var emote in Enum.GetValues(typeof(Player.PlayerEmote)).Cast<Player.PlayerEmote>())
            {
                m_customEmotes["/" + emote.ToString().ToLower()] = emote;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnChatChange()
        {
            var chatMessage = this.InputField.text;
            if (chatMessage.Length > 32)
            {
                this.InputField.text = chatMessage.Substring(0, 32);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnChatSubmit()
        {
            var chatMessage = this.InputField.text;
            this.InputField.text = string.Empty;

            if (m_customEmotes.ContainsKey(chatMessage.ToLower()))
            {
                AddMessage("You", "Start to " + chatMessage.Substring(1));
                m_localAnimationController.PerformEmote(m_customEmotes[chatMessage.ToLower()]);
            }
            else
            {
                AddMessage("You", chatMessage);
            }
            
            var chatSendPacket = new Realms.Client.Packet.PlayerChatSendPacket(chatMessage);
            m_localClient.QueuePacket(chatSendPacket);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packet"></param>
        public void HandleChatPacket(RemoteClient client, string chatMessage)
        {
            if (m_customEmotes.ContainsKey(chatMessage.ToLower()))
            {
                var animationController = client.GetComponent<Player.PlayerAnimationController>();
                animationController.PerformEmote(m_customEmotes[chatMessage.ToLower()]);
            }
            else
            {
                AddMessage(client.Username, chatMessage);
                client.SetChatLine(chatMessage);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="chatMessage"></param>
        private void AddMessage(string username, string chatMessage)
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

