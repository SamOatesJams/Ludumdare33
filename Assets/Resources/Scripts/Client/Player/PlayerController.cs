using UnityEngine;
using InControl;
using System.Collections;

namespace Realms.Client.Player
{
    public class PlayerController : MonoBehaviour
    {
        public Camera Camera;

        private Vector3 lastLocation;
        private NavMeshAgent navAgent;
        private PlayerControls playerControls;

        // Use this for initialization
        void Start()
        {
            navAgent = GetComponent<NavMeshAgent>();

            playerControls = new PlayerControls();
            playerControls.SetupControls();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (playerControls.MouseLeft.IsPressed)
            {
                Debug.Log("Mouse clicked");
                RaycastHit hit;
                Ray ray = Camera.ScreenPointToRay(Input.mousePosition);
                Physics.Raycast(ray, out hit);

                if (hit.collider)
                {
                    Debug.Log("Moving to");
                    navAgent.destination = hit.point;
                }
            }
        }
    }
}
