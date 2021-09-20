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
        public Line[] GetCollisionRays(Vector2 delta)
        {
            return new Line[] {
                new Line(transform.TL, transform.TL + delta),
                new Line(transform.TR, transform.TR + delta),
                new Line(transform.BL, transform.BL + delta),
                new Line(transform.BR, transform.BR + delta)
            };
        }
    }
}
