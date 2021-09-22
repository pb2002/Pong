using Microsoft.Xna.Framework;

namespace Pong
{
    public class Player : Entity
    {
        public int id;

        public Vector2 renderPosition;
        public bool serving;
        public float movementSpeedMultiplier;
        public int lives;
        public int frontEdge;
        private readonly int[] idFrontEdgeLUT = { 3, 2, 1, 0 };
        public Player(int id, Vector2 position, Vector2 size, bool serving) : base(position, size)
        {
            this.id = id;
            this.serving = serving;
            
            renderPosition = position;
            frontEdge = idFrontEdgeLUT[id];

            movementSpeedMultiplier = 1;
            lives = 3;
        }
        public void Move(int dir, float baseSpeed, float dt)
        {
            Vector2 pos = transform.position;

            velocity = Vector2.UnitY * dir * movementSpeedMultiplier * baseSpeed;

            pos.Y += velocity.Y * dt;            
            pos.Y = MathHelper.Clamp(pos.Y, Settings.playFieldMargin.Y + transform.size.Y / 2, 
                                            Settings.screenSize.Y - Settings.playFieldMargin.Y - transform.size.Y / 2);
            transform.position = pos;
            renderPosition = Utils.Lerp(renderPosition, pos, 25 * dt);            
        }
    }
}
