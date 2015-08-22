using UnityEngine;
using InControl;
using System.Collections;

namespace Realms.Client.Player
{
    public class PlayerController : MonoBehaviour
    {
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
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                Physics.Raycast(ray, out hit);

                if (hit.collider)
                {
                    navAgent.destination = hit.point;
                }
            }
        }
    }
}
