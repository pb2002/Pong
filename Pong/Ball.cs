using Microsoft.Xna.Framework;

namespace Pong
{
    public class Ball : Entity
    {
        public Ball(Vector2 position, Vector2 size, Vector2 velocity) : base(position, size, velocity) { }
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
