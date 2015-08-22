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

        public PlayerAction ZoomKeyboardIn;
        public PlayerAction ZoomKeyboardOut;
        public PlayerOneAxisAction ZoomKeyboard;

        public PlayerAction LookButton;

        public PlayerAction MousePositiveX;
        public PlayerAction MousePositiveY;
        public PlayerAction MouseNegativeX;
        public PlayerAction MouseNegativeY;
        public PlayerTwoAxisAction LookMouse;

        public PlayerAction KeyboardPositiveX;
        public PlayerAction KeyboardPositiveY;
        public PlayerAction KeyboardNegativeX;
        public PlayerAction KeyboardNegativeY;
        public PlayerTwoAxisAction LookKeyboard;

        public PlayerControls()
        {
            MouseLeft = CreatePlayerAction("Mouse Left Click");
            MouseRight = CreatePlayerAction("Mouse Right Click");

            LookButton = CreatePlayerAction("Mouse look");

            ZoomIn = CreatePlayerAction("Zoom in (mouse)");
            ZoomOut = CreatePlayerAction("Zoom out (mouse)");
            Zoom = CreateOneAxisPlayerAction(ZoomIn, ZoomOut);

            ZoomKeyboardIn = CreatePlayerAction("Zoom in (keyboard)");
            ZoomKeyboardOut = CreatePlayerAction("Zoom out (keyboard)");
            ZoomKeyboard = CreateOneAxisPlayerAction(ZoomKeyboardIn, ZoomKeyboardOut);

            MousePositiveX = CreatePlayerAction("Look positive X (mouse)");
            MousePositiveY = CreatePlayerAction("Look positive Y (mouse)");
            MouseNegativeX = CreatePlayerAction("Look negative X (mouse)");
            MouseNegativeY = CreatePlayerAction("Look negative Y (mouse)");
            LookMouse = CreateTwoAxisPlayerAction(MouseNegativeX, MousePositiveX, MouseNegativeY, MousePositiveY);

            KeyboardPositiveX = CreatePlayerAction("Look positive X (keyboard)");
            KeyboardPositiveY = CreatePlayerAction("Look positive Y (keyboard)");
            KeyboardNegativeX = CreatePlayerAction("Look negative X (keyboard)");
            KeyboardNegativeY = CreatePlayerAction("Look negative Y (keyboard)");
            LookKeyboard = CreateTwoAxisPlayerAction(KeyboardNegativeX, KeyboardPositiveX, KeyboardNegativeY, KeyboardPositiveY);
        }

        public void SetupControls()
        {
            MouseLeft.AddDefaultBinding(Mouse.LeftButton);
            MouseRight.AddDefaultBinding(Mouse.RightButton);

            LookButton.AddDefaultBinding(Mouse.MiddleButton);

            ZoomIn.AddDefaultBinding(Mouse.PositiveScrollWheel);
            ZoomOut.AddDefaultBinding(Mouse.NegativeScrollWheel);

            ZoomKeyboardIn.AddDefaultBinding(Key.PageUp);
            ZoomKeyboardOut.AddDefaultBinding(Key.PageDown);
            
            MousePositiveX.AddDefaultBinding(Mouse.PositiveX);
            MousePositiveY.AddDefaultBinding(Mouse.PositiveY);
            MouseNegativeX.AddDefaultBinding(Mouse.NegativeX);
            MouseNegativeY.AddDefaultBinding(Mouse.NegativeY);

            KeyboardPositiveX.AddDefaultBinding(Key.A);
            KeyboardPositiveY.AddDefaultBinding(Key.W);
            KeyboardNegativeX.AddDefaultBinding(Key.D);
            KeyboardNegativeY.AddDefaultBinding(Key.S);
        }
    }
}


