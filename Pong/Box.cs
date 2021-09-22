using Microsoft.Xna.Framework;

namespace Pong
{
    public struct Box
    {
        private Vector2 _position;
        public Vector2 position
        {
            get
            {
                return _position;
            }
            set
            {
                var delta = value - _position;
                _position = value;
                TL += delta;
                TR += delta;
                BL += delta;
                BR += delta;
            }
        }

        private Vector2 _size;
        public Vector2 size
        {
            get
            {
                return _size;
            }
            set
            {
                var delta = value - _size;
                _size = value;
                TL += new Vector2(-delta.X, -delta.Y);
                TR += new Vector2(delta.X, -delta.Y);
                BL += new Vector2(-delta.X, delta.Y);
                BR += new Vector2(delta.X, delta.Y);
            }
        }

        public Vector2 TL { get; private set; }
        public Vector2 TR { get; private set; }
        public Vector2 BL { get; private set; }
        public Vector2 BR { get; private set; }

        public Line Top => new Line(TL, TR);
        public Line Bottom => new Line(BL, BR);
        public Line Left => new Line(TL, BL);
        public Line Right => new Line(TR, BR);
        
        public Line[] Edges => new Line[]{Top, Bottom, Left, Right};
        public static readonly Vector2[] normals = { -Vector2.UnitY, Vector2.UnitY, -Vector2.UnitX, Vector2.UnitX }; 
        public Box(Vector2 position, Vector2 size)
        {
            _position = position;
            _size = size;
            
            TL = _position - size / 2;
            BR = _position + size / 2;
            TR = new Vector2(BR.X, TL.Y);
            BL = new Vector2(TL.X, BR.Y);
        }
        public Line[] GetCollisionRays(Vector2 delta)
        {
            return new Line[] {
                new Line(TL, TL + delta),
                new Line(TR, TR + delta),
                new Line(BL, BL + delta),
                new Line(BR, BR + delta)
            };
        }
    }
}
