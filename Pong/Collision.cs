using Microsoft.Xna.Framework;

namespace Pong
{
    public struct Collision
    {
        public Vector2 hit;
        public Vector2 intersection;
        public int hitEdgeIndex;
        public Vector2 corner;
    }
}
