using UnityEngine;
using InControl;
using System.Collections;

namespace Realms.Client.Player
{
    public class PlayerControls : PlayerActionSet
    {
        public PlayerAction MouseLeft;
        public PlayerAction MouseRight;

        public PlayerAction ZoomIn;
        public PlayerAction ZoomOut;
        public PlayerOneAxisAction Zoom;

        public PlayerAction LookButton;

        public PlayerAction LookPositiveX;
        public PlayerAction LookPositiveY;
        public PlayerAction LookNegativeX;
        public PlayerAction LookNegativeY;
        public PlayerTwoAxisAction Look;

        public PlayerControls()
        {
            MouseLeft = CreatePlayerAction("Mouse Left Click");
            MouseRight = CreatePlayerAction("Mouse Right Click");

            LookButton = CreatePlayerAction("Mouse look");

            ZoomIn = CreatePlayerAction("Zoom in");
            ZoomOut = CreatePlayerAction("Zoom out");
            Zoom = CreateOneAxisPlayerAction(ZoomIn, ZoomOut);

            LookPositiveX = CreatePlayerAction("Look positive X");
            LookPositiveY = CreatePlayerAction("Look positive Y");
            LookNegativeX = CreatePlayerAction("Look negative X");
            LookNegativeY = CreatePlayerAction("Look negative Y");
            Look = CreateTwoAxisPlayerAction(LookNegativeX, LookPositiveX, LookNegativeY, LookPositiveY);
        }

        public void SetupControls()
        {
            MouseLeft.AddDefaultBinding(Mouse.LeftButton);
            MouseRight.AddDefaultBinding(Mouse.RightButton);

            LookButton.AddDefaultBinding(Mouse.MiddleButton);

            ZoomIn.AddDefaultBinding(Mouse.PositiveScrollWheel);
            ZoomOut.AddDefaultBinding(Mouse.NegativeScrollWheel);

            LookPositiveX.AddDefaultBinding(Mouse.PositiveX);
            LookPositiveY.AddDefaultBinding(Mouse.PositiveY);
            LookNegativeX.AddDefaultBinding(Mouse.NegativeX);
            LookNegativeY.AddDefaultBinding(Mouse.NegativeY);
        }
    }
}


