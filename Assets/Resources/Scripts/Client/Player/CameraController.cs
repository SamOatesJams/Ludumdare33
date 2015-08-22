using UnityEngine;
using System.Collections;

namespace Realms.Client.Player
{
    public class CameraController : MonoBehaviour
    {
        public GameObject Player;

        private Vector3 LastPos;

        // Use this for initialization
        void Start()
        {
            LastPos = Player.transform.position;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            var movement = Player.transform.position - LastPos;
            transform.position += movement;
            LastPos = Player.transform.position;
        }
    }
}
