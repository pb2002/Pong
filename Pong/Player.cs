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
        public Vector2 normal;
        public Player(int id, Vector2 position, Vector2 size, bool serving) : base(position, size)
        {
            this.id = id;
            this.renderPosition = position;
            this.serving = serving;
            this.movementSpeedMultiplier = 1;
            this.lives = 3;
            switch (id)
            {
                case 0:
                    this.normal = new Vector2(1, 0);
                    break;
                case 1:
                    this.normal = new Vector2(-1, 0);
                    break;
            }
        }        
        public Line[] GetCollisionEdges()
        {
            switch (id)
            {
                case 0:
                    return new Line[] { transform.Right, transform.Top, transform.Bottom };                    
                case 1:
                    return new Line[] { transform.Left, transform.Top, transform.Bottom };
                default:
                    return new Line[] { transform.Right, transform.Top, transform.Bottom };
            }
            
        }
        public void Move(int dir, float baseSpeed, float dt)
        {
            var pos = transform.position;

            velocity = Vector2.UnitY * dir * movementSpeedMultiplier * baseSpeed;

            pos.Y += velocity.Y * dt;            
            pos.Y = MathHelper.Clamp(pos.Y, Settings.playFieldMargin.Y + transform.size.Y / 2, 
                                            Settings.screenSize.Y - Settings.playFieldMargin.Y - transform.size.Y / 2);
            transform.position = pos;
            renderPosition = Utils.Lerp(renderPosition, pos, 25 * dt);            
        }
    }
}
