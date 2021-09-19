using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Pong
{
    
    public struct Player
    {
        public Vector2 position;
        public Vector2 size;
        public Vector2 renderPosition;        
        public bool serving;
        public int score;

        public Vector2[] corners;

        public Player(Vector2 position, Vector2 size, bool serving)
        {
            this.position = position;
            this.renderPosition = position;
            this.size = size;
            this.serving = serving;
            this.score = 0;
            var tl = - size / 2;
            var br = size / 2;
            corners = new Vector2[] { tl,
                                 new Vector2(br.X, tl.Y),
                                 new Vector2(tl.X, br.Y),
                                 br };
        }        
    }
    public struct Ball
    {
        public Vector2 position;
        public Vector2 size;
        public Vector2 velocity;
        
        public Ball(Vector2 position, Vector2 size, Vector2 velocity)
        {
            this.position = position;
            this.size = size;
            this.velocity = velocity;
            
        }
    }

    public class PongGame : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        // objects
        private Player player1;
        private Player player2;
        private Ball ball;

        // prefs
        private Vector2 screenSize { get; } = new Vector2(1280, 720);
        private Vector2 playFieldMargin { get; } = new Vector2(0, 0);
        private float playerHOffset { get; } = 128;
        private Vector2 serveBallOffset { get; } = new Vector2(8, 0);

        private Vector2 playerSize { get; } = new Vector2(32,128);
        private Vector2 ballSize { get; } = new Vector2(32, 32);
        private float speedMultiplier { get; } = 1.03f;
        private float ballStartVelocity { get; } = 300;
        
        private float playerMovementSpeed = 500;

        // textures
        private Texture2D playerTexture;
        private Texture2D ballTexture;
        private SpriteFont font;

        // input variables
        private int player1MoveInput;
        private int player2MoveInput;
        private bool player1Serve;
        private bool player2Serve;

       
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

        

        private void HandleInput()
        {
            GamePadState gamepad1 = GamePad.GetState(PlayerIndex.One);
            GamePadState gamepad2 = GamePad.GetState(PlayerIndex.One);
            KeyboardState keyboard = Keyboard.GetState();

            if (gamepad1.Buttons.Back == ButtonState.Pressed || keyboard.IsKeyDown(Keys.Escape))
                Exit();

            player1MoveInput = 0;
            player2MoveInput = 0;

            if (gamepad1.DPad.Up == ButtonState.Pressed || keyboard.IsKeyDown(Keys.W))
                player1MoveInput -= 1;
            else if (gamepad1.DPad.Down == ButtonState.Pressed || keyboard.IsKeyDown(Keys.S))
                player1MoveInput += 1;

            if (gamepad2.DPad.Up == ButtonState.Pressed || keyboard.IsKeyDown(Keys.Up))
                player2MoveInput -= 1;
            else if (gamepad2.DPad.Down == ButtonState.Pressed || keyboard.IsKeyDown(Keys.Down))
                player2MoveInput += 1;

            player1Serve = gamepad1.Buttons.A == ButtonState.Pressed || keyboard.IsKeyDown(Keys.D);
            player2Serve = gamepad2.Buttons.A == ButtonState.Pressed || keyboard.IsKeyDown(Keys.Left);
        }

        private void MovePlayers(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float player1boundary = playerSize.Y / 2 + playFieldMargin.Y;
            float player2boundary = playerSize.Y / 2 + playFieldMargin.Y;

            player2.position.Y += player2MoveInput * playerMovementSpeed * dt;
            player1.position.Y += player1MoveInput * playerMovementSpeed * dt;

            player1.position.Y = MathHelper.Clamp(player1.position.Y, player1boundary, screenSize.Y - player1boundary);
            player2.position.Y = MathHelper.Clamp(player2.position.Y, player2boundary, screenSize.Y - player2boundary);

            player1.renderPosition = Utils.Lerp(player1.renderPosition, player1.position, 15 * dt);
            player2.renderPosition = Utils.Lerp(player2.renderPosition, player2.position, 15 * dt);
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
        private void MoveBall(GameTime gameTime)
        {
            
            if (player1.serving)
            {
                // use render position here so the ball doesn't lag behind
                ball.position = player1.renderPosition + serveBallOffset;
                ball.position.X += player1.size.X / 2 + ball.size.X / 2;
                if (player1Serve)
                {
                    player1.serving = false;
                    int dir = player1MoveInput == 0 ? 1 : player1MoveInput;
                    ball.velocity = new Vector2(ballStartVelocity, ballStartVelocity * dir);
                }
            }
            else if (player2.serving)
            {
                ball.position = player2.renderPosition - serveBallOffset;
                ball.position.X -= player2.size.X / 2 + ball.size.X / 2;
                if (player2Serve)
                {
                    player2.serving = false;
                    int dir = player2MoveInput == 0 ? -1 : player2MoveInput;
                    ball.velocity = new Vector2(-ballStartVelocity, ballStartVelocity * dir);
                }
            }
            else
            {
                Vector2 deltaPos = ball.velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
                Vector2 nextPos = ball.position + deltaPos;
                Vector2 hEdgeOffset = new Vector2(ball.size.X/2, 0);
                Vector2 vEdgeOffset = new Vector2(0, ball.size.Y/2);
                Vector2 playFieldTL = playFieldMargin;
                Vector2 playFieldBR = screenSize - playFieldMargin;
                Vector2 hit;

                if (Utils.LineIntersection(ball.position, nextPos - vEdgeOffset, playFieldTL, 
                    new Vector2(playFieldBR.X, playFieldTL.Y), out hit))
                {
                    deltaPos = ResolveCollision(ball.position - vEdgeOffset, hit, Vector2.UnitY, deltaPos);
                    nextPos = hit + vEdgeOffset;// two collisions may occur in one frame (player + wall)
                }
                else if (Utils.LineIntersection(ball.position, nextPos + vEdgeOffset,
                    new Vector2(playFieldTL.X, playFieldBR.Y), playFieldBR, out hit))
                {
                    deltaPos = ResolveCollision(ball.position + vEdgeOffset, hit, -Vector2.UnitY, deltaPos);
                    nextPos = hit - vEdgeOffset;
                }

                if (Utils.LineIntersection(ball.position, nextPos - hEdgeOffset,
                    player1.position + player1.corners[1] - vEdgeOffset,
                    player1.position + player1.corners[3] + vEdgeOffset, out hit))
                {
                    float collisionRange = player1.size.Y / 2 + vEdgeOffset.Y;
                    float playerBallHeightDiff = hit.Y - player1.position.Y;
                    float normalOffset = playerBallHeightDiff / collisionRange;
                    Vector2 n = Vector2.Normalize(new Vector2(1, normalOffset/2));
                    deltaPos = ResolveCollision(ball.position - hEdgeOffset, hit, n, deltaPos);
                    nextPos = hit + hEdgeOffset;
                    ball.velocity *= speedMultiplier;
                    playerMovementSpeed *= speedMultiplier;
                }
                else if (Utils.LineIntersection(ball.position, nextPos + hEdgeOffset,
                    player2.position + player2.corners[0] - vEdgeOffset,
                    player2.position + player2.corners[2] + vEdgeOffset, out hit))
                {
                    float collisionRange = player2.size.Y / 2 + vEdgeOffset.Y;
                    float playerBallHeightDiff = hit.Y - player2.position.Y;
                    float normalOffset = playerBallHeightDiff / collisionRange;
                    Vector2 n = Vector2.Normalize(new Vector2(-1, normalOffset/2));

                    deltaPos = ResolveCollision(ball.position + hEdgeOffset, hit, n, deltaPos);
                    nextPos = hit - hEdgeOffset;
                    ball.velocity *= speedMultiplier;
                    playerMovementSpeed *= speedMultiplier;
                }
                ball.position = nextPos;
                ball.velocity = Vector2.Normalize(deltaPos) * ball.velocity.Length();
            }
        }

        private void CheckMiss()
        {
            if (ball.position.X < 0)
            {
                // player 1 missed, player 2 gets point, player 1 gets service
                player2.score += 1;
                player1.serving = true;
            }
            else if (ball.position.X > screenSize.X)
            {
                // player 2 missed, player 1 gets point, player 2 gets service
                player1.score += 1;
                player2.serving = true;
            }
        }

        public PongGame()
        {
            
            graphics = new GraphicsDeviceManager(this);
            
            
            graphics.PreferredBackBufferWidth = (int)screenSize.X;
            graphics.PreferredBackBufferHeight = (int)screenSize.Y;
            graphics.ApplyChanges();

            player1 = new Player(new Vector2(playerHOffset, screenSize.Y/2),
                                    playerSize,
                                    true);
            player2 = new Player(new Vector2(screenSize.X - playerHOffset, screenSize.Y/2), 
                                    playerSize,
                                    false);
            ball = new Ball(new Vector2(screenSize.X / 2, screenSize.Y / 2),
                                    ballSize,
                                    Vector2.Zero);

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

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
            HandleInput();
            MovePlayers(gameTime);
            MoveBall(gameTime);
            CheckMiss();
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();
            DrawSpriteCentered(playerTexture, player1.renderPosition);
            DrawSpriteCentered(playerTexture, player2.renderPosition);
            DrawSpriteCentered(ballTexture, ball.position);

            DrawTextCentered(player1.score.ToString(), new Vector2(screenSize.X * 0.25f, screenSize.Y * 0.5f));
            DrawTextCentered(player2.score.ToString(), new Vector2(screenSize.X * 0.75f, screenSize.Y * 0.5f));
            spriteBatch.End();
            
            base.Draw(gameTime);
        }
    }
}
