namespace Pokedex;

public partial class Form1 : Form
{
    public int tileSize = 60; // Size of each tile in pixels
    private List<Tile> tiles;
    private Player player;
    private System.Windows.Forms.Timer gameTimer;
    private const int TARGET_FPS = 60; // Set your desired frame rate here
    private const int TIMER_INTERVAL = 1000 / TARGET_FPS; // ~16ms for 60 FPS

    private const bool DEBUG_MODE = true; // Set to true for debugging features

    // Frame rate monitoring
    private DateTime lastFrameTime = DateTime.Now;
    private int frameCount = 0;
    private double currentFPS = 0;
    
    // Input handling
    private bool needsRedraw = false;
    
    // Performance optimization
    private bool gameStarted = false;

    // Camera offset
    private float cameraOffsetX = 0;
    private float cameraOffsetY = 0;

    public Form1()
    {
        InitializeComponent();
        InitializeGame();
        SetupForm();
        InitializeGameTimer();
        gameStarted = true;
    }

    private void InitializeGameTimer()
    {
        gameTimer = new System.Windows.Forms.Timer();
        gameTimer.Interval = TIMER_INTERVAL;
        gameTimer.Tick += GameTimer_Tick;
        gameTimer.Start();
    }

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
            this.Text = $"Pokedex Game - FPS: {currentFPS:F1} - Keys: {player.GetKeyBuffer()}";
        }
    }

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
                if(!isWalkable)
                {
                    color = Color.Gray; // Non-walkable tiles are gray
                }

                tiles.Add(new Tile(x * tileSize, y * tileSize, color, isWalkable, speed));
            }
        }

        player = new Player(0, 0, tileSize); // Start player at the top-left corner
        needsRedraw = true; // Initial draw needed
    }

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
            using (Brush brush = new SolidBrush(tile.TileColor))
            {
                g.FillRectangle(brush, tile.X, tile.Y, tileSize, tileSize);
            }
        }

        // Draw player using actual position for smooth movement
        using (Brush playerBrush = new SolidBrush(Color.Blue))
        {
            g.FillRectangle(playerBrush, player.GetVisualX(), player.GetVisualY(), player.Size, player.Size);
        }

        // Reset transform
        g.ResetTransform();
    }

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
