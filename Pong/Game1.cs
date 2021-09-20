using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Pong
{
    public class PongGame : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        // objects
        private Player[] players;
        private Ball ball;

        
        private Line topWall;
        private Line bottomWall;

        // textures            
        private Texture2D playerTexture;
        private Texture2D ballTexture;
        private SpriteFont font;

        private void DrawSpriteCentered(Texture2D sprite, Vector2 position, Color color)
        {
            Vector2 offset = new Vector2(sprite.Width / 2, sprite.Height / 2);
            spriteBatch.Draw(sprite, position - offset, color);
        }

        private void DrawTextCentered(string text, Vector2 position)
        {
            Vector2 textMiddlePoint = font.MeasureString(text) / 2;
            spriteBatch.DrawString(font, text, position, Color.White, 0, textMiddlePoint, 1.0f, SpriteEffects.None, 0.5f);
        }

        private void MovePlayers(float dt)
        {
            for(int i = 0; i < Settings.PlayerCount; i++)
            {
                players[i].Move(InputHandler.current.playerMovementInput[i], dt);
            }
        }
        private Vector2 ResolveCollision(Vector2 start, Vector2 hit, Vector2 n, Vector2 delta)
        {
            float deltaLength = delta.Length();
            float startToHitLength = (start - hit).Length();

            // distance to travel after collision
            float remainingTravelDistance = deltaLength - startToHitLength;

            // calculate reflected travel direction
            Vector2 dn = Vector2.Normalize(delta);
            Vector2 rdn = dn.Reflect(n);

            // final position
            return rdn * remainingTravelDistance;
        }        
        private void MoveBall(float dt)
        {
            bool serving = false;
            for(int i = 0; i < Settings.PlayerCount; i++)
            {
                Player player = players[i];
                if (player.serving)
                {
                    serving = true;
                    // use render position here so the ball doesn't lag behind
                    ball.transform.position = player.renderPosition + player.normal * Settings.BallServeOffset;
                    ball.transform.position += player.normal * (player.transform.size.X / 2 + ball.transform.size.X / 2);
                    if (InputHandler.current.playerServeInput[i])
                    {
                        player.serving = false;
                        int dir = InputHandler.current.playerMovementInput[i] == 0 ? 1 : InputHandler.current.playerMovementInput[i];
                        ball.velocity = player.normal * Settings.BallStartSpeed + new Vector2(0, Settings.BallStartSpeed * dir);
                    }
                }
            }
            if (!serving)
            {
                Vector2 deltaPos = ball.velocity * dt;

                bool collided = true;

                // repeat collision checks until there are no collisions
                while (collided)
                {
                    collided = false;
                    
                    Line[] collisionRays = ball.GetCollisionRays(deltaPos);
                    
                    

                    Vector2 nextPos = ball.transform.position + deltaPos;
                    Vector2 intersect;

                    // a collision ray will always hit a player before hitting a wall (can be deduced from playfield layout)

                    // Player Collision Check

                    for(int i = 0; i < Settings.PlayerCount; i++)
                    {
                        Player player = players[i];
                        if (Utils.ClosestLineIntersection(collisionRays, player.GetCollisionEdges(), out intersect, out int ia, out int ib, out float t))
                        {
                            Vector2 n;
                            switch (ib)
                            {
                                case 0:
                                    n = player.normal;
                                    break;
                                default:
                                    n = Vector2.Normalize(-deltaPos); // side collision: reverse travel direction
                                    break;
                            }

                            deltaPos = ResolveCollision(collisionRays[ia].start, intersect, n, deltaPos);
                            nextPos = intersect + ball.transform.position - collisionRays[ia].start + n * 0.01f;
                            collided = true;
                            break;
                        }
                    }

                    // Top Wall Collision Check
                    // walls extend out to infinity so only one ray is needed here
                    if (deltaPos.Y < 0)
                    {
                        if (Utils.LineIntersection(topWall, collisionRays[0], out intersect))
                        {
                            Vector2 n = new Vector2(0, 1);
                            deltaPos = ResolveCollision(collisionRays[0].start, intersect, n, deltaPos);
                            nextPos = intersect + ball.transform.position - ball.transform.TL;
                            collided = true;
                        }
                    }
                    // Bottom Wall Collision Check
                    else
                    {
                        if (Utils.LineIntersection(bottomWall, collisionRays[2], out intersect))
                        {
                            Vector2 n = new Vector2(0, -1);
                            deltaPos = ResolveCollision(collisionRays[2].start, intersect, n, deltaPos);
                            nextPos = intersect + ball.transform.position - ball.transform.BL;
                            collided = true;
                        }
                    }
                    // Update ball position
                    ball.transform.position = nextPos;
                }
                ball.velocity = Vector2.Normalize(deltaPos) * ball.velocity.Length();
            }
        }

        private void CheckMiss()
        {
            if (ball.transform.position.X < 0)
            {
                // player 1 missed
                players[0].lives -= 1;
                players[0].serving = true;
            }
            else if (ball.transform.position.X > Settings.screenSize.X)
            {
                // player 2 missed
                players[1].lives -= 1;
                players[1].serving = true;
            }
        }

        public PongGame()
        {
            
            graphics = new GraphicsDeviceManager(this);
            _ = new InputHandler();
            
            graphics.PreferredBackBufferWidth = (int)Settings.screenSize.X;
            graphics.PreferredBackBufferHeight = (int)Settings.screenSize.Y;
            graphics.ApplyChanges();

            topWall = new Line(
                Settings.playFieldMargin, 
                new Vector2(Settings.screenSize.X - Settings.playFieldMargin.X, Settings.playFieldMargin.Y)
                );

            bottomWall = new Line(
                new Vector2(Settings.playFieldMargin.X, Settings.screenSize.Y - Settings.playFieldMargin.Y),
                Settings.screenSize - Settings.playFieldMargin
                );

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            IsFixedTimeStep = false;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            players = new Player[]{ 
                new Player(
                    0,
                    new Vector2(Settings.playerHOffset, Settings.screenSize.Y/2), 
                    Settings.PlayerSize, 
                    true, 
                    Settings.PlayerStartSpeed
                    ),
                new Player(
                    1,
                    new Vector2(Settings.screenSize.X - Settings.playerHOffset, Settings.screenSize.Y/2), 
                    Settings.PlayerSize, 
                    false, 
                    Settings.PlayerStartSpeed) 
            };

            ball = new Ball(new Vector2(Settings.screenSize.X / 2, Settings.screenSize.Y / 2),
                                    Settings.BallSize,
                                    Vector2.Zero);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            ballTexture = Content.Load<Texture2D>("ball");
            playerTexture = Content.Load<Texture2D>("player");
            font = Content.Load<SpriteFont>("font");
        }

        protected override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            InputHandler.current.HandleInput();
            MovePlayers(dt);
            MoveBall(dt);
            CheckMiss();
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();
            foreach(Player player in players)
            {
                DrawSpriteCentered(playerTexture, player.renderPosition, Color.LightGoldenrodYellow);
            }
            DrawSpriteCentered(ballTexture, ball.transform.position, Color.Coral);

            DrawTextCentered(players[0].lives.ToString(), new Vector2(Settings.screenSize.X * 0.25f, Settings.screenSize.Y * 0.5f));
            DrawTextCentered(players[1].lives.ToString(), new Vector2(Settings.screenSize.X * 0.75f, Settings.screenSize.Y * 0.5f));
            spriteBatch.End();
            
            base.Draw(gameTime);
        }
    }
}
