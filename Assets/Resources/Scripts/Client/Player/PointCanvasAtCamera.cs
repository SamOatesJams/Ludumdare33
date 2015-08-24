using UnityEngine;
using System.Collections;

namespace Realms.Client.Player
{
    public class PointCanvasAtCamera : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        private Camera m_camera = null;

        /// <summary>
        /// 
        /// </summary>
        private Canvas m_canvas = null;

        /// <summary>
        /// 
        /// </summary>
        void Start()
        {
            m_canvas = this.GetComponent<Canvas>();
            m_camera = Camera.main;
        }

        /// <summary>
        /// 
        /// </summary>
        void Update()
        {
            if (m_canvas != null && m_camera != null)
            {
                m_canvas.transform.LookAt(m_camera.transform);
            }
        }
    }
}

