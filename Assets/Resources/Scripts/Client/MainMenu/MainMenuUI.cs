using UnityEngine;
using System.Collections;

namespace Realms.Client.MainMenu
{
    public class MainMenuUI : MonoBehaviour
    {
        public GameObject UI = null;
        public GameObject Environment = null;
        public UnityEngine.UI.InputField UsernameInputField = null;
        public UnityEngine.UI.Text ErrorText = null;
        public string MainWorldName = "Dev-World";

        public string Username { get; private set; }

        private bool m_shouldLogin = false;

        /// <summary>
        /// 
        /// </summary>
        public void OnUserLogin()
        {
            var text = UsernameInputField.text;
            if (text.Length > 5 && text.Length < 16)
            {
                this.ErrorText.text = string.Empty;
                this.Username = text;
                m_shouldLogin = true;
            }
            else
            {
                this.ErrorText.text = "Username should be 6 - 15 characters";
            }
        }        

        private void FixedUpdate()
        {
            if (m_shouldLogin == true)
            {
                m_shouldLogin = false;
                GameObject.Destroy(this.Environment);
                GameObject.Destroy(this.UI);
                Application.LoadLevelAdditive(this.MainWorldName);
            }
        }
    }
}
