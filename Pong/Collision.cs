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
        public Vector2 a;
        public Vector2 b;
        public Line(Vector2 a, Vector2 b)
        {
            this.a = a;
            this.b = b;
        }        
    }
    public struct Rect
    {
        public Vector2 center;
        public Vector2 size;
        public Vector2 TL { get { return center - size / 2; } }
        public Vector2 BR { get { return center + size / 2; } }
        public Vector2 TR { get { return new Vector2(BR.X, TL.Y); } }
        public Vector2 BL { get { return new Vector2(TL.X, BR.Y); } }

        public Rect(Vector2 center, Vector2 size)
        {
            this.center = center;
            this.size = size;
        }
    }
}
