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

        private int player1MoveInput;
        private int player2MoveInput;
        private bool player1ServeInput;
        private bool player2ServeInput;
    }
}
