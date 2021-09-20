using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pong
{
    public class Renderer
    {
        public static Renderer current;
        
        public SpriteBatch spriteBatch;
        private GraphicsDevice gd;
        
        public static readonly Color scoreColor = new Color(24, 144, 192);
        public static readonly Color playerColor = new Color(32,224,255);
        public static readonly Color ballColor = new Color(192,255,255);
        public static readonly Color red = Color.Coral;
        public Renderer(GraphicsDevice gd)
        {
            current = this;
            this.gd = gd;
        }
        public void DrawSpriteCentered(Texture2D sprite, Vector2 position, Color color, bool flip = false)
        {
            var offset = new Vector2(sprite.Width / 2f, sprite.Height / 2f);            
            spriteBatch.Draw(sprite, position - offset, null, color, 0, Vector2.Zero, 1, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.5f);
        }
        // Draws text centered at the given position
        public void Begin() => spriteBatch.Begin();
        public void End() => spriteBatch.End();
        public void DrawTextCentered(SpriteFont font, string text, Vector2 position, Color color)
        {
            var textMiddlePoint = font.MeasureString(text) / 2;
            spriteBatch.DrawString(font, text, position, color, 0, textMiddlePoint, 1.0f, SpriteEffects.None, 0.5f);
        }
        
    }
}