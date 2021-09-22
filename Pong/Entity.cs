using Microsoft.Xna.Framework;

namespace Pong
{
    // TODO: Encapsulate!!!!
    public class Entity
    {
        public Box transform;
        public Vector2 velocity;

        public Entity(Vector2 position, Vector2 size)
        {
            this.transform = new Box(position, size);
            this.velocity = Vector2.Zero;
        }
        public Entity(Vector2 position, Vector2 size, Vector2 velocity)
        {
            this.transform = new Box(position, size);
            this.velocity = velocity;
        }

        
    }
}
