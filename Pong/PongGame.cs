using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pong
{
    public enum GameState
    {
        Menu = 0,
        InGame = 1, 
        Serving = 2,
        GameOver = 3,
    }
    public class PongGame : Game
    {
        private GraphicsDeviceManager graphics;

        // objects
        private Player[] players;
        private Ball ball;

        private Box playField; // the bounds of the playing field
        private Line[] playFieldEdges; // playing field edges

        // textures            
        private Texture2D playerTexture;
        private Texture2D ballTexture;

        // fonts
        private SpriteFont scoreFont;
        private SpriteFont titleFont;
        private SpriteFont subtitleFont;

        // global data
        private GameState state;
        private int winningPlayer = 0;
        private float basePlayerSpeed = Settings.PlayerStartSpeed;
        public PongGame()
        {
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            IsFixedTimeStep = false;
            
            graphics = new GraphicsDeviceManager(this);
            
            graphics.PreferredBackBufferWidth = (int)Settings.screenSize.X;
            graphics.PreferredBackBufferHeight = (int)Settings.screenSize.Y;
            graphics.ApplyChanges();
            
            _ = new InputHandler();
            _ = new Renderer(GraphicsDevice);
            

            playField = new Box(Settings.screenSize / 2, Settings.screenSize - Settings.playFieldMargin);
            playFieldEdges = new[] { playField.Top, playField.Bottom, playField.Left, playField.Right };
            
            players = new[]{
                new Player(
                    0,
                    new Vector2(Settings.playerHOffset, Settings.screenSize.Y/2),
                    Settings.PlayerSize,
                    true
                ),
                new Player(
                    1,
                    new Vector2(Settings.screenSize.X - Settings.playerHOffset, Settings.screenSize.Y/2),
                    Settings.PlayerSize,
                    false
                )
            };

            ball = new Ball(new Vector2(Settings.screenSize.X / 2, Settings.screenSize.Y / 2),
                Settings.BallSize,
                new Vector2(Settings.BallStartSpeed, Settings.BallStartSpeed));
            
            state = GameState.Menu;
        }

        protected override void LoadContent()
        {
            Renderer.current.spriteBatch = new SpriteBatch(GraphicsDevice);
            ballTexture = Content.Load<Texture2D>("ball");
            playerTexture = Content.Load<Texture2D>("player");
            scoreFont = Content.Load<SpriteFont>("scoreFont");
            titleFont = Content.Load<SpriteFont>("titleFont");
            subtitleFont = Content.Load<SpriteFont>("subtitleFont");
        }
        private void MovePlayers(float dt)
        {
            for(int i = 0; i < Settings.PlayerCount; i++)
            {
                players[i].Move(InputHandler.current.playerMovementInput[i], basePlayerSpeed, dt);
            }
        }
        private void ServeBall()
        {
            // serving behaviour
            for (int i = 0; i < Settings.PlayerCount; i++)
            {
                var player = players[i];
                
                if (!player.serving) continue;
                
                // use render position here so the ball doesn't lag behind
                ball.transform.position = player.renderPosition + player.normal * Settings.BallServeOffset;
                ball.transform.position += player.normal * (player.transform.size.X / 2 + ball.transform.size.X / 2);
                
                if (!InputHandler.current.playerServeInput[i]) continue;
                
                player.serving = false;
                state = GameState.InGame;
                int dir = InputHandler.current.playerMovementInput[i] == 0 
                    ? 1 
                    : InputHandler.current.playerMovementInput[i];
                ball.velocity = player.normal * Settings.BallStartSpeed + new Vector2(0, Settings.BallStartSpeed * dir);
            }
        }
        private void MoveBall(float dt)
        {

            Vector2 deltaPos = ball.velocity * dt;
            if (deltaPos == Vector2.Zero)
            {
                return; // no need to check collision if the ball isn't moving
            }
               
            // repeat collision checks until there are no collisions
            bool collided;

            do
            {
                collided = false;

                Line[] collisionRays = ball.GetCollisionRays(deltaPos);

                Vector2 nextPos = ball.transform.position + deltaPos;
                Vector2 intersect;

                // a collision ray will always hit a player before hitting a wall (can be deduced from playfield layout)

                // Player Collision Check
                if (state != GameState.Menu)
                {
                    // todo: use collision class in some way
                    for (int i = 0; i < Settings.PlayerCount; i++)
                    {
                        var player = players[i];
                        if (!Utils.ClosestLineIntersection(collisionRays, player.GetCollisionEdges(), out intersect,
                            out int ia, out int ib, out float t)) continue;

                        Vector2 n;
                        switch (ib)
                        {
                            case 0:
                                float dst = (intersect.Y - player.transform.position.Y) / player.transform.size.Y;

                                n = Vector2.Normalize(player.normal + dst * Vector2.UnitY * 0.7f);
                                break;
                            default:
                                n = Vector2.Normalize(-deltaPos); // side collision: reverse travel direction
                                break;
                        }

                        deltaPos = Utils.ResolveCollision(collisionRays[ia].start, intersect, n, deltaPos);
                        nextPos = intersect + ball.transform.position - collisionRays[ia].start + n * 0.01f;
                        ball.velocity *= Settings.SpeedMultiplier;
                        basePlayerSpeed *= Settings.SpeedMultiplier;
                        collided = true;
                        break;
                    }
                }

                // Top Wall Collision Check
                // walls extend out to infinity so only one ray is needed here
                if (deltaPos.Y < 0)
                {
                    if (Utils.LineIntersection(playFieldEdges[0], collisionRays[0], out intersect))
                    {
                        var n = new Vector2(0, 1);
                        deltaPos = Utils.ResolveCollision(collisionRays[0].start, intersect, n, deltaPos);
                        nextPos = intersect + ball.transform.position - ball.transform.TL;
                        collided = true;
                    }
                }
                // Bottom Wall Collision Check
                else
                {
                    if (Utils.LineIntersection(playFieldEdges[1], collisionRays[2], out intersect))
                    {
                        var n = new Vector2(0, -1);
                        deltaPos = Utils.ResolveCollision(collisionRays[2].start, intersect, n, deltaPos);
                        nextPos = intersect + ball.transform.position - ball.transform.BL;
                        collided = true;
                    }
                }

                // Check for left and right wall in menu
                if (state == GameState.Menu)
                {
                    if (deltaPos.X < 0)
                    {
                        if (Utils.LineIntersection(playFieldEdges[2], collisionRays[0], out intersect))
                        {
                            var n = new Vector2(1, 0);
                            deltaPos = Utils.ResolveCollision(collisionRays[0].start, intersect, n, deltaPos);
                            nextPos = intersect + ball.transform.position - ball.transform.TL;
                            collided = true;
                        }
                    }
                    else
                    {
                        if (Utils.LineIntersection(playFieldEdges[3], collisionRays[1], out intersect))
                        {
                            var n = new Vector2(-1, 0);
                            deltaPos = Utils.ResolveCollision(collisionRays[1].start, intersect, n, deltaPos);
                            nextPos = intersect + ball.transform.position - ball.transform.TR;
                            collided = true;
                        }
                    }
                }

                // Update ball position
                ball.transform.position = nextPos;
            } while (collided);
            ball.velocity = Vector2.Normalize(deltaPos) * ball.velocity.Length();
        }
        private void CheckMiss()
        {
            if (ball.transform.BR.X < 0)
            {
                // player 1 missed
                players[0].lives -= 1;
                if (players[0].lives == 0)
                {
                    state = GameState.GameOver;
                    winningPlayer = 1;
                }
                else state = GameState.Serving;
                players[0].serving = true;
            }
            else if (ball.transform.TL.X > Settings.screenSize.X)
            {
                // player 2 missed
                players[1].lives -= 1;
                if (players[1].lives == 0)
                {
                    state = GameState.GameOver;
                    winningPlayer = 0;
                }
                else state = GameState.Serving;
                players[1].serving = true;
            }
        }
        
        protected override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            InputHandler.current.HandleInput();
            
            switch (state)
            {
                case GameState.Menu:
                    MoveBall(dt);
                    if (InputHandler.current.startGame)
                        state = GameState.Serving;                    
                    break;
                case GameState.Serving:
                    MovePlayers(dt);
                    ServeBall();
                    break;
                case GameState.InGame:
                    MovePlayers(dt);
                    MoveBall(dt);
                    CheckMiss();
                    break;
                case GameState.GameOver:
                    if (InputHandler.current.playerServeInput[winningPlayer])
                        state = GameState.Menu;
                    break;
            }            
            base.Update(gameTime);
        }

        #region Draw Code
        private void DrawScore()
        {
            Renderer.current.DrawTextCentered(scoreFont, players[0].lives.ToString(), new Vector2(Settings.screenSize.X * 0.25f, Settings.screenSize.Y * 0.5f), new Color(24, 144, 192));
            Renderer.current.DrawTextCentered(scoreFont, players[1].lives.ToString(), new Vector2(Settings.screenSize.X * 0.75f, Settings.screenSize.Y * 0.5f), new Color(24, 144, 192));
        }
        private void DrawPlayers()
        {
            foreach (Player player in players)
            {
                Color c = (player.lives == 0) ? Renderer.red : Renderer.playerColor;

                if (player.normal.X > 0)
                    Renderer.current.DrawSpriteCentered(playerTexture, player.renderPosition, c);
                else
                    Renderer.current.DrawSpriteCentered(playerTexture, player.renderPosition, c, true);
            }
        }
        private void DrawBall()
        {
            Renderer.current.DrawSpriteCentered(ballTexture, ball.transform.position, Renderer.ballColor);
        }
        private void DrawTitleScreen()
        {
            Vector2 center = Settings.screenSize / 2;
            var offset = new Vector2(0, 64);
            
            Renderer.current.DrawTextCentered(titleFont, 
                "pong.", 
                center - offset, 
                Renderer.playerColor);
            Renderer.current.DrawTextCentered(subtitleFont, 
                "feat. overacurrate collision detection & fancy graphics", 
                center + offset, 
                Renderer.scoreColor);
            Renderer.current.DrawTextCentered(subtitleFont, 
                "by Pepijn & Tigo", 
                center + 2*offset, 
                Renderer.scoreColor);
        }
        private void DrawGameOverScreen()
        {
            Vector2 center = Settings.screenSize / 2;
            var offset = new Vector2(0, 64);
            
            Renderer.current.DrawTextCentered(titleFont, 
                "Game Over", 
                center - offset, 
                Renderer.red);
            Renderer.current.DrawTextCentered(subtitleFont, 
                "press Space / A button to continue", 
                center + offset, 
                Renderer.scoreColor);
        }
        private void DrawServingText()
        {
            Vector2 center = Settings.screenSize / 2;
            var offset = new Vector2(0, 128);
            
            Renderer.current.DrawTextCentered(subtitleFont, 
                "press D / LeftArrow / A button to serve", 
                center - offset, 
                Renderer.scoreColor);
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(16,32,48));
            Renderer.current.Begin();
            switch (state)
            {
                case GameState.Menu:
                    DrawTitleScreen();
                    DrawBall();
                    break;               
                case GameState.InGame:
                    DrawScore();
                    DrawBall();
                    DrawPlayers();
                    break;
                case GameState.Serving:
                    DrawScore();
                    DrawServingText();
                    DrawBall();
                    DrawPlayers();
                    break;

                case GameState.GameOver:
                    DrawGameOverScreen();
                    DrawPlayers();
                    break;
            }
            Renderer.current.End();
            base.Draw(gameTime);
        }
        #endregion

    }
}
