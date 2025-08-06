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
    private DateTime lastKeyPress = DateTime.MinValue;
    private const int KEY_COOLDOWN_MS = 150; // Minimum time between key presses
    
    // Performance optimization
    private bool gameStarted = false;

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
        
        // Only redraw if something has changed
        if (needsRedraw)
        {
            Invalidate();
            needsRedraw = false;
        }
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
            
            // Update window title with FPS (optional - for debugging)
            if (gameStarted && DEBUG_MODE)
            {
                this.Text = $"Pokedex Game - FPS: {currentFPS:F1}";
            }
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
                bool isWalkable = (x + y) % 10 != 0; // Example logic for walkability
                Color color = isWalkable ? Color.Green : Color.Gray;
                tiles.Add(new Tile(x * tileSize, y * tileSize, color, isWalkable));
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

        // Draw tiles
        foreach (var tile in tiles)
        {
            using (Brush brush = new SolidBrush(tile.TileColor))
            {
                g.FillRectangle(brush, tile.X, tile.Y, tileSize, tileSize);
            }
        }

        // Draw player
        using (Brush playerBrush = new SolidBrush(Color.Blue))
        {
            g.FillRectangle(playerBrush, player.X, player.Y, player.Size, player.Size);
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        
        // Implement key cooldown to prevent rapid movement
        var now = DateTime.Now;
        if ((now - lastKeyPress).TotalMilliseconds < KEY_COOLDOWN_MS)
        {
            return;
        }

        int deltaX = 0, deltaY = 0;

        switch (e.KeyCode)
        {
            case Keys.W:
            case Keys.Up:
                deltaY = -1;
                break;
            case Keys.S:
            case Keys.Down:
                deltaY = 1;
                break;
            case Keys.A:
            case Keys.Left:
                deltaX = -1;
                break;
            case Keys.D:
            case Keys.Right:
                deltaX = 1;
                break;
            case Keys.Escape:
                // Optional: Allow ESC to quit
                this.Close();
                return;
        }

        if (deltaX != 0 || deltaY != 0)
        {
            var oldX = player.X;
            var oldY = player.Y;
            
            player.Move(deltaX, deltaY, tiles);
            
            // Only flag for redraw if player actually moved
            if (player.X != oldX || player.Y != oldY)
            {
                needsRedraw = true;
                lastKeyPress = now;
            }
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
