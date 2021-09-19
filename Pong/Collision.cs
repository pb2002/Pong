using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Pong
{
    public struct Line
    {
        public Vector2 start;
        public Vector2 end;
        public Line(Vector2 start, Vector2 end)
        {
            this.start = start;
            this.end = end;
        }        
    }
    public struct Box
    {
        public Vector2 center;
        public Vector2 size;

        public Box(Vector2 center, Vector2 size)
        {
            this.center = center;
            this.size = size;
        }
    }
}
