using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Pong
{
    class InputHandler
    {
        public static InputHandler current;
        public InputHandler()
        {            
            current = this;
            playerMovementInput = new int[Settings.PlayerCount];
        }
        public int[] playerMovementInput { get; private set; }
        public bool[] playerServeInput { get; private set; }

        public int player1MoveInput;
        public int player2MoveInput;
        public bool player1ServeInput;
        public bool player2ServeInput;
    }
}
