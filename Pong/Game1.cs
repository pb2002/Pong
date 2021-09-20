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
        private Player player1;
        private Player player2;
        private Ball ball;

        // prefs
        
        private float playerMovementSpeed = Settings.PlayerStartSpeed;
        private Line topWall;
        private Line bottomWall;
        // textures            
        private Texture2D playerTexture;
        private Texture2D ballTexture;
        private SpriteFont font;

        // input variables


       
        private void DrawSpriteCentered(Texture2D sprite, Vector2 position)
        {
            Vector2 offset = new Vector2(sprite.Width / 2, sprite.Height / 2);
            spriteBatch.Draw(sprite, position - offset, Color.White);
        }

        private void DrawTextCentered(string text, Vector2 position)
        {
            Vector2 textMiddlePoint = font.MeasureString(text) / 2;
            spriteBatch.DrawString(font, text, position, Color.White, 0, textMiddlePoint, 1.0f, SpriteEffects.None, 0.5f);
        }

        
        // TODO: Migrate to InputHandler singleton class
        private void HandleInput()
        {
            GamePadState gamepad1 = GamePad.GetState(PlayerIndex.One);
            GamePadState gamepad2 = GamePad.GetState(PlayerIndex.One);
            KeyboardState keyboard = Keyboard.GetState();

            if (gamepad1.Buttons.Back == ButtonState.Pressed || keyboard.IsKeyDown(Keys.Escape))
                Exit();

            InputHandler.current.player1MoveInput = 0;
            InputHandler.current.player2MoveInput = 0;

            if (gamepad1.DPad.Up == ButtonState.Pressed || keyboard.IsKeyDown(Keys.W))
                InputHandler.current.player1MoveInput -= 1;
            else if (gamepad1.DPad.Down == ButtonState.Pressed || keyboard.IsKeyDown(Keys.S))
                InputHandler.current.player1MoveInput += 1;

            if (gamepad2.DPad.Up == ButtonState.Pressed || keyboard.IsKeyDown(Keys.Up))
                InputHandler.current.player2MoveInput -= 1;
            else if (gamepad2.DPad.Down == ButtonState.Pressed || keyboard.IsKeyDown(Keys.Down))
                InputHandler.current.player2MoveInput += 1;

            InputHandler.current.player1ServeInput = gamepad1.Buttons.A == ButtonState.Pressed || keyboard.IsKeyDown(Keys.D);
            InputHandler.current.player2ServeInput = gamepad2.Buttons.A == ButtonState.Pressed || keyboard.IsKeyDown(Keys.Left);
        }

        private void MovePlayers(float dt)
        {
            player1.Move(InputHandler.current.player1MoveInput, dt);          
            player2.Move(InputHandler.current.player2MoveInput, dt);
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
            
            if (player1.serving)
            {
                // use render position here so the ball doesn't lag behind
                ball.transform.position = player1.renderPosition + Settings.BallServeOffset;
                ball.transform.position += (player1.transform.size.X / 2 + ball.transform.size.X / 2) * Vector2.UnitX;
                if (InputHandler.current.player1ServeInput)
                {
                    player1.serving = false;
                    int dir = InputHandler.current.player1MoveInput == 0 ? 1 : InputHandler.current.player1MoveInput;
                    ball.velocity = new Vector2(Settings.BallStartSpeed, Settings.BallStartSpeed * dir);
                }
            }
            else if (player2.serving)
            {

                ball.transform.position = player2.renderPosition - Settings.BallServeOffset;
                ball.transform.position -= (player2.transform.size.X / 2 + ball.transform.size.X / 2) * Vector2.UnitX;
                if (InputHandler.current.player2ServeInput)
                {
                    player2.serving = false;
                    int dir = InputHandler.current.player2MoveInput == 0 ? -1 : InputHandler.current.player2MoveInput;
                    ball.velocity = new Vector2(-Settings.BallStartSpeed, Settings.BallStartSpeed * dir);
                }
            }
            else
            {
                Vector2 deltaPos = ball.velocity * dt;

                bool collided = true;
                // repeat collision checks until there are no collisions
                while (collided)
                {
                    collided = false;
                    
                    Line[] collisionRays = ball.GetCollisionRays(deltaPos);
                    
                    Line[] player1Edges = new Line[] { player1.transform.Right, player1.transform.Top, player1.transform.Bottom };

                    Line[] player2Edges = new Line[] { player2.transform.Left, player2.transform.Top, player2.transform.Bottom };


                    Vector2 nextPos = ball.transform.position + deltaPos;
                    Vector2 intersect;
                    
                    // a collision ray will always hit a player before hitting a wall (can be deduced from playfield layout)

                    // Player 1 Collision Check
                    // Check for intersections with the right player edge

                    if (deltaPos.X < 0)
                    {
                        // Check for both rays on the left side of the ball (index 0 and 2)                        
                        if (Utils.ClosestLineIntersection(collisionRays, player1Edges, out intersect, out int ia, out int ib, out float t))
                        {
                            Vector2 n;
                            switch (ib)
                            {
                                case 0:
                                    n = new Vector2(1, 0); // normal vector of right side (player1 is facing to the right)
                                    break;
                                default:
                                    n = Vector2.Normalize(-deltaPos); // side collision: reverse travel direction
                                    break;
                            }
                            
                            deltaPos = ResolveCollision(collisionRays[ia].start, intersect, n, deltaPos);
                            nextPos = intersect + ball.transform.position - collisionRays[ia].start;
                            collided = true;
                        }
                    }
                    // Player 2 Collision Check
                    // Check for intersections with the left player edge
                    else {

                        // Check for both rays on the right side of the ball (index 1 and 3)
                        if (Utils.ClosestLineIntersection(collisionRays, player2Edges, out intersect, out int ia, out int ib, out float t))
                        {
                            Vector2 n;
                            switch (ib)
                            {
                                case 0:
                                    n = new Vector2(-1, 0); // normal vector of right side (player1 is facing to the right)
                                    break;
                                default:
                                    n = Vector2.Normalize(-deltaPos); // side collision: reverse travel direction
                                    break;
                            }

                            deltaPos = ResolveCollision(collisionRays[ia].start, intersect, n, deltaPos);
                            nextPos = intersect + ball.transform.position - collisionRays[ia].start;
                            collided = true;
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
                // player 1 missed, player 2 gets point, player 1 gets service
                player2.lives += 1;
                player1.serving = true;
            }
            else if (ball.transform.position.X > Settings.screenSize.X)
            {
                // player 2 missed, player 1 gets point, player 2 gets service
                player1.lives += 1;
                player2.serving = true;
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
            player1 = new Player(new Vector2(Settings.playerHOffset, Settings.screenSize.Y/2),
                                    Settings.PlayerSize,
                                    true,
                                    playerMovementSpeed);
            player2 = new Player(new Vector2(Settings.screenSize.X - Settings.playerHOffset, Settings.screenSize.Y/2),
                                    Settings.PlayerSize,
                                    false,
                                    playerMovementSpeed);

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
            HandleInput();
            MovePlayers(dt);
            MoveBall(dt);
            CheckMiss();
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();
            DrawSpriteCentered(playerTexture, player1.renderPosition);
            DrawSpriteCentered(playerTexture, player2.renderPosition);
            DrawSpriteCentered(ballTexture, ball.transform.position);

            DrawTextCentered(player1.lives.ToString(), new Vector2(Settings.screenSize.X * 0.25f, Settings.screenSize.Y * 0.5f));
            DrawTextCentered(player2.lives.ToString(), new Vector2(Settings.screenSize.X * 0.75f, Settings.screenSize.Y * 0.5f));
            spriteBatch.End();
            
            base.Draw(gameTime);
        }
    }
}
