using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Pong
{
    // Input Handler Singleton
    class InputHandler
    {
        public static InputHandler current;

        public Buttons[] ControllerMapping = { Buttons.DPadUp, Buttons.DPadDown, Buttons.A };
        public Keys[][] KeyboardMapping =
        {
            new Keys[]{Keys.W,Keys.S,Keys.D},
            new Keys[]{Keys.Up,Keys.Down,Keys.Left},
        };

        public InputHandler()
        {            
            current = this;
            playerMovementInput = new int[4] { 0, 0, 0, 0 };
            playerServeInput = new bool[4] { false, false, false, false };
        }

        public void HandleInput()
        {            
            KeyboardState keyboard = Keyboard.GetState();         
                
            for(int i = 0; i < Settings.PlayerCount; i++) {
                GamePadState gamepad = GamePad.GetState(i);
                int moveInput = 0;
                if (keyboard.IsKeyDown(KeyboardMapping[i][0]) || gamepad.IsButtonDown(ControllerMapping[0]))
                    moveInput -= 1;
                else if (keyboard.IsKeyDown(KeyboardMapping[i][1]) || gamepad.IsButtonDown(ControllerMapping[1]))
                    moveInput += 1;
                playerMovementInput[i] = moveInput;
                playerServeInput[i] = keyboard.IsKeyDown(KeyboardMapping[i][2]) || gamepad.IsButtonDown(ControllerMapping[2]);
            }
        }

        public int[] playerMovementInput { get; private set; }
        public bool[] playerServeInput { get; private set; }

        // TODO: integrate
        public int player1MoveInput;
        public int player2MoveInput;
        public bool player1ServeInput;
        public bool player2ServeInput;
    }
}
