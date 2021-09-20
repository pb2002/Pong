using Microsoft.Xna.Framework;

namespace Pong
{
    public static class Settings
    {
        // screen size
        public static Vector2 screenSize { get; } = new Vector2(1280, 720);

        // margins of the playing field
        public static Vector2 playFieldMargin { get; } = new Vector2(0, 0);

        // distance between screen border and paddles
        public static float playerHOffset { get; } = 128;

        // distance between paddle and ball when serving
        public static Vector2 BallServeOffset { get; } = new Vector2(8, 0);

        // hitbox size of the player
        public static Vector2 PlayerSize { get; } = new Vector2(32, 128);
        // hitbox size of the ball
        public static Vector2 BallSize { get; } = new Vector2(32, 32);

        public static int PlayerCount { get; } = 2;

        // speed multiplier on each bounce with the paddle
        public static float SpeedMultiplier { get; } = 1.03f;

        // initial ball velocity
        public static float BallStartSpeed { get; } = 200;
        // initial player velocity
        public static float PlayerStartSpeed { get; } = 350;
    }
}
