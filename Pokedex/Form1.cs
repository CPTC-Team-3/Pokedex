namespace Pokedex;

/// <summary>
/// Main form class that handles the game window, rendering, and input processing
/// </summary>
public partial class Form1 : Form
{
    /// <summary>
    /// Size of each tile in pixels
    /// </summary>
    public int tileSize = 60;

    /// <summary>
    /// List of all tiles in the game world
    /// </summary>
    private List<Tile> tiles;

    /// <summary>
    /// The player instance controlled by user input
    /// </summary>
    private Player player;

    /// <summary>
    /// Timer that controls the game loop and update frequency
    /// </summary>
    private System.Windows.Forms.Timer gameTimer;

    /// <summary>
    /// Target frames per second for the game
    /// </summary>
    private const int TARGET_FPS = 60;

    /// <summary>
    /// Interval between timer ticks in milliseconds (calculated from TARGET_FPS)
    /// </summary>
    private const int TIMER_INTERVAL = 1000 / TARGET_FPS;

    /// <summary>
    /// Flag to enable debug features and information display
    /// </summary>
    private const bool DEBUG_MODE = true;

    /// <summary>
    /// Timestamp of the last frame for FPS calculation
    /// </summary>
    private DateTime lastFrameTime = DateTime.Now;

    /// <summary>
    /// Counter for frames rendered in the current second
    /// </summary>
    private int frameCount = 0;

    /// <summary>
    /// Current calculated frames per second
    /// </summary>
    private double currentFPS = 0;

    /// <summary>
    /// Flag indicating whether the game needs to redraw the screen
    /// </summary>
    private bool needsRedraw = false;

    /// <summary>
    /// Flag indicating whether the game has been initialized and started
    /// </summary>
    private bool gameStarted = false;

    /// <summary>
    /// Camera offset in the X direction for centering the view on the player
    /// </summary>
    private float cameraOffsetX = 0;

    /// <summary>
    /// Camera offset in the Y direction for centering the view on the player
    /// </summary>
    private float cameraOffsetY = 0;

    /// <summary>
    /// Initializes a new instance of the Form1 class and starts the game
    /// </summary>
    public Form1()
    {
        InitializeComponent();
        InitializeGame();
        SetupForm();
        InitializeGameTimer();

        this.MaximizeBox = false;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;

        // Try to load textures from the textures folder in the application lib folder.
        string texturesPath = Path.Combine(Application.StartupPath, "../../../lib/textures");
        LoadTextures(texturesPath);

        gameStarted = true;
        
        // Handle form closing to properly clean up resources and exit application
        this.FormClosing += Form1_FormClosing;
    }

    /// <summary>
    /// Handles the form closing event to properly clean up resources and exit the application
    /// </summary>
    private void Form1_FormClosing(object sender, FormClosingEventArgs e)
    {
        // Stop the game timer to prevent it from continuing after form closes
        if (gameTimer != null)
        {
            gameTimer.Stop();
            gameTimer.Dispose();
        }

        // Exit the entire application when the game form closes
        Application.Exit();
    }

    /// <summary>
    /// Initializes and starts the game timer with the specified interval
    /// </summary>
    private void InitializeGameTimer()
    {
        gameTimer = new System.Windows.Forms.Timer();
        gameTimer.Interval = TIMER_INTERVAL;
        gameTimer.Tick += GameTimer_Tick;
        gameTimer.Start();
    }

    /// <summary>
    /// Event handler for the game timer tick. Updates game state and handles rendering
    /// </summary>
    /// <param name="sender">The source of the event</param>
    /// <param name="e">Event arguments</param>
    private void GameTimer_Tick(object sender, EventArgs e)
    {
        // Calculate FPS for monitoring
        CalculateFPS();

        // Update player movement
        if (player.UpdateMovement(tiles))
        {
            needsRedraw = true;
        }

        // Update camera position
        UpdateCamera();

        // Only redraw if something has changed
        if (needsRedraw)
        {
            Invalidate();
            needsRedraw = false;
        }

        // Update debug info if needed
        if (DEBUG_MODE)
        {
            string spriteInfo = player.HasSpriteSheet ?
                $" - Dir: {player.CurrentDirection} - Frame: {player.GetCurrentFrame()}" :
                " - No Sprite";
            this.Text = $"Pokedex Game - FPS: {currentFPS:F1} - Keys: {player.GetKeyBuffer()}{spriteInfo}";
        }
    }

    /// <summary>
    /// Updates the camera position to center on the player
    /// </summary>
    private void UpdateCamera()
    {
        // Calculate the center of the screen
        float screenCenterX = ClientSize.Width / 2f;
        float screenCenterY = ClientSize.Height / 2f;

        // Calculate where the camera should be to center on the player
        cameraOffsetX = screenCenterX - player.GetVisualX() - (tileSize / 2f);
        cameraOffsetY = screenCenterY - player.GetVisualY() - (tileSize / 2f);

        needsRedraw = true;
    }

    /// <summary>
    /// Calculates and updates the current FPS value
    /// </summary>
    private void CalculateFPS()
    {
        frameCount++;
        var now = DateTime.Now;
        var elapsed = (now - lastFrameTime).TotalSeconds;

        if (elapsed >= 1.0) // Update FPS display every second
        {
            currentFPS = frameCount / elapsed;
            frameCount = 0;
            lastFrameTime = now;
        }
    }

    /// <summary>
    /// Sets up the form properties and enables necessary styles for game rendering
    /// </summary>
    private void SetupForm()
    {
        // Enable the form to receive key events
        this.KeyPreview = true;
        this.Focus();

        // Optional: Set form properties for better visibility
        this.Text = "Pokedex Game";
        this.WindowState = FormWindowState.Maximized;
        this.StartPosition = FormStartPosition.CenterScreen;

        // Enable double buffering to reduce flicker
        this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.DoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
    }

    /// <summary>
    /// Initializes the game world by creating tiles and the player
    /// </summary>
    private void InitializeGame()
    {
        tiles = new List<Tile>();
        int mapX = 20, mapY = 20;

        // Create a x by y grid of tiles - alternating walkable tiles
        for (int x = 0; x < mapX; x++)
        {
            for (int y = 0; y < mapY; y++)
            {
                bool isSand = (x + y) % 10 >= 3; // Example logic for sand tiles
                Color color = isSand ? Color.SandyBrown : Color.Green;
                float speed = isSand ? 0.5f : 1.0f; // Sand tiles are slower

                bool isWalkable = (x + y) % 15 != 0 || (x + y) % 5 <= 1; // Example logic for walkable tiles
                if (!isWalkable)
                {
                    color = Color.Gray; // Non-walkable tiles are gray
                }

                Tile tile = new Tile(x * tileSize, y * tileSize, color, isWalkable, speed);

                // Try to load tile textures (you can add your texture files here)
                // Example: tile.SetTexture($"Textures/Tiles/{(isSand ? "sand" : "grass")}.png");

                tiles.Add(tile);
            }
        }

        player = new Player(0, 0, tileSize); // Start player at the top-left corner

        // Try to load player sprite sheet (you can add your sprite sheet file here)
        // Example: player.SetSpriteSheet("Textures/Player/player_spritesheet.png", 32, 32);

        needsRedraw = true; // Initial draw needed
    }

    /// <summary>
    /// Loads textures for tiles and player from the specified directory.
    /// Call this method after InitializeGame() to load your texture files.
    /// </summary>
    /// <param name="texturesDirectory">Path to the directory containing texture files</param>
    public void LoadTextures(string texturesDirectory)
    {
        if (!Directory.Exists(texturesDirectory))
        {
            return;
        }

        // Load player sprite sheet
        string playerSpritePath = Path.Combine(texturesDirectory, "Player.png");
        if (File.Exists(playerSpritePath))
        {
            player.SetSpriteSheet(playerSpritePath, 32, 32);
        }


        // Example: Load different textures for different tile types
        string grassTexturePath = Path.Combine(texturesDirectory, "Grass.png");
        string sandTexturePath = Path.Combine(texturesDirectory, "Sand.png");
        string rockTexturePath = Path.Combine(texturesDirectory, "Rock.png");

        foreach (var tile in tiles)
        {
            if (tile.TileColor == Color.Green && File.Exists(grassTexturePath))
            {
                tile.SetTexture(grassTexturePath);
            }
            else if (tile.TileColor == Color.SandyBrown && File.Exists(sandTexturePath))
            {
                tile.SetTexture(sandTexturePath);
            }
            else if (tile.TileColor == Color.Gray && File.Exists(rockTexturePath))
            {
                tile.SetTexture(rockTexturePath);
            }
        }


        needsRedraw = true;
    }

    /// <summary>
    /// Handles the rendering of the game world and player
    /// </summary>
    /// <param name="e">Paint event arguments containing the graphics context</param>
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        Graphics g = e.Graphics;

        // Optimize graphics rendering
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

        // Apply camera transform
        g.TranslateTransform(cameraOffsetX, cameraOffsetY);

        // Draw tiles
        foreach (var tile in tiles)
        {
            if (tile.HasTexture)
            {
                // Draw tile texture
                g.DrawImage(tile.Texture!, tile.X, tile.Y, tileSize, tileSize);
            }
            else
            {
                // Fall back to color rendering
                using (Brush brush = new SolidBrush(tile.TileColor))
                {
                    g.FillRectangle(brush, tile.X, tile.Y, tileSize, tileSize);
                }
            }
        }

        // Draw player
        if (player.HasSpriteSheet)
        {
            // Draw player using sprite sheet animation
            Rectangle sourceRect = player.GetCurrentSpriteFrame();
            RectangleF destRect = new RectangleF(player.GetVisualX(), player.GetVisualY(), player.Size, player.Size);

            if (player.ShouldFlipHorizontally)
            {
                // Save the current graphics state
                var state = g.Save();

                // Translate to the center of the sprite, flip horizontally, then translate back
                g.TranslateTransform(destRect.X + destRect.Width / 2, destRect.Y + destRect.Height / 2);
                g.ScaleTransform(-1, 1);
                g.TranslateTransform(-destRect.Width / 2, -destRect.Height / 2);

                // Draw the sprite
                g.DrawImage(player.SpriteSheet!, new RectangleF(0, 0, destRect.Width, destRect.Height), sourceRect, GraphicsUnit.Pixel);

                // Restore the graphics state
                g.Restore(state);
            }
            else
            {
                // Draw normally
                g.DrawImage(player.SpriteSheet!, destRect, sourceRect, GraphicsUnit.Pixel);
            }
        }
        else
        {
            // Fall back to color rendering for player
            using (Brush playerBrush = new SolidBrush(Color.Blue))
            {
                g.FillRectangle(playerBrush, player.GetVisualX(), player.GetVisualY(), player.Size, player.Size);
            }
        }

        // Reset transform
        g.ResetTransform();
    }

    /// <summary>
    /// Handles key press events for player movement
    /// </summary>
    /// <param name="e">Key event arguments containing the pressed key information</param>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        char? keyChar = e.KeyCode switch
        {
            Keys.W or Keys.Up => 'W',
            Keys.A or Keys.Left => 'A',
            Keys.S or Keys.Down => 'S',
            Keys.D or Keys.Right => 'D',
            Keys.Escape => null,
            _ => null
        };

        if (keyChar.HasValue)
        {
            player.AddKeyToBuffer(keyChar.Value);
        }
        else if (e.KeyCode == Keys.Escape)
        {
            this.Close();
        }
    }

    /// <summary>
    /// Handles key release events for player movement
    /// </summary>
    /// <param name="e">Key event arguments containing the released key information</param>
    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);

        char? keyChar = e.KeyCode switch
        {
            Keys.W or Keys.Up => 'W',
            Keys.A or Keys.Left => 'A',
            Keys.S or Keys.Down => 'S',
            Keys.D or Keys.Right => 'D',
            _ => null
        };

        if (keyChar.HasValue)
        {
            player.RemoveKeyFromBuffer(keyChar.Value);
        }
    }

    /// <summary>
    /// Gets the current frame rate of the game
    /// </summary>
    public double GetCurrentFPS()
    {
        return currentFPS;
    }

    /// <summary>
    /// Allows changing the target frame rate at runtime
    /// </summary>
    /// <param name="newFPS">New target FPS (between 1 and 120)</param>
    public void SetTargetFPS(int newFPS)
    {
        if (newFPS < 1 || newFPS > 120)
            return;

        gameTimer.Stop();
        gameTimer.Interval = 1000 / newFPS;
        gameTimer.Start();
    }
}
