using UnityEngine;
using InControl;
using System.Collections;

namespace Realms.Client.Player
{
    public class PlayerControls : PlayerActionSet
    {
        public PlayerAction MouseLeft;
        public PlayerAction MouseRight;

        public PlayerControls()
        {
            MouseLeft = CreatePlayerAction("Mouse Left Click");
            MouseRight = CreatePlayerAction("Mouse Right Click");
        }

        public void SetupControls()
        {
            MouseLeft.AddDefaultBinding(Mouse.LeftButton);
            MouseRight.AddDefaultBinding(Mouse.RightButton);
        }
    }
}


