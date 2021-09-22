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
        private GameState gameState; // current game state
        private int winningPlayer = 0; // index of winning player 
        private float basePlayerSpeed = Settings.PlayerStartSpeed; // current player speed without modifiers
        public PongGame() 
        {
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            IsFixedTimeStep = false; // I like to play pong in butter smooth 165Hz
            
            graphics = new GraphicsDeviceManager(this);
            
            // set the window size
            graphics.PreferredBackBufferWidth = (int)Settings.screenSize.X;
            graphics.PreferredBackBufferHeight = (int)Settings.screenSize.Y;
            graphics.ApplyChanges(); 
            
            // Initialize InputHandler and Renderer. These are both singletons so we 
            // can discard their instances.
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
            
            gameState = GameState.Menu;
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
                Player player = players[i];
                player.Move(InputHandler.current.playerMovementInput[i], basePlayerSpeed, dt);
                // todo: fix player clipping into ball
            }
        }
        private void ServeBall()
        {
            // serving behaviour
            for (int i = 0; i < Settings.PlayerCount; i++)
            {
                Player player = players[i];
                Vector2 n = Box.normals[player.frontEdge];
                if (!player.serving) continue;
                
                // use render position here so the ball doesn't lag behind
                ball.transform.position = player.renderPosition + n * Settings.BallServeOffset;
                ball.transform.position += n * (player.transform.size.X / 2 + ball.transform.size.X / 2);
                
                if (!InputHandler.current.playerServeInput[i]) continue;
                
                player.serving = false;
                gameState = GameState.InGame;
                int dir = InputHandler.current.playerMovementInput[i] == 0 
                    ? 1 
                    : InputHandler.current.playerMovementInput[i];
                ball.velocity = n * Settings.BallStartSpeed + new Vector2(0, Settings.BallStartSpeed * dir);
            }
        }
        private void MoveBall(float dt)
        {
            Vector2 deltaPos = ball.velocity * dt; // delta-position of current frame
            if (deltaPos == Vector2.Zero) return; // no need to check collision if the ball isn't moving

            bool collided;
            // repeat collision checks until there are no collisions
            do
            {
                collided = false; // assume there are no collisions
 
                // get the collision rays of the ball (rays starting at each corner and travelling deltaPos)

                // if there are no collisions, the next position of the ball will be: position + deltaPos
                Vector2 nextPos = ball.transform.position + deltaPos;

                Collision collision;
                // Player Collision Check --------------
                // The walls extend out to infinity, so if a collision ray happens to hit both a player and a wall,
                // it will always hit the player first.
                if (gameState != GameState.Menu) // Ignore players in the menu
                {
                    for (int i = 0; i < Settings.PlayerCount; i++)
                    {
                        Player player = players[i];
                        // Check for collisions between the player and the ball
                        if (!Utils.CheckBoxCollision(ball.transform,player.transform, deltaPos, out collision)) continue;
            
                        Vector2 n;
                        
                        if (collision.hitEdgeIndex == player.frontEdge)
                        {
                            n = Box.normals[player.frontEdge];
                            var tangent = new Vector2(n.Y, -n.X);
                            
                            float dst = (collision.hit.Y - player.transform.position.Y) / player.transform.size.Y;
                            n += tangent * dst * 0.5f;
                        }
                        else
                            n = Vector2.Normalize(-deltaPos); // side collision: reverse travel direction

                        deltaPos = Utils.ResolveCollision(ball.transform.position, collision.hit, n, deltaPos);
                        nextPos = collision.hit + n * 0.01f;
                        
                        ball.velocity *= Settings.SpeedMultiplier;
                        basePlayerSpeed *= Settings.SpeedMultiplier;
                        
                        collided = true;
                        break;
                    }
                }
                // Wall collision check
                // check for any collisions between the ball and the playfield rect
                if (Utils.CheckBoxCollision(ball.transform, playField, deltaPos, out collision))
                {
                    // edges with index 2 and 3 are the left and right edges, we only care about those in the
                    // menu screen (otherwise ball will bounce off of left and right walls during the game)
                    if (collision.hitEdgeIndex <= 1 || gameState == GameState.Menu)
                    {
                        Vector2 n = -Box.normals[collision.hitEdgeIndex];
                        deltaPos = Utils.ResolveCollision(ball.transform.position, collision.hit,
                            n, deltaPos);
                        nextPos = collision.hit + n * 0.01f;
                        collided = true;
                    }
                    // we invert the normals because the ball is inside the box
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
                    gameState = GameState.GameOver;
                    winningPlayer = 1;
                }
                else gameState = GameState.Serving;
                players[0].serving = true;
                basePlayerSpeed = Settings.PlayerStartSpeed;
            }
            else if (ball.transform.TL.X > Settings.screenSize.X)
            {
                // player 2 missed
                players[1].lives -= 1;
                if (players[1].lives == 0)
                {
                    gameState = GameState.GameOver;
                    winningPlayer = 0;
                }
                else gameState = GameState.Serving;
                players[1].serving = true;
                basePlayerSpeed = Settings.PlayerStartSpeed;
            }
        }
        
        protected override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            InputHandler.current.HandleInput();
            
            switch (gameState)
            {
                case GameState.Menu:
                    MoveBall(dt);
                    if (InputHandler.current.startGame)
                        gameState = GameState.Serving;                    
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
                        gameState = GameState.Menu;
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

                switch (player.id)
                {
                    case 0:
                        Renderer.current.DrawSpriteCentered(playerTexture, player.renderPosition, c);
                        break;
                    case 1:
                        Renderer.current.DrawSpriteCentered(playerTexture, player.renderPosition, c, true);
                        break;
                }
            }
        }
        private void DrawBall()
        {
            Renderer.current.DrawSpriteCentered(ballTexture, ball.transform.position, Renderer.ballColor);
        }
        private void DrawTitleScreen()
        {
            Vector2 center = Settings.screenSize / 2;
            var verticalOffset = new Vector2(0, 96);
            var horizontalOffset = new Vector2(300,0);
            Renderer.current.DrawTextCentered(titleFont, 
                "pong.", 
                center - verticalOffset, 
                Renderer.ballColor);
            
            Renderer.current.DrawTextCentered(subtitleFont,
                "Press Space / Start to begin",
                center + 0.3f*verticalOffset,
                Renderer.playerColor);
                
            Renderer.current.DrawTextCentered(subtitleFont, 
                "PLAYER 1 CONTROLS:", 
                center + verticalOffset - horizontalOffset, 
                Renderer.scoreColor);
            Renderer.current.DrawTextCentered(subtitleFont,
                "W/S to move, D to serve",
                center + 1.5f*verticalOffset - horizontalOffset,
                Renderer.scoreColor);
            Renderer.current.DrawTextCentered(subtitleFont, 
                "PLAYER 2 CONTROLS:",
                center + verticalOffset + horizontalOffset, 
                Renderer.scoreColor);
            Renderer.current.DrawTextCentered(subtitleFont,
                "Up/Down to move, Left to serve",
                center + 1.5f*verticalOffset + horizontalOffset,
                Renderer.scoreColor);
            Renderer.current.DrawTextCentered(subtitleFont,
                "CONTROLLER BINDINGS:", center + 2.5f*verticalOffset,
                Renderer.scoreColor);
            Renderer.current.DrawTextCentered(subtitleFont,
                "DPad to move, A (XBOX) / X (PSN) to serve", center + 3f*verticalOffset,
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
            GraphicsDevice.Clear(new Color(8,20,32));
            Renderer.current.Begin();
            switch (gameState)
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
