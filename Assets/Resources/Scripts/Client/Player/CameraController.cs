using UnityEngine;
using System.Collections;

namespace Realms.Client.Player
{
    public class CameraController : MonoBehaviour
    {
        private GameObject Player;

        public float ZoomSensitivity;
        public float ZoomKeyboardSensitivity;
        public float LookXSensitvity;
        public float LookYSensitvity;

        public float ZoomMax = 90;
        public float ZoomMin = 30;

        private Vector3 LastPos;

        // Use this for initialization
        void Start()
        {
            Player = GameObject.FindGameObjectWithTag("LocalPlayer");
            
            if (Player == null)
            {
                Debug.LogError("Could not find player");
                enabled = false;
                return;
            }

            LastPos = Player.transform.position;
        }

        // Update is called once per frame
        void LateUpdate()
        {
            // Update position to follow player
            var movement = Player.transform.position - LastPos;
            transform.position += movement;
            LastPos = Player.transform.position;

            var playerController = Player.GetComponent<PlayerController>();

            // Check for rotation
            if (playerController.PlayerControls.LookButton.IsPressed)
            {
                transform.RotateAround(Player.transform.position, Vector3.up, playerController.PlayerControls.LookMouse.X * LookXSensitvity);
                transform.RotateAround(Player.transform.position, transform.right, playerController.PlayerControls.LookMouse.Y * LookYSensitvity);
            }

            transform.RotateAround(Player.transform.position, Vector3.up, playerController.PlayerControls.LookKeyboard.X * LookXSensitvity);
            transform.RotateAround(Player.transform.position, transform.right, playerController.PlayerControls.LookKeyboard.Y * LookYSensitvity);

            // Check for zoom
            var camera = GetComponent<Camera>();

            var value = camera.fieldOfView;
            value += playerController.PlayerControls.Zoom.Value * ZoomSensitivity;
            value += playerController.PlayerControls.ZoomKeyboard.Value * ZoomKeyboardSensitivity;
            value = Mathf.Clamp(value, ZoomMin, ZoomMax);

            camera.fieldOfView = value;
        }
    }
}
