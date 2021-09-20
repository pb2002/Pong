using Microsoft.Xna.Framework;

namespace Pong
{
    public class Player : Entity
    {
        public Vector2 renderPosition;
        public float movementSpeed;
        public bool serving;
        public int lives;

        public Player(Vector2 position, Vector2 size, bool serving, float movementSpeed) : base(position, size)
        {
            this.renderPosition = position;
            this.serving = serving;
            this.movementSpeed = movementSpeed;

            this.lives = 3;
        }        
        public void Move(int dir, float dt)
        {
            var pos = transform.position;

            velocity = Vector2.UnitY * dir * movementSpeed;
            pos.Y += velocity.Y * dt;
            pos.Y = MathHelper.Clamp(pos.Y, Settings.playFieldMargin.Y + transform.size.Y / 2, 
                                            Settings.screenSize.Y - Settings.playFieldMargin.Y - transform.size.Y / 2);
            transform.position = pos;
            renderPosition = Utils.Lerp(renderPosition, pos, 15 * dt);            
        }
    }
}
