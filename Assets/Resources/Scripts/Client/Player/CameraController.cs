using UnityEngine;
using System.Collections;

namespace Realms.Client.Player
{
    public class CameraController : MonoBehaviour
    {
        public GameObject Player;
        public float ZoomSensitivity;
        public float LookXSensitvity;
        public float LookYSensitvity;

        private Vector3 LastPos;

        // Use this for initialization
        void Start()
        {
            Player = GameObject.FindGameObjectsWithTag("LocalPlayer")[0];

            if (Player == null)
            {
                Debug.LogError("Could not find player");
                enabled = false;
                return;
            }

            LastPos = Player.transform.position;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            // Update position to follow player
            var movement = Player.transform.position - LastPos;
            transform.position += movement;
            LastPos = Player.transform.position;

            var playerController = Player.GetComponent<PlayerController>();

            // Check for rotation
            if (playerController.PlayerControls.LookButton.IsPressed)
            {
                transform.RotateAround(Player.transform.position, Vector3.up, playerController.PlayerControls.Look.X * LookXSensitvity);
                transform.RotateAround(Player.transform.position, transform.right, playerController.PlayerControls.Look.Y * LookYSensitvity);
            }

            // Check for zoom
            var camera = GetComponent<Camera>();
            var value = camera.fieldOfView + (playerController.PlayerControls.Zoom.Value * ZoomSensitivity);
            value = Mathf.Clamp(value, 30, 90);
            camera.fieldOfView = value;
        }
    }
}
