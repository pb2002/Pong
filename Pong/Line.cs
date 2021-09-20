using Microsoft.Xna.Framework;

namespace Pong
{
    // Object representing a 2D line segment
    public struct Line
    {
        public Vector2 start;
        public Vector2 end;

        // constructor
        public Line(Vector2 start, Vector2 end)
        {
            this.start = start;
            this.end = end;
        }        
    }

}
