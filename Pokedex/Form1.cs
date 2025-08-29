using Microsoft.Data.SqlClient;

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
    /// The currently selected user playing the game
    /// </summary>
    private User? currentUser;

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

    // Wild Pokemon Encounter System
    /// <summary>
    /// Chance of encountering a wild Pokemon on each step in a wild zone (5%)
    /// </summary>
    private const float ENCOUNTER_CHANCE = 0.05f;

    /// <summary>
    /// Random number generator for Pokemon encounters
    /// </summary>
    private Random encounterRandom = new Random();

    /// <summary>
    /// Flag indicating if a wild Pokemon encounter is currently happening
    /// </summary>
    private bool inWildEncounter = false;

    /// <summary>
    /// Current phase of the encounter transition
    /// </summary>
    private EncounterPhase encounterPhase = EncounterPhase.None;

    /// <summary>
    /// Timer for encounter transition effects
    /// </summary>
    private float encounterTimer = 0f;

    /// <summary>
    /// Duration of the fade to white effect in seconds
    /// </summary>
    private const float FADE_DURATION = 1.0f;

    /// <summary>
    /// Duration of the Pokeball growth animation in seconds
    /// </summary>
    private const float POKEBALL_GROW_DURATION = 1.0f;

    /// <summary>
    /// Duration to wait at full size before shrinking in seconds
    /// </summary>
    private const float POKEBALL_HOLD_DURATION = 0.8f;

    /// <summary>
    /// Duration of the Pokeball shrinking animation in seconds
    /// </summary>
    private const float POKEBALL_SHRINK_DURATION = 0.5f;

    /// <summary>
    /// Point during fade when Pokeball starts appearing
    /// </summary>
    private const float POKEBALL_START_THRESHOLD = 0.15f;

    /// <summary>
    /// Current alpha value for the white fade overlay
    /// </summary>
    private float fadeAlpha = 0f;

    /// <summary>
    /// Current scale factor for the Pokeball image
    /// </summary>
    private float pokeballScale = 0f;

    /// <summary>
    /// Target scale for the Pokeball
    /// </summary>
    private float pokeballTargetScale = 0.28f;

    /// <summary>
    /// Pokeball image for encounter transitions
    /// </summary>
    private Image? pokeballImage = null;

    /// <summary>
    /// Flag to track if Pokeball animation has started
    /// </summary>
    private bool pokeballAnimationStarted = false;

    // Battle System Elements
    /// <summary>
    /// The wild Pokemon encountered in battle
    /// </summary>
    private Pokemon? wildPokemon = null;

    /// <summary>
    /// Image of the wild Pokemon
    /// </summary>
    private Image? wildPokemonImage = null;

    /// <summary>
    /// List of Pokemon the current user owns
    /// </summary>
    private List<Pokemon>? userPokemon = null;

    /// <summary>
    /// The Pokemon selected by the user for battle
    /// </summary>
    private Pokemon? selectedUserPokemon = null;

    /// <summary>
    /// Image of the selected user Pokemon
    /// </summary>
    private Image? selectedUserPokemonImage = null;

    /// <summary>
    /// Flag indicating if Pokemon selection box is visible
    /// </summary>
    private bool showPokemonSelection = false;

    /// <summary>
    /// Currently highlighted Pokemon in the selection box
    /// </summary>
    private int selectedPokemonIndex = 0;

    /// <summary>
    /// Phases of the wild Pokemon encounter transition
    /// </summary>
    private enum EncounterPhase
    {
        None,
        FadingToWhite,
        PokeballGrowing,
        PokeballHolding,
        PokeballShrinking,
        BattleSetup,
        PokemonSelection,
        BattleReady
    }

    /// <summary>
    /// Initializes a new instance of the Form1 class and starts the game
    /// /// <param name="selectedUser">The user selected from the main menu (optional for backward compatibility)</param>
    public Form1(User? selectedUser = null)
    {
        currentUser = selectedUser;
        
        InitializeComponent();
        InitializeGame();
        SetupForm();
        InitializeGameTimer();

        this.MaximizeBox = false;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;

        // Try to load textures from the textures folder in the application lib folder.
        string texturesPath = Path.Combine(Application.StartupPath, "../../../lib/textures");
        LoadTextures(texturesPath);

        // Try to load Pokeball image for encounters
        LoadPokeballImage(texturesPath);

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

        // Handle encounter transitions
        if (inWildEncounter)
        {
            UpdateEncounterTransition();
            needsRedraw = true;
        }
        else
        {
            // Update player movement only if not in encounter
            if (player.UpdateMovement(tiles))
            {
                needsRedraw = true;
                
                // Check for wild Pokemon encounters when player moves
                CheckForWildEncounter();
            }

            // Update camera position
            UpdateCamera();
        }

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
            
            string userInfo = currentUser != null ?
                $" - User: {currentUser.Username} (Lvl {currentUser.TrainerLevel})" :
                " - Guest Mode";

            string encounterInfo = inWildEncounter ?
                $" - Encounter: {encounterPhase}" :
                "";
            
            this.Text = $"Pokedex Game - FPS: {currentFPS:F1} - Keys: {player.GetKeyBuffer()}{spriteInfo}{userInfo}{encounterInfo}";
        }
    }

    /// <summary>
    /// Checks if the player should encounter a wild Pokemon on the current tile
    /// </summary>
    private void CheckForWildEncounter()
    {
        // Get the tile the player is currently on
        int playerTileX = (int)(player.GetVisualX() / tileSize);
        int playerTileY = (int)(player.GetVisualY() / tileSize);

        var currentTile = tiles.FirstOrDefault(t => 
            t.X == playerTileX * tileSize && t.Y == playerTileY * tileSize);

        if (currentTile != null && currentTile.WildZone)
        {
            // 5% chance of encounter on each step in a wild zone
            if (encounterRandom.NextDouble() < ENCOUNTER_CHANCE)
            {
                StartWildEncounter();
            }
        }
    }

    /// <summary>
    /// Initiates a wild Pokemon encounter transition
    /// </summary>
    private void StartWildEncounter()
    {
        inWildEncounter = true;
        encounterPhase = EncounterPhase.FadingToWhite;
        encounterTimer = 0f;
        fadeAlpha = 0f;
        pokeballScale = 0f;
        pokeballAnimationStarted = false; // Reset Pokeball animation flag
        
        // Reset battle elements
        wildPokemon = null;
        wildPokemonImage = null;
        userPokemon = null;
        selectedUserPokemon = null;
        selectedUserPokemonImage = null;
        showPokemonSelection = false;
        selectedPokemonIndex = 0;
        
        // Hide button1 when encounter starts (as screen begins to glow white)
        if (button1 != null)
        {
            button1.Visible = false;
        }
        
        // Pause player movement during encounter
        // (This is handled by the inWildEncounter check in GameTimer_Tick)
        
        needsRedraw = true;
    }

    /// <summary>
    /// Updates the encounter transition animation
    /// </summary>
    private void UpdateEncounterTransition()
    {
        encounterTimer += 1f / TARGET_FPS; // Add frame time

        switch (encounterPhase)
        {
            case EncounterPhase.FadingToWhite:
                // Fade to white over FADE_DURATION seconds
                fadeAlpha = Math.Min(1f, encounterTimer / FADE_DURATION);
                
                // Hide button1 as soon as the fade begins (extra safety check)
                if (button1 != null && button1.Visible && fadeAlpha > 0)
                {
                    button1.Visible = false;
                }

                // Start Pokeball animation when fade reaches the threshold (15% complete)
                if (!pokeballAnimationStarted && fadeAlpha >= POKEBALL_START_THRESHOLD)
                {
                    pokeballAnimationStarted = true;
                    // The Pokeball will start growing from this point while fade continues
                }

                // Update Pokeball if its animation has started
                if (pokeballAnimationStarted)
                {
                    // Calculate how long the Pokeball has been animating since it started
                    float timeFromThreshold = (fadeAlpha - POKEBALL_START_THRESHOLD) / (1f - POKEBALL_START_THRESHOLD);
                    float pokeballAnimationTime = timeFromThreshold * (FADE_DURATION * (1f - POKEBALL_START_THRESHOLD));
                    
                    // If fade is complete, add the time since fade completed
                    if (fadeAlpha >= 1f)
                    {
                        float timeAfterFade = encounterTimer - FADE_DURATION;
                        pokeballAnimationTime = (FADE_DURATION * (1f - POKEBALL_START_THRESHOLD)) + timeAfterFade;
                    }

                    // Calculate progress based on total Pokeball animation time
                    float pokeballProgress = pokeballAnimationTime / POKEBALL_GROW_DURATION;
                    pokeballScale = Math.Min(1f, pokeballProgress) * pokeballTargetScale;
                }
                
                if (encounterTimer >= FADE_DURATION)
                {
                    // Transition to Pokeball growing phase (but don't reset timer)
                    encounterPhase = EncounterPhase.PokeballGrowing;
                    // DO NOT reset encounterTimer - let it continue counting
                }
                break;

            case EncounterPhase.PokeballGrowing:
                // Continue Pokeball growth using the same timing calculation
                if (pokeballAnimationStarted && pokeballScale < pokeballTargetScale)
                {
                    // Calculate total time since Pokeball animation started
                    float timeAfterFade = encounterTimer - FADE_DURATION;
                    float totalPokeballTime = (FADE_DURATION * (1f - POKEBALL_START_THRESHOLD)) + timeAfterFade;
                    float pokeballProgress = totalPokeballTime / POKEBALL_GROW_DURATION;
                    
                    pokeballScale = Math.Min(1f, pokeballProgress) * pokeballTargetScale;
                }
                
                if (pokeballScale >= pokeballTargetScale)
                {
                    // Pokeball reached full size, transition to holding phase
                    encounterPhase = EncounterPhase.PokeballHolding;
                    encounterTimer = 0f; // Reset timer for holding duration
                }
                break;

            case EncounterPhase.PokeballHolding:
                // Hold Pokeball at full size for POKEBALL_HOLD_DURATION seconds
                if (encounterTimer >= POKEBALL_HOLD_DURATION)
                {
                    // Start shrinking the Pokeball
                    encounterPhase = EncounterPhase.PokeballShrinking;
                    encounterTimer = 0f; // Reset timer for shrinking animation
                }
                break;

            case EncounterPhase.PokeballShrinking:
                // Shrink Pokeball quickly over POKEBALL_SHRINK_DURATION seconds
                float shrinkProgress = encounterTimer / POKEBALL_SHRINK_DURATION;
                pokeballScale = (1f - Math.Min(1f, shrinkProgress)) * pokeballTargetScale;
                
                if (encounterTimer >= POKEBALL_SHRINK_DURATION)
                {
                    // Pokeball has shrunk completely, transition to battle setup
                    pokeballScale = 0f;
                    encounterPhase = EncounterPhase.BattleSetup;
                    SetupBattle();
                }
                break;

            case EncounterPhase.PokemonSelection:
                // Pokemon selection is handled by input events
                // This phase continues until user selects a Pokemon
                break;

            case EncounterPhase.BattleReady:
                // Battle is ready - this is the final softlock state
                break;
        }
    }

    /// <summary>
    /// Ends the current wild Pokemon encounter and restores normal game state
    /// This method is for future use when the battle system is implemented
    /// </summary>
    private void EndWildEncounter()
    {
        inWildEncounter = false;
        encounterPhase = EncounterPhase.None;
        encounterTimer = 0f;
        fadeAlpha = 0f;
        pokeballScale = 0f;
        pokeballAnimationStarted = false; // Reset Pokeball animation flag
        
        // Clean up battle elements
        wildPokemon = null;
        wildPokemonImage?.Dispose();
        wildPokemonImage = null;
        userPokemon = null;
        selectedUserPokemon = null;
        selectedUserPokemonImage?.Dispose();
        selectedUserPokemonImage = null;
        showPokemonSelection = false;
        selectedPokemonIndex = 0;
        
        // Restore button1 visibility when encounter ends
        if (button1 != null)
        {
            button1.Visible = true;
        }
        
        needsRedraw = true;
    }

    /// <summary>
    /// Sets up the form properties and enables necessary styles for game rendering
    /// </summary>
    private void SetupForm()
    {
        // Enable the form to receive key events
        this.KeyPreview = true;
        this.Focus();

        // Set form title based on current user
        string baseTitle = "Pokedex Game";
        if (currentUser != null)
        {
            this.Text = $"{baseTitle} - {currentUser.FirstName} {currentUser.LastName} ({currentUser.Username})";
        }
        else
        {
            this.Text = $"{baseTitle} - Guest Mode";
        }

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
        // Generate map using the Map class
        int mapWidth = 40; // Larger map for more exploration
        int mapHeight = 30;
        tiles = Map.GenerateMap(mapWidth, mapHeight, tileSize);

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

        // Load textures for different tile types
        string grassTexturePath = Path.Combine(texturesDirectory, "Grass.png");
        string dirtTexturePath = Path.Combine(texturesDirectory, "Dirt.png");
        string sandTexturePath = Path.Combine(texturesDirectory, "Sand.png");
        string stoneTexturePath = Path.Combine(texturesDirectory, "Stone.png");
        string waterTexturePath = Path.Combine(texturesDirectory, "Water.png");

        foreach (var tile in tiles)
        {
            // Match tiles by color to their corresponding textures
            if (tile.TileColor == Color.Green && File.Exists(grassTexturePath))
            {
                tile.SetTexture(grassTexturePath);
            }
            else if (tile.TileColor == Color.SaddleBrown && File.Exists(dirtTexturePath))
            {
                tile.SetTexture(dirtTexturePath);
            }
            else if (tile.TileColor == Color.SandyBrown && File.Exists(sandTexturePath))
            {
                tile.SetTexture(sandTexturePath);
            }
            else if (tile.TileColor == Color.Gray && File.Exists(stoneTexturePath))
            {
                tile.SetTexture(stoneTexturePath);
            }
            else if (tile.TileColor == Color.Blue && File.Exists(waterTexturePath))
            {
                tile.SetTexture(waterTexturePath);
            }
        }

        needsRedraw = true;
    }

    /// <summary>
    /// Loads the Pokeball image for encounter transitions
    /// </summary>
    /// <param name="texturesDirectory">Path to the textures directory</param>
    private void LoadPokeballImage(string texturesDirectory)
    {
        string pokeballPath = Path.Combine(texturesDirectory, "Pokeball.png");
        if (File.Exists(pokeballPath))
        {
            try
            {
                pokeballImage = Image.FromFile(pokeballPath);
            }
            catch
            {
                // If we can't load the image, create a simple placeholder
                CreatePokeballPlaceholder();
            }
        }
        else
        {
            // Create a simple placeholder Pokeball if the image doesn't exist
            CreatePokeballPlaceholder();
        }
    }

    /// <summary>
    /// Creates a simple Pokeball placeholder image if the actual image is not available
    /// </summary>
    private void CreatePokeballPlaceholder()
    {
        int size = 200;
        Bitmap pokeball = new Bitmap(size, size);
        using (Graphics g = Graphics.FromImage(pokeball))
        {
            g.Clear(Color.Transparent);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Draw the main circle (white bottom, red top)
            using (Brush whiteBrush = new SolidBrush(Color.White))
            using (Brush redBrush = new SolidBrush(Color.Red))
            using (Brush blackBrush = new SolidBrush(Color.Black))
            {
                // White bottom half
                g.FillPie(whiteBrush, 0, 0, size, size, 0, 180);
                // Red top half
                g.FillPie(redBrush, 0, 0, size, size, 180, 180);
                
                // Black border
                g.DrawEllipse(new Pen(Color.Black, 4), 2, 2, size - 4, size - 4);
                
                // Middle line
                g.FillRectangle(blackBrush, 0, size / 2 - 4, size, 8);
                
                // Center button
                g.FillEllipse(whiteBrush, size / 2 - 20, size / 2 - 20, 40, 40);
                g.DrawEllipse(new Pen(Color.Black, 2), size / 2 - 20, size / 2 - 20, 40, 40);
            }
        }
        pokeballImage = pokeball;
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

        // Reset transform before drawing encounter effects
        g.ResetTransform();

        // Draw encounter transition effects
        if (inWildEncounter)
        {
            DrawEncounterEffects(g);
        }
    }

    /// <summary>
    /// Draws the encounter transition effects (fade to white and Pokeball)
    /// </summary>
    /// <param name="g">Graphics context for drawing</param>
    private void DrawEncounterEffects(Graphics g)
    {
        // Draw white fade overlay (battlefield background)
        if (fadeAlpha > 0)
        {
            using (Brush fadeOverlay = new SolidBrush(Color.FromArgb((int)(255 * fadeAlpha), Color.White)))
            {
                g.FillRectangle(fadeOverlay, 0, 0, ClientSize.Width, ClientSize.Height);
            }
        }

        // Draw Pokeball if it should be visible
        if (pokeballScale > 0 && pokeballImage != null)
        {
            // Calculate Pokeball size and position
            float targetHeight = ClientSize.Height * pokeballTargetScale;
            float currentHeight = targetHeight * (pokeballScale / pokeballTargetScale);
            float currentWidth = currentHeight; // Keep it square
            
            float x = (ClientSize.Width - currentWidth) / 2;
            float y = (ClientSize.Height - currentHeight) / 2;

            // Enable anti-aliasing for smoother Pokeball rendering
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            g.DrawImage(pokeballImage, x, y, currentWidth, currentHeight);

            // Reset rendering mode
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
        }

        // Draw battle elements if in appropriate phases
        if (encounterPhase >= EncounterPhase.BattleSetup)
        {
            DrawBattleElements(g);
        }
    }

    /// <summary>
    /// Draws the battle elements including Pokemon and selection interface
    /// </summary>
    /// <param name="g">Graphics context for drawing</param>
    private void DrawBattleElements(Graphics g)
    {
        // Draw Pokeball indicator in top left corner
        DrawPokeballIndicator(g);

        // Draw wild Pokemon on the right side of screen
        if (wildPokemon != null && wildPokemonImage != null)
        {
            float pokemonSize = ClientSize.Height * 0.3f; // 30% of screen height
            float x = ClientSize.Width * 0.75f - pokemonSize / 2; // Right side, more to the right
            float y = ClientSize.Height * 0.25f; // Upper portion of screen
            
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            
            g.DrawImage(wildPokemonImage, x, y, pokemonSize, pokemonSize);
            
            // Draw wild Pokemon health bar above the Pokemon
            DrawPokemonHealthBar(g, wildPokemon, x, y - 60, pokemonSize, true);
            
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
        }

        // Draw user's Pokemon on the left side if selected (same height as wild Pokemon)
        if (selectedUserPokemon != null && selectedUserPokemonImage != null && !showPokemonSelection)
        {
            float pokemonSize = ClientSize.Height * 0.3f; // Same size as wild Pokemon
            float x = ClientSize.Width * 0.25f - pokemonSize / 2; // Left side, centered
            float y = ClientSize.Height * 0.25f; // Same height as wild Pokemon
            
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            
            g.DrawImage(selectedUserPokemonImage, x, y, pokemonSize, pokemonSize);
            
            // Draw user Pokemon health bar above the Pokemon
            DrawPokemonHealthBar(g, selectedUserPokemon, x, y - 60, pokemonSize, false);
            
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
        }

        // Draw moves box at the bottom center of screen
        if (encounterPhase == EncounterPhase.BattleReady && !showPokemonSelection)
        {
            DrawMovesBox(g);
        }

        // Draw Pokemon selection box
        if (showPokemonSelection && userPokemon != null)
        {
            DrawPokemonSelectionBox(g);
        }
    }

    /// <summary>
    /// Draws a health bar with Pokemon name above it
    /// </summary>
    /// <param name="g">Graphics context for drawing</param>
    /// <param name="pokemon">The Pokemon to draw health bar for</param>
    /// <param name="x">X position of the health bar</param>
    /// <param name="y">Y position of the health bar</param>
    /// <param name="width">Width of the health bar area</param>
    /// <param name="isWild">Whether this is for a wild Pokemon</param>
    private void DrawPokemonHealthBar(Graphics g, Pokemon pokemon, float x, float y, float width, bool isWild)
    {
        // Calculate health percentage (for now assume current HP = max HP)
        float healthPercentage = 1.0f; // 100% health for now
        
        // Health bar dimensions
        float healthBarWidth = width * 0.8f;
        float healthBarHeight = 20f;
        float healthBarX = x + (width - healthBarWidth) / 2;
        float healthBarY = y + 25;

        // Draw Pokemon name above health bar
        string pokemonName = isWild ? $"Wild {pokemon.Name}" : pokemon.Name;
        using (Font nameFont = new Font("Arial", 12, FontStyle.Bold))
        using (Brush nameTextBrush = new SolidBrush(Color.Black))
        {
            var nameSize = g.MeasureString(pokemonName, nameFont);
            g.DrawString(pokemonName, nameFont, nameTextBrush, 
                healthBarX + (healthBarWidth - nameSize.Width) / 2, y);
        }

        // Draw health bar background (dark red)
        using (Brush bgBrush = new SolidBrush(Color.DarkRed))
        {
            g.FillRectangle(bgBrush, healthBarX, healthBarY, healthBarWidth, healthBarHeight);
        }

        // Draw current health (green to red gradient based on health)
        Color healthColor = healthPercentage > 0.5f ? Color.Green : 
                           healthPercentage > 0.25f ? Color.Yellow : Color.Red;
        
        using (Brush healthBrush = new SolidBrush(healthColor))
        {
            float currentHealthWidth = healthBarWidth * healthPercentage;
            g.FillRectangle(healthBrush, healthBarX, healthBarY, currentHealthWidth, healthBarHeight);
        }

        // Draw health bar border
        using (Pen borderPen = new Pen(Color.Black, 2))
        {
            g.DrawRectangle(borderPen, healthBarX, healthBarY, healthBarWidth, healthBarHeight);
        }

        // Draw health text
        string healthText = $"{pokemon.HP}/{pokemon.HP}"; // For now show max/max
        using (Font healthFont = new Font("Arial", 8, FontStyle.Bold))
        using (Brush healthTextBrush = new SolidBrush(Color.White))
        {
            var healthTextSize = g.MeasureString(healthText, healthFont);
            g.DrawString(healthText, healthFont, healthTextBrush, 
                healthBarX + (healthBarWidth - healthTextSize.Width) / 2, 
                healthBarY + (healthBarHeight - healthTextSize.Height) / 2);
        }

        // Draw level for user Pokemon
        if (!isWild)
        {
            string levelText = $"Lv.{pokemon.Level}";
            using (Font levelFont = new Font("Arial", 10, FontStyle.Bold))
            using (Brush levelTextBrush = new SolidBrush(Color.Black))
            {
                g.DrawString(levelText, levelFont, levelTextBrush, 
                    healthBarX + healthBarWidth + 5, healthBarY + 2);
            }
        }
    }

    /// <summary>
    /// Draws the moves selection box at the bottom of the screen
    /// </summary>
    /// <param name="g">Graphics context for drawing</param>
    private void DrawMovesBox(Graphics g)
    {
        // Moves box dimensions - bottom center of screen
        float boxWidth = ClientSize.Width * 0.6f;
        float boxHeight = ClientSize.Height * 0.25f;
        float boxX = (ClientSize.Width - boxWidth) / 2;
        float boxY = ClientSize.Height - boxHeight - 20;

        // Draw light gray background
        using (Brush bgBrush = new SolidBrush(Color.LightGray))
        {
            g.FillRectangle(bgBrush, boxX, boxY, boxWidth, boxHeight);
        }

        // Draw border
        using (Pen borderPen = new Pen(Color.DarkGray, 3))
        {
            g.DrawRectangle(borderPen, boxX, boxY, boxWidth, boxHeight);
        }

        // Draw title
        using (Font titleFont = new Font("Arial", 16, FontStyle.Bold))
        using (Brush titleBrush = new SolidBrush(Color.Black))
        {
            string title = "Pokemon Moves";
            var titleSize = g.MeasureString(title, titleFont);
            g.DrawString(title, titleFont, titleBrush, 
                boxX + (boxWidth - titleSize.Width) / 2, boxY + 10);
        }

        // Placeholder text for moves (will be replaced with actual moves later)
        using (Font movesFont = new Font("Arial", 12))
        using (Brush movesTextBrush = new SolidBrush(Color.DarkGray))
        {
            string placeholderText = "Move selection will be implemented here\n\n" +
                                   "• Attack moves\n" +
                                   "• Defense moves\n" +
                                   "• Special abilities\n" +
                                   "• Items";
            
            g.DrawString(placeholderText, movesFont, movesTextBrush, 
                boxX + 20, boxY + 50);
        }
    }

    /// <summary>
    /// Draws the Pokemon selection interface
    /// </summary>
    /// <param name="g">Graphics context for drawing</param>
    private void DrawPokemonSelectionBox(Graphics g)
    {
        if (userPokemon == null || userPokemon.Count == 0) return;

        // Selection box covers left half of screen
        float boxWidth = ClientSize.Width * 0.5f;
        float boxHeight = ClientSize.Height * 0.8f;
        float boxX = 0; // Left side of screen
        float boxY = ClientSize.Height * 0.1f;

        // Draw solid blue background (non-transparent)
        using (Brush bgBrush = new SolidBrush(Color.FromArgb(255, 30, 70, 130)))
        {
            g.FillRectangle(bgBrush, boxX, boxY, boxWidth, boxHeight);
        }

        // Draw border
        using (Pen borderPen = new Pen(Color.Navy, 3))
        {
            g.DrawRectangle(borderPen, boxX, boxY, boxWidth, boxHeight);
        }

        // Draw title
        using (Font titleFont = new Font("Arial", 18, FontStyle.Bold))
        using (Brush titleBrush = new SolidBrush(Color.White))
        {
            string title = "Choose Your Pokemon";
            var titleSize = g.MeasureString(title, titleFont);
            g.DrawString(title, titleFont, titleBrush, 
                boxX + (boxWidth - titleSize.Width) / 2, boxY + 20);
        }

        // Draw Pokemon list
        float itemHeight = 70;
        float startY = boxY + 80;
        
        for (int i = 0; i < userPokemon.Count; i++)
        {
            var pokemon = userPokemon[i];
            float itemY = startY + i * itemHeight;
            
            // Highlight selected Pokemon
            if (i == selectedPokemonIndex)
            {
                using (Brush highlightBrush = new SolidBrush(Color.FromArgb(200, 255, 255, 0)))
                {
                    g.FillRectangle(highlightBrush, boxX + 10, itemY - 5, boxWidth - 20, itemHeight - 10);
                }
            }
            
            // Draw Pokemon name
            using (Font pokemonFont = new Font("Arial", 14, FontStyle.Bold))
            using (Brush pokemonBrush = new SolidBrush(Color.White))
            {
                g.DrawString(pokemon.Name, pokemonFont, pokemonBrush, boxX + 30, itemY + 5);
            }
            
            // Draw Pokemon type and level
            using (Font typeFont = new Font("Arial", 10))
            using (Brush typeBrush = new SolidBrush(Color.LightGray))
            {
                g.DrawString($"Type: {pokemon.PokemonType1} | Level: {pokemon.Level}", typeFont, typeBrush, boxX + 30, itemY + 25);
            }
            
            // Draw Pokemon stats
            using (Font statsFont = new Font("Arial", 9))
            using (Brush statsBrush = new SolidBrush(Color.LightBlue))
            {
                g.DrawString($"HP: {pokemon.HP} | ATK: {pokemon.Attack} | DEF: {pokemon.Defense}", statsFont, statsBrush, boxX + 30, itemY + 45);
            }
        }

        // Draw instructions
        using (Font instructFont = new Font("Arial", 12))
        using (Brush instructBrush = new SolidBrush(Color.White))
        {
            string instructions = "Click on a Pokemon to select it\nKeyboard: W/S or Up/Down to navigate\nPress Enter/Space to select, Escape to cancel";
            g.DrawString(instructions, instructFont, instructBrush, 
                boxX + 20, boxY + boxHeight - 100);
        }
    }

    /// <summary>
    /// Sets up the battle by selecting a random Pokemon from database and loading user's Pokemon
    /// </summary>
    private void SetupBattle()
    {
        try
        {
            PokedexDB db = new PokedexDB();
            
            // Get a random wild Pokemon from the actual database
            wildPokemon = GetRandomPokemonFromDatabase(db);
            if (wildPokemon != null)
            {
                LoadWildPokemonImage();
            }
            
            // Load user's actual collected Pokemon from database
            if (currentUser != null)
            {
                List<CollectedPokemon> collectedPokemon = db.GetAllCollectedPokemon(currentUser.UserId);
                
                if (collectedPokemon.Count > 0)
                {
                    // Convert CollectedPokemon to Pokemon objects for battle system
                    userPokemon = collectedPokemon.Select(cp => new Pokemon
                    {
                        Name = cp.Name,
                        PokemonType1 = cp.PokemonType1,
                        PokemonType2 = cp.PokemonType2,
                        HP = cp.HP,
                        Attack = cp.Attack,
                        Defense = cp.Defense,
                        SpAttack = cp.SpAttack,
                        SpDefense = cp.SpDefense,
                        Speed = cp.Speed,
                        Level = cp.Level
                    }).ToList();
                    
                    encounterPhase = EncounterPhase.PokemonSelection;
                    showPokemonSelection = true;
                    selectedPokemonIndex = 0;
                }
                else
                {
                    // User has no collected Pokemon - skip to battle ready (guest mode)
                    encounterPhase = EncounterPhase.BattleReady;
                }
            }
            else
            {
                // User is in guest mode - skip to battle ready
                encounterPhase = EncounterPhase.BattleReady;
            }
        }
        catch (Exception ex)
        {
            // On error, fall back to mock data for demo purposes
            System.Diagnostics.Debug.WriteLine($"Error setting up battle with database: {ex.Message}");
            
            // Fallback to mock data
            var (pokemonName, pokemonType) = GetRandomPokemonData();
            wildPokemon = new Pokemon 
            { 
                Name = pokemonName,
                PokemonType1 = pokemonType,
                HP = 100,
                Attack = 50,
                Defense = 50,
                SpAttack = 50,
                SpDefense = 50,
                Speed = 50,
                Level = Random.Shared.Next(5, 15)
            };
            LoadWildPokemonImage();
            
            if (currentUser != null)
            {
                // Mock data as fallback
                userPokemon = new List<Pokemon>
                {
                    new Pokemon { Name = "Bulbasaur", PokemonType1 = "Grass", HP = 80, Attack = 40, Defense = 40, SpAttack = 60, SpDefense = 60, Speed = 45 },
                    new Pokemon { Name = "Charmander", PokemonType1 = "Fire", HP = 70, Attack = 52, Defense = 35, SpAttack = 60, SpDefense = 50, Speed = 65 },
                    new Pokemon { Name = "Squirtle", PokemonType1 = "Water", HP = 85, Attack = 45, Defense = 65, SpAttack = 50, SpDefense = 64, Speed = 43 }
                };
                encounterPhase = EncounterPhase.PokemonSelection;
                showPokemonSelection = true;
                selectedPokemonIndex = 0;
            }
            else
            {
                encounterPhase = EncounterPhase.BattleReady;
            }
        }
        
        needsRedraw = true;
    }

    /// <summary>
    /// Gets a random Pokemon from the database
    /// </summary>
    private Pokemon? GetRandomPokemonFromDatabase(PokedexDB db)
    {
        try
        {
            // Get all Pokemon from database and select one randomly
            var allPokemon = GetAllPokemonFromDatabase(db);
            if (allPokemon.Count > 0)
            {
                var randomPokemon = allPokemon[encounterRandom.Next(allPokemon.Count)];
                // Set random level for wild Pokemon
                randomPokemon.Level = encounterRandom.Next(5, 15);
                return randomPokemon;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting random Pokemon from database: {ex.Message}");
        }
        return null;
    }

    /// <summary>
    /// Gets all Pokemon from the database
    /// </summary>
    private List<Pokemon> GetAllPokemonFromDatabase(PokedexDB db)
    {
        List<Pokemon> allPokemon = new List<Pokemon>();
        try
        {
            using SqlConnection connection = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=PokedexDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False");
            connection.Open();

            string query = "SELECT * FROM Pokemon";
            using SqlCommand command = new SqlCommand(query, connection);
            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                allPokemon.Add(new Pokemon
                {
                    PokemonID = reader.GetInt32(reader.GetOrdinal("PokemonId")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    HP = reader.GetInt32(reader.GetOrdinal("HP")),
                    Attack = reader.GetInt32(reader.GetOrdinal("Attack")),
                    Defense = reader.GetInt32(reader.GetOrdinal("Defense")),
                    SpAttack = reader.GetInt32(reader.GetOrdinal("SpecialAttack")),
                    SpDefense = reader.GetInt32(reader.GetOrdinal("SpecialDefense")),
                    Speed = reader.GetInt32(reader.GetOrdinal("Speed")),
                    PokemonType1 = reader.GetString(reader.GetOrdinal("PokemonType1")),
                    PokemonType2 = reader.IsDBNull(reader.GetOrdinal("PokemonType2")) ? null : reader.GetString(reader.GetOrdinal("PokemonType2")),
                    Level = 1 // Default level, will be overridden for wild Pokemon
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting all Pokemon from database: {ex.Message}");
        }
        return allPokemon;
    }

    /// <summary>
    /// Gets a random Pokemon name and type for wild encounters (fallback method)
    /// </summary>
    private (string name, string type) GetRandomPokemonData()
    {
        (string name, string type)[] pokemonData = { 
            ("Pikachu", "Electric"), ("Charizard", "Fire"), ("Blastoise", "Water"), ("Venusaur", "Grass"), 
            ("Alakazam", "Psychic"), ("Machamp", "Fighting"), ("Gengar", "Ghost"), ("Dragonite", "Dragon"), 
            ("Mewtwo", "Psychic"), ("Mew", "Psychic"), ("Caterpie", "Bug"), ("Weedle", "Bug"), 
            ("Pidgey", "Flying"), ("Rattata", "Normal"), ("Spearow", "Flying")
        };
        return pokemonData[encounterRandom.Next(pokemonData.Length)];
    }

    /// <summary>
    /// Loads the image for the wild Pokemon
    /// </summary>
    private void LoadWildPokemonImage()
    {
        if (wildPokemon == null) return;
        
        string texturesPath = Path.Combine(Application.StartupPath, "../../../lib/textures");
        string pokemonImagePath = Path.Combine(texturesPath, "Pokemon", $"{wildPokemon.Name}.png");
        
        try
        {
            if (File.Exists(pokemonImagePath))
            {
                wildPokemonImage = Image.FromFile(pokemonImagePath);
            }
            else
            {
                // Create a placeholder if image doesn't exist
                CreatePokemonPlaceholder(wildPokemon.Name, out wildPokemonImage);
            }
        }
        catch
        {
            // Create a simple placeholder on any error
            CreatePokemonPlaceholder(wildPokemon.Name, out wildPokemonImage);
        }
    }

    /// <summary>
    /// Loads the image for the selected user Pokemon
    /// </summary>
    private void LoadSelectedUserPokemonImage()
    {
        if (selectedUserPokemon == null) return;
        
        string texturesPath = Path.Combine(Application.StartupPath, "../../../lib/textures");
        string pokemonImagePath = Path.Combine(texturesPath, "Pokemon", $"{selectedUserPokemon.Name}.png");
        
        try
        {
            if (File.Exists(pokemonImagePath))
            {
                selectedUserPokemonImage = Image.FromFile(pokemonImagePath);
            }
            else
            {
                // Create a placeholder if image doesn't exist
                CreatePokemonPlaceholder(selectedUserPokemon.Name, out selectedUserPokemonImage);
            }
        }
        catch
        {
            // Create a simple placeholder on any error
            CreatePokemonPlaceholder(selectedUserPokemon.Name, out selectedUserPokemonImage);
        }
    }

    /// <summary>
    /// Creates a placeholder image for a Pokemon if the actual image is not available
    /// </summary>
    private void CreatePokemonPlaceholder(string pokemonName, out Image? placeholderImage)
    {
        int size = 150;
        Bitmap placeholder = new Bitmap(size, size);
        using (Graphics g = Graphics.FromImage(placeholder))
        {
            g.Clear(Color.LightGray);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            
            // Draw a simple circle
            using (Brush brush = new SolidBrush(Color.DarkGray))
            {
                g.FillEllipse(brush, 20, 20, size - 40, size - 40);
            }
            
            // Draw Pokemon name
            using (Font font = new Font("Arial", 10, FontStyle.Bold))
            {
                using (Brush textBrush = new SolidBrush(Color.Black))
                {
                    var textSize = g.MeasureString(pokemonName, font);
                    float x = (size - textSize.Width) / 2;
                    float y = (size - textSize.Height) / 2;
                    g.DrawString(pokemonName, font, textBrush, x, y);
                }
            }
        }
        placeholderImage = placeholder;
    }

    /// <summary>
    /// Handles key press events for player movement and battle interactions
    /// </summary>
    /// <param name="e">Key event arguments containing the pressed key information</param>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        // Handle battle interactions if in encounter (keyboard controls still supported as backup)
        if (inWildEncounter && encounterPhase == EncounterPhase.PokemonSelection)
        {
            HandlePokemonSelectionInput(e);
            return;
        }

        // Normal movement handling
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
    /// Handles mouse click events for Pokemon selection
    /// </summary>
    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);

        // Handle Pokemon selection with mouse clicks
        if (inWildEncounter && encounterPhase == EncounterPhase.PokemonSelection && showPokemonSelection && userPokemon != null)
        {
            HandlePokemonSelectionMouseClick(e);
        }
    }

    /// <summary>
    /// Handles mouse movement for Pokemon selection highlighting
    /// </summary>
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        // Handle Pokemon selection highlighting with mouse hover
        if (inWildEncounter && encounterPhase == EncounterPhase.PokemonSelection && showPokemonSelection && userPokemon != null)
        {
            HandlePokemonSelectionMouseMove(e);
        }
    }

    /// <summary>
    /// Handles input during Pokemon selection phase
    /// </summary>
    private void HandlePokemonSelectionInput(KeyEventArgs e)
    {
        if (userPokemon == null || userPokemon.Count == 0) return;

        switch (e.KeyCode)
        {
            case Keys.Up or Keys.W:
                selectedPokemonIndex = Math.Max(0, selectedPokemonIndex - 1);
                needsRedraw = true;
                break;
                
            case Keys.Down or Keys.S:
                selectedPokemonIndex = Math.Min(userPokemon.Count - 1, selectedPokemonIndex + 1);
                needsRedraw = true;
                break;
                
            case Keys.Enter or Keys.Space:
                // Select the current Pokemon
                selectedUserPokemon = userPokemon[selectedPokemonIndex];
                LoadSelectedUserPokemonImage();
                showPokemonSelection = false;
                encounterPhase = EncounterPhase.BattleReady;
                needsRedraw = true;
                break;
                
            case Keys.Escape:
                // Cancel selection (for now, just select the first Pokemon)
                if (userPokemon.Count > 0)
                {
                    selectedUserPokemon = userPokemon[0];
                    LoadSelectedUserPokemonImage();
                    showPokemonSelection = false;
                    encounterPhase = EncounterPhase.BattleReady;
                    needsRedraw = true;
                }
                break;
        }
    }

    /// <summary>
    /// Handles mouse clicks during Pokemon selection phase
    /// </summary>
    private void HandlePokemonSelectionMouseClick(MouseEventArgs e)
    {
        if (userPokemon == null || userPokemon.Count == 0) return;

        // Calculate selection box bounds
        float boxWidth = ClientSize.Width * 0.5f;
        float boxHeight = ClientSize.Height * 0.8f;
        float boxX = 0; // Left side of screen
        float boxY = ClientSize.Height * 0.1f;

        // Check if click is within the selection box
        if (e.X >= boxX && e.X <= boxX + boxWidth && e.Y >= boxY && e.Y <= boxY + boxHeight)
        {
            // Calculate which Pokemon was clicked
            float itemHeight = 70;
            float startY = boxY + 80;
            
            for (int i = 0; i < userPokemon.Count; i++)
            {
                float itemY = startY + i * itemHeight;
                
                if (e.Y >= itemY - 5 && e.Y <= itemY + itemHeight - 10)
                {
                    // Pokemon was clicked - select it
                    selectedUserPokemon = userPokemon[i];
                    LoadSelectedUserPokemonImage();
                    showPokemonSelection = false;
                    encounterPhase = EncounterPhase.BattleReady;
                    needsRedraw = true;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Handles mouse movement during Pokemon selection phase for highlighting
    /// </summary>
    private void HandlePokemonSelectionMouseMove(MouseEventArgs e)
    {
        if (userPokemon == null || userPokemon.Count == 0) return;

        // Calculate selection box bounds
        float boxWidth = ClientSize.Width * 0.5f;
        float boxHeight = ClientSize.Height * 0.8f;
        float boxX = 0; // Left side of screen
        float boxY = ClientSize.Height * 0.1f;

        // Check if mouse is within the selection box
        if (e.X >= boxX && e.X <= boxX + boxWidth && e.Y >= boxY && e.Y <= boxY + boxHeight)
        {
            // Calculate which Pokemon is being hovered
            float itemHeight = 70;
            float startY = boxY + 80;
            
            int newSelectedIndex = selectedPokemonIndex;
            for (int i = 0; i < userPokemon.Count; i++)
            {
                float itemY = startY + i * itemHeight;
                
                if (e.Y >= itemY - 5 && e.Y <= itemY + itemHeight - 10)
                {
                    newSelectedIndex = i;
                    break;
                }
            }
            
            // Update selection if it changed
            if (newSelectedIndex != selectedPokemonIndex)
            {
                selectedPokemonIndex = newSelectedIndex;
                needsRedraw = true;
            }
        }
    }

    /// <summary>
    /// Gets the currently logged in user
    /// </summary>
    /// <returns>The current User object, or null if no user is logged in</returns>
    public User? GetCurrentUser()
    {
        return currentUser;
    }

    /// <summary>
    /// Gets the current user's username
    /// </summary>
    /// <returns>The username of the current user, or "Guest" if no user is logged in</returns>
    public string GetCurrentUsername()
    {
        return currentUser?.Username ?? "Guest";
    }

    /// <summary>
    /// Gets the current user's trainer level
    /// </summary>
    /// <returns>The trainer level of the current user, or 1 if no user is logged in</returns>
    public int GetCurrentUserLevel()
    {
        return currentUser?.TrainerLevel ?? 1;
    }

    /// <summary>
    /// Gets the current user's ID
    /// </summary>
    /// <returns>The user ID of the current user, or -1 if no user is logged in</returns>
    public int GetCurrentUserId()
    {
        return currentUser?.UserId ?? -1;
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

    /// <summary>
    /// Draws a small Pokeball indicator in the top left corner
    /// </summary>
    /// <param name="g">Graphics context for drawing</param>
    private void DrawPokeballIndicator(Graphics g)
    {
        float indicatorSize = 40f;
        float margin = 20f;
        float circleX = margin;
        float circleY = margin;

        // Draw circle background
        using (Brush circleBrush = new SolidBrush(Color.White))
        {
            g.FillEllipse(circleBrush, circleX, circleY, indicatorSize, indicatorSize);
        }

        // Draw circle border
        using (Pen circlePen = new Pen(Color.Black, 2))
        {
            g.DrawEllipse(circlePen, circleX, circleY, indicatorSize, indicatorSize);
        }

        // Draw small Pokeball inside the circle
        if (pokeballImage != null)
        {
            float pokeballSize = indicatorSize * 0.7f;
            float pokeballX = circleX + (indicatorSize - pokeballSize) / 2;
            float pokeballY = circleY + (indicatorSize - pokeballSize) / 2;

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            
            g.DrawImage(pokeballImage, pokeballX, pokeballY, pokeballSize, pokeballSize);
            
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
        }
        else
        {
            // Draw a simple Pokeball placeholder if image is not available
            float smallBallSize = indicatorSize * 0.6f;
            float smallBallX = circleX + (indicatorSize - smallBallSize) / 2;
            float smallBallY = circleY + (indicatorSize - smallBallSize) / 2;

            // Draw red top half
            using (Brush redBrush = new SolidBrush(Color.Red))
            {
                g.FillPie(redBrush, smallBallX, smallBallY, smallBallSize, smallBallSize, 180, 180);
            }

            // Draw white bottom half
            using (Brush whiteBrush = new SolidBrush(Color.White))
            {
                g.FillPie(whiteBrush, smallBallX, smallBallY, smallBallSize, smallBallSize, 0, 180);
            }

            // Draw border and center line
            using (Pen ballPen = new Pen(Color.Black, 1))
            {
                g.DrawEllipse(ballPen, smallBallX, smallBallY, smallBallSize, smallBallSize);
                g.DrawLine(ballPen, smallBallX, smallBallY + smallBallSize / 2, 
                          smallBallX + smallBallSize, smallBallY + smallBallSize / 2);
            }

            // Draw center button
            float buttonSize = smallBallSize * 0.25f;
            float buttonX = smallBallX + (smallBallSize - buttonSize) / 2;
            float buttonY = smallBallY + (smallBallSize - buttonSize) / 2;
            
            using (Brush buttonBrush = new SolidBrush(Color.White))
            {
                g.FillEllipse(buttonBrush, buttonX, buttonY, buttonSize, buttonSize);
            }
            
            using (Pen buttonPen = new Pen(Color.Black, 1))
            {
                g.DrawEllipse(buttonPen, buttonX, buttonY, buttonSize, buttonSize);
            }
        }
    }
}
