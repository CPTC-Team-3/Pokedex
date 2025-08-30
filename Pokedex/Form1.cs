using Microsoft.Data.SqlClient;
using System.Drawing.Imaging;
//
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
    private List<Tile> tiles = null!;

    /// <summary>
    /// The player instance controlled by user input
    /// </summary>
    private Player player = null!;

    /// <summary>
    /// Timer that controls the game loop and update frequency
    /// </summary>
    private System.Windows.Forms.Timer gameTimer = null!;

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

    //

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
    /// Chance of encountering a wild Pokemon on each step in a wild zone
    /// </summary>
    private const float ENCOUNTER_CHANCE = 0.15f;

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
    /// Original size of the Pokeball image
    /// </summary>
    private Image? smallPokeballImage = null;

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
    // Track original wild image when swapping to Pokeball
    private Image? wildPokemonImageOriginal = null;

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
    /// Index of the currently selected/highlighted move (0-3 for the four moves)
    /// </summary>
    private int selectedMoveIndex = -1;

    /// <summary>
    /// Index of the move currently being hovered over by the mouse
    /// </summary>
    private int hoveredMoveIndex = -1;

    /// <summary>
    /// The four universal moves that all Pokemon can use
    /// </summary>
    private readonly string[] pokemonMoves = { "Tackle", "Projectile", "Protect", "Rest" };

    /// <summary>
    /// Descriptions for each move
    /// </summary>
    private readonly string[] moveDescriptions = {
        "A basic physical attack",
        "Ranged attack based on Pokemon type", 
        "Defensive move that blocks damage",
        "Recovers HP and removes status effects"
    };

    // --- Battle turn state for move execution and announcer ---
    private string playerSelectedMoveName = string.Empty;
    private string wildSelectedMoveName = string.Empty;
    private bool isAnnouncingTurn = false;
    private readonly List<string> turnAnnouncements = new();
    private bool playerProtectedThisTurn = false;
    private bool wildProtectedThisTurn = false;
    // Catch flow state
    private bool catchPendingResolution = false;
    private bool wildTextureIsPokeball = false;
    private RectangleF catchButtonRect = RectangleF.Empty;

    // --- HP, KO and fade state ---
    private int playerMaxHP = 0, playerCurrentHP = 0;
    private int wildMaxHP = 0, wildCurrentHP = 0;
    private bool playerKO = false, wildKO = false;
    private float playerSpriteAlpha = 1f, wildSpriteAlpha = 1f;
    private const float KO_FADE_DURATION = 1.0f; // seconds

    // --- Tile arrival tracking for encounter checks ---
    private int lastPlayerTileX = -1;
    private int lastPlayerTileY = -1;

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

    // game started
        
        // Handle form closing to properly clean up resources and exit application
        this.FormClosing += Form1_FormClosing;
    }

    /// <summary>
    /// Handles the form closing event to properly clean up resources and exit the application
    /// </summary>
    private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
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
    private void GameTimer_Tick(object? sender, EventArgs e)
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
            bool movementUpdated = player.UpdateMovement(tiles);
            if (movementUpdated)
            {
                needsRedraw = true;
            }

            // Check for wild Pokemon encounters only when entering a new tile
            int currentTileX = (int)Math.Floor(player.GetVisualX() / tileSize);
            int currentTileY = (int)Math.Floor(player.GetVisualY() / tileSize);
            if (currentTileX != lastPlayerTileX || currentTileY != lastPlayerTileY)
            {
                lastPlayerTileX = currentTileX;
                lastPlayerTileY = currentTileY;
                TryWildEncounterOnCurrentTile(currentTileX, currentTileY);
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
    /// Called when the player enters a new tile; performs a wild encounter check for that tile.
    /// </summary>
    private void TryWildEncounterOnCurrentTile(int tileX, int tileY)
    {
        var currentTile = tiles.FirstOrDefault(t => t.X == tileX * tileSize && t.Y == tileY * tileSize);
        if (currentTile != null && currentTile.WildZone)
        {
            if (encounterRandom.NextDouble() < ENCOUNTER_CHANCE)
            {
                StartWildEncounter();
            }
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
        float dt = 1f / TARGET_FPS;

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
                // During battle, handle KO fade-outs
                if (wildKO)
                {
                    wildSpriteAlpha = Math.Max(0f, wildSpriteAlpha - dt / KO_FADE_DURATION);
                    needsRedraw = true;
                }
                if (playerKO)
                {
                    playerSpriteAlpha = Math.Max(0f, playerSpriteAlpha - dt / KO_FADE_DURATION);
                    needsRedraw = true;
                }

                if ((wildKO && wildSpriteAlpha <= 0f) || (playerKO && playerSpriteAlpha <= 0f))
                {
                    // After fade completes, end encounter, possibly level up already handled
                    EndWildEncounter();
                }
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

        // Create a smaller version of the Pokeball for use in the battle interface
        string smallPokeballPath = Path.Combine(texturesDirectory, "Pokemon/Pokeball.png");
        if (File.Exists(pokeballPath))
        {
            try
            {
                smallPokeballImage = Image.FromFile(smallPokeballPath);
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
    // (Moved Pokeball indicator into the moves box as a Catch button)

        // Draw wild Pokemon on the right side of screen
        if (wildPokemon != null && wildPokemonImage != null)
        {
            float pokemonSize = ClientSize.Height * 0.3f; // 30% of screen height
            float x = ClientSize.Width * 0.75f - pokemonSize / 2; // Right side, more to the right
            float y = ClientSize.Height * 0.25f; // Upper portion of screen
            
            // Use nearest neighbor interpolation for sharp pixel art
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
            
            // Draw with alpha (for fade on KO)
            DrawImageWithAlpha(g, wildPokemonImage, new RectangleF(x, y, pokemonSize, pokemonSize), wildSpriteAlpha);
            
            // Draw wild Pokemon health bar above the Pokemon
            DrawPokemonHealthBar(g, wildPokemon, x, y - 60, pokemonSize, true);
        }

        // Draw user's Pokemon on the left side if selected (same height as wild Pokemon)
        if (selectedUserPokemon != null && selectedUserPokemonImage != null && !showPokemonSelection)
        {
            float pokemonSize = ClientSize.Height * 0.3f; // Same size as wild Pokemon
            float x = ClientSize.Width * 0.25f - pokemonSize / 2; // Left side, centered
            float y = ClientSize.Height * 0.25f; // Same height as wild Pokemon
            
            // Use nearest neighbor interpolation for sharp pixel art
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
            
            // Draw with alpha (for fade on KO)
            DrawImageWithAlpha(g, selectedUserPokemonImage, new RectangleF(x, y, pokemonSize, pokemonSize), playerSpriteAlpha);
            
            // Draw user Pokemon health bar above the Pokemon
            DrawPokemonHealthBar(g, selectedUserPokemon, x, y - 60, pokemonSize, false);
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
    /// Draw an image with specified alpha using ColorMatrix
    /// </summary>
    private void DrawImageWithAlpha(Graphics g, Image image, RectangleF destRect, float alpha)
    {
        if (image == null) return;
        using var attributes = new ImageAttributes();
        var matrix = new ColorMatrix
        {
            Matrix00 = 1f,
            Matrix11 = 1f,
            Matrix22 = 1f,
            Matrix33 = Math.Clamp(alpha, 0f, 1f), // Alpha
            Matrix44 = 1f
        };
        attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
        g.DrawImage(image, Rectangle.Round(destRect), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
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
        // Determine current and max HP based on side
        int maxHp = isWild ? wildMaxHP : playerMaxHP;
        int curHp = isWild ? wildCurrentHP : playerCurrentHP;
        if (maxHp <= 0) maxHp = Math.Max(1, pokemon.HP); // safety fallback
        curHp = Math.Clamp(curHp, 0, maxHp);

        float healthPercentage = (float)curHp / maxHp;
        
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

        // Draw current health (green/yellow/red based on thresholds)
        Color healthColor = healthPercentage > 0.5f ? Color.Green : 
                           healthPercentage > 0.2f ? Color.Yellow : Color.Red;
        
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
        string healthText = $"{curHp}/{maxHp}";
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

        // Draw Pokeball catch button in the top-right of the moves box
        float indicatorSize = 50f;
        float circleX = boxX + boxWidth - indicatorSize - 15f;
        float circleY = boxY + 10f;
        catchButtonRect = new RectangleF(circleX, circleY, indicatorSize, indicatorSize);
        using (Brush circleBrush = new SolidBrush(Color.White)) g.FillEllipse(circleBrush, catchButtonRect);
        using (Pen circlePen = new Pen(Color.Black, 2)) g.DrawEllipse(circlePen, catchButtonRect);
        if (pokeballImage != null)
        {
            float pbSize = indicatorSize * 0.7f;
            float pbX = circleX + (indicatorSize - pbSize) / 2f;
            float pbY = circleY + (indicatorSize - pbSize) / 2f;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
            g.DrawImage(pokeballImage, pbX, pbY, pbSize, pbSize);
        }
        // Optional small label
        using (Font catchFont = new Font("Arial", 9, FontStyle.Bold))
        using (Brush catchBrush = new SolidBrush(Color.Black))
        {
            var txt = "Catch";
            var size = g.MeasureString(txt, catchFont);
            g.DrawString(txt, catchFont, catchBrush, circleX + (indicatorSize - size.Width) / 2f, circleY + indicatorSize + 2f);
        }

        // If currently announcing the turn, draw announcement text instead of options
        if (isAnnouncingTurn)
        {
            using (Font titleFont = new Font("Arial", 16, FontStyle.Bold))
            using (Brush titleBrush = new SolidBrush(Color.Black))
            {
                string title = "Battle Log";
                var titleSize = g.MeasureString(title, titleFont);
                g.DrawString(title, titleFont, titleBrush, 
                    boxX + (boxWidth - titleSize.Width) / 2, boxY + 10);
            }

            using (Font msgFont = new Font("Arial", 11))
            using (Brush msgBrush = new SolidBrush(Color.Black))
            {
                float announceStartY = boxY + 50;
                float lineHeight = 22f;
                foreach (var line in turnAnnouncements)
                {
                    g.DrawString(line, msgFont, msgBrush, boxX + 20, announceStartY);
                    announceStartY += lineHeight;
                }

                string continueText = "Click or press any key to continue...";
                using (Font contFont = new Font("Arial", 10, FontStyle.Italic))
                using (Brush contBrush = new SolidBrush(Color.DarkBlue))
                {
                    var size = g.MeasureString(continueText, contFont);
                    g.DrawString(continueText, contFont, contBrush, boxX + (boxWidth - size.Width) / 2, boxY + boxHeight - 25);
                }
            }
            return;
        }

        // Draw title
        using (Font titleFont = new Font("Arial", 16, FontStyle.Bold))
        using (Brush titleBrush = new SolidBrush(Color.Black))
        {
            string title = "Choose a Move";
            var titleSize = g.MeasureString(title, titleFont);
            g.DrawString(title, titleFont, titleBrush, 
                boxX + (boxWidth - titleSize.Width) / 2, boxY + 10);
        }

        // Draw move buttons in 2x2 grid
        float buttonWidth = (boxWidth - 60) / 2;  // 2 columns with padding
        float buttonHeight = 50;
        float buttonSpacing = 20;
        float startX = boxX + 20;
        float startY = boxY + 50;

        for (int i = 0; i < pokemonMoves.Length; i++)
        {
            int row = i / 2;
            int col = i % 2;
            
            float buttonX = startX + col * (buttonWidth + buttonSpacing);
            float buttonY = startY + row * (buttonHeight + buttonSpacing);

            // Determine button color based on state
            Color buttonColor = Color.White;
            Color textColor = Color.Black;
            
            if (i == selectedMoveIndex)
            {
                buttonColor = Color.LightBlue;
                textColor = Color.DarkBlue;
            }
            else if (i == hoveredMoveIndex)
            {
                buttonColor = Color.LightYellow;
                textColor = Color.DarkOrange;
            }

            // Draw button background
            using (Brush buttonBrush = new SolidBrush(buttonColor))
            {
                g.FillRectangle(buttonBrush, buttonX, buttonY, buttonWidth, buttonHeight);
            }

            // Draw button border
            using (Pen buttonPen = new Pen(Color.Gray, 2))
            {
                g.DrawRectangle(buttonPen, buttonX, buttonY, buttonWidth, buttonHeight);
            }

            // Draw move name
            using (Font moveFont = new Font("Arial", 12, FontStyle.Bold))
            using (Brush textBrush = new SolidBrush(textColor))
            {
                var textSize = g.MeasureString(pokemonMoves[i], moveFont);
                float textX = buttonX + (buttonWidth - textSize.Width) / 2;
                float textY = buttonY + 8;
                g.DrawString(pokemonMoves[i], moveFont, textBrush, textX, textY);
            }

            // Draw move description
            using (Font descFont = new Font("Arial", 8))
            using (Brush descBrush = new SolidBrush(Color.DarkGray))
            {
                var descSize = g.MeasureString(moveDescriptions[i], descFont);
                float descX = buttonX + (buttonWidth - descSize.Width) / 2;
                float descY = buttonY + 28;
                g.DrawString(moveDescriptions[i], descFont, descBrush, descX, descY);
            }
        }

        // Draw instructions
        using (Font instructFont = new Font("Arial", 10))
        using (Brush instructBrush = new SolidBrush(Color.DarkBlue))
        {
            string instructions = "Click on a move to select it | Arrow keys to navigate | Enter to confirm";
            var instructSize = g.MeasureString(instructions, instructFont);
            g.DrawString(instructions, instructFont, instructBrush, 
                boxX + (boxWidth - instructSize.Width) / 2, boxY + boxHeight - 25);
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
                // Initialize wild HP and state
                wildMaxHP = Math.Max(1, wildPokemon.HP);
                wildCurrentHP = wildMaxHP;
                wildKO = false; wildSpriteAlpha = 1f;
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
            wildMaxHP = Math.Max(1, wildPokemon.HP);
            wildCurrentHP = wildMaxHP;
            wildKO = false; wildSpriteAlpha = 1f;

            if (currentUser != null)
            {
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

        // Initialize player HP and state when image is loaded (i.e., selection confirmed)
        playerMaxHP = Math.Max(1, selectedUserPokemon.HP);
        playerCurrentHP = playerMaxHP;
        playerKO = false; playerSpriteAlpha = 1f;
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
            
            // Use high quality rendering for placeholder creation
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            
            // Draw a simple circle
            using (Brush brush = new SolidBrush(Color.DarkGray))
            {
                g.FillEllipse(brush, 20, 20, size - 40, size - 40);
            }
            
            // Draw Pokemon name with clear text
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

        // If announcing and a KO occurred, ignore input and let fade/end proceed
        if (inWildEncounter && encounterPhase == EncounterPhase.BattleReady && isAnnouncingTurn && (wildKO || playerKO))
        {
            return;
        }

        // If announcing, any key continues to next turn (restore move options)
        if (inWildEncounter && encounterPhase == EncounterPhase.BattleReady && isAnnouncingTurn)
        {
            isAnnouncingTurn = false;
            playerProtectedThisTurn = false;
            wildProtectedThisTurn = false;
            selectedMoveIndex = -1;
            hoveredMoveIndex = -1;
            needsRedraw = true;
            return;
        }

        // Handle battle interactions if in encounter (keyboard controls still supported as backup)
        if (inWildEncounter && encounterPhase == EncounterPhase.PokemonSelection)
        {
            HandlePokemonSelectionInput(e);
            return;
        }
        // Handle move selection during battle
        else if (inWildEncounter && encounterPhase == EncounterPhase.BattleReady && !showPokemonSelection)
        {
            HandleMoveSelectionInput(e);
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
    /// Handles input during move selection phase
    /// </summary>
    private void HandleMoveSelectionInput(KeyEventArgs e)
    {
        // Ignore input when announcing
        if (isAnnouncingTurn) return;

        switch (e.KeyCode)
        {
            case Keys.Up or Keys.W:
                // Move up in 2x2 grid
                if (selectedMoveIndex == -1) selectedMoveIndex = 0;
                else if (selectedMoveIndex >= 2) selectedMoveIndex -= 2; // Move up one row
                needsRedraw = true;
                break;
                
            case Keys.Down or Keys.S:
                // Move down in 2x2 grid
                if (selectedMoveIndex == -1) selectedMoveIndex = 0;
                else if (selectedMoveIndex < 2) selectedMoveIndex += 2; // Move down one row
                needsRedraw = true;
                break;
                
            case Keys.Left or Keys.A:
                // Move left in 2x2 grid
                if (selectedMoveIndex == -1) selectedMoveIndex = 0;
                else if (selectedMoveIndex % 2 == 1) selectedMoveIndex -= 1; // Move left if not already in left column
                needsRedraw = true;
                break;
                
            case Keys.Right or Keys.D:
                // Move right in 2x2 grid
                if (selectedMoveIndex == -1) selectedMoveIndex = 0;
                else if (selectedMoveIndex % 2 == 0) selectedMoveIndex += 1; // Move right if not already in right column
                needsRedraw = true;
                break;
                
            case Keys.Enter or Keys.Space:
                // Select the current move
                if (selectedMoveIndex >= 0 && selectedMoveIndex < pokemonMoves.Length)
                {
                    playerSelectedMoveName = pokemonMoves[selectedMoveIndex];
                    StartBattleTurn();
                }
                break;
                
            case Keys.Escape:
                // Deselect move
                selectedMoveIndex = -1;
                needsRedraw = true;
                break;
        }
    }

    /// <summary>
    /// Handles mouse click events for Pokemon selection
    /// </summary>
    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);

        // If announcing and a KO occurred, ignore input and let fade/end proceed
        if (inWildEncounter && encounterPhase == EncounterPhase.BattleReady && isAnnouncingTurn && (wildKO || playerKO))
        {
            return;
        }

        // If announcing, handle special catch resolution first; otherwise restore move options
        if (inWildEncounter && encounterPhase == EncounterPhase.BattleReady && isAnnouncingTurn)
        {
            if (catchPendingResolution)
            {
                // Resolve catch based on wild HP
                bool caught = (wildPokemon != null && wildMaxHP > 0 && wildCurrentHP <= (int)(wildMaxHP * 0.3f));
                catchPendingResolution = false;

                if (caught && wildPokemon != null)
                {
                    // Save to DB if possible
                    if (currentUser != null)
                    {
                        try { new PokedexDB().AddPokemonToUser(currentUser.UserId, wildPokemon.Name); } catch { /* ignore */ }
                    }
                    // Remove Pokeball from battle screen by ending encounter
                    wildTextureIsPokeball = false;
                    wildPokemonImageOriginal = null;
                    EndWildEncounter();
                    return;
                }
                else
                {
                    // Restore original texture and resume battle
                    if (wildPokemonImageOriginal != null)
                    {
                        wildPokemonImage = wildPokemonImageOriginal;
                    }
                    wildTextureIsPokeball = false;
                    isAnnouncingTurn = false;
                    playerProtectedThisTurn = false;
                    wildProtectedThisTurn = false;
                    selectedMoveIndex = -1;
                    hoveredMoveIndex = -1;
                    needsRedraw = true;
                    return;
                }
            }
            else
            {
                isAnnouncingTurn = false;
                playerProtectedThisTurn = false;
                wildProtectedThisTurn = false;
                selectedMoveIndex = -1;
                hoveredMoveIndex = -1;
                needsRedraw = true;
                return;
            }
        }

        // Handle Pokemon selection with mouse clicks
        if (inWildEncounter && encounterPhase == EncounterPhase.PokemonSelection && showPokemonSelection && userPokemon != null)
        {
            HandlePokemonSelectionMouseClick(e);
        }
        // Handle move selection with mouse clicks
        else if (inWildEncounter && encounterPhase == EncounterPhase.BattleReady && !showPokemonSelection)
        {
            HandleMoveSelectionMouseClick(e);
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
        // Handle move selection highlighting with mouse hover
        else if (inWildEncounter && encounterPhase == EncounterPhase.BattleReady && !showPokemonSelection)
        {
            HandleMoveSelectionMouseMove(e);
        }
    }

    /// <summary>
    /// Handles mouse clicks during move selection phase
    /// </summary>
    private void HandleMoveSelectionMouseClick(MouseEventArgs e)
    {
        // Ignore clicks when announcing
        if (isAnnouncingTurn) return;

        // Catch button click
        if (!catchButtonRect.IsEmpty && catchButtonRect.Contains(e.Location) && wildPokemon != null && encounterPhase == EncounterPhase.BattleReady)
        {
            // Swap wild texture to Pokeball and announce
            wildPokemonImageOriginal ??= wildPokemonImage;
            if (smallPokeballImage != null)
            {
                wildPokemonImage = smallPokeballImage;
                wildTextureIsPokeball = true;
            }
            turnAnnouncements.Clear();
            turnAnnouncements.Add("You threw the Pokeball.");
            isAnnouncingTurn = true;
            catchPendingResolution = true;
            needsRedraw = true;
            return;
        }

        // Calculate moves box bounds
        float boxWidth = ClientSize.Width * 0.6f;
        float boxHeight = ClientSize.Height * 0.25f;
        float boxX = (ClientSize.Width - boxWidth) / 2;
        float boxY = ClientSize.Height - boxHeight - 20;

        // Check if click is within the moves box
        if (e.X >= boxX && e.X <= boxX + boxWidth && e.Y >= boxY && e.Y <= boxY + boxHeight)
        {
            // Calculate move button bounds
            float buttonWidth = (boxWidth - 60) / 2;
            float buttonHeight = 50;
            float buttonSpacing = 20;
            float startX = boxX + 20;
            float startY = boxY + 50;

            for (int i = 0; i < pokemonMoves.Length; i++)
            {
                int row = i / 2;
                int col = i % 2;
                
                float buttonX = startX + col * (buttonWidth + buttonSpacing);
                float buttonY = startY + row * (buttonHeight + buttonSpacing);

                // Check if click is within this button
                if (e.X >= buttonX && e.X <= buttonX + buttonWidth && 
                    e.Y >= buttonY && e.Y <= buttonY + buttonHeight)
                {
                    // Move was clicked - select it
                    selectedMoveIndex = i;
                    playerSelectedMoveName = pokemonMoves[i];
                    needsRedraw = true;
                    
                    StartBattleTurn();
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Handles mouse movement during move selection phase for highlighting
    /// </summary>
    private void HandleMoveSelectionMouseMove(MouseEventArgs e)
    {
        // Ignore hover when announcing
        if (isAnnouncingTurn) return;

        // Calculate moves box bounds
        float boxWidth = ClientSize.Width * 0.6f;
        float boxHeight = ClientSize.Height * 0.25f;
        float boxX = (ClientSize.Width - boxWidth) / 2;
        float boxY = ClientSize.Height - boxHeight - 20;

        int newHoveredIndex = -1;

        // Check if mouse is within the moves box
        if (e.X >= boxX && e.X <= boxX + boxWidth && e.Y >= boxY && e.Y <= boxY + boxHeight)
        {
            // Calculate move button bounds
            float buttonWidth = (boxWidth - 60) / 2;
            float buttonHeight = 50;
            float buttonSpacing = 20;
            float startX = boxX + 20;
            float startY = boxY + 50;

            for (int i = 0; i < pokemonMoves.Length; i++)
            {
                int row = i / 2;
                int col = i % 2;
                
                float buttonX = startX + col * (buttonWidth + buttonSpacing);
                float buttonY = startY + row * (buttonHeight + buttonSpacing);

                // Check if mouse is within this button
                if (e.X >= buttonX && e.X <= buttonX + buttonWidth && 
                    e.Y >= buttonY && e.Y <= buttonY + buttonHeight)
                {
                    newHoveredIndex = i;
                    break;
                }
            }
        }

        // Update hover state if it changed
        if (newHoveredIndex != hoveredMoveIndex)
        {
            hoveredMoveIndex = newHoveredIndex;
            needsRedraw = true;
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

            // Use nearest neighbor for sharp Pokeball rendering
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
            
            g.DrawImage(pokeballImage, pokeballX, pokeballY, pokeballSize, pokeballSize);
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

    // ===================== Wild battle move system =====================

    /// <summary>
    /// Starts and resolves a single turn: player selects a move, enemy chooses randomly,
    /// determines order by Speed (unless slower uses Protect), executes both, and announces.
    /// </summary>
    private void StartBattleTurn()
    {
        if (!inWildEncounter || encounterPhase != EncounterPhase.BattleReady || selectedUserPokemon == null || wildPokemon == null)
            return;

        // Store enemy move selection
        wildSelectedMoveName = GetRandomEnemyMove();

        // Reset turn state
        turnAnnouncements.Clear();
        playerProtectedThisTurn = false;
        wildProtectedThisTurn = false;

        // Determine who is faster (higher Speed goes first)
        bool playerFaster = selectedUserPokemon.Speed >= wildPokemon.Speed;

        // If slower uses Protect, it goes first
        bool playerIsSlower = selectedUserPokemon.Speed < wildPokemon.Speed;
        bool wildIsSlower = wildPokemon.Speed < selectedUserPokemon.Speed;

        if (playerIsSlower && string.Equals(playerSelectedMoveName, "Protect", StringComparison.OrdinalIgnoreCase))
        {
            playerFaster = true; // Force player to act first
        }
        else if (wildIsSlower && string.Equals(wildSelectedMoveName, "Protect", StringComparison.OrdinalIgnoreCase))
        {
            playerFaster = false; // Force wild to act first
        }

        // Execute first mover
        if (playerFaster)
        {
            ExecuteMove(selectedUserPokemon, wildPokemon, playerSelectedMoveName, isPlayer: true);
            ExecuteMove(wildPokemon, selectedUserPokemon, wildSelectedMoveName, isPlayer: false);
        }
        else
        {
            ExecuteMove(wildPokemon, selectedUserPokemon, wildSelectedMoveName, isPlayer: false);
            ExecuteMove(selectedUserPokemon, wildPokemon, playerSelectedMoveName, isPlayer: true);
        }

        // KO checks and announcements
        if (wildCurrentHP <= 0 && !wildKO)
        {
            wildCurrentHP = 0; wildKO = true;
            turnAnnouncements.Add($"Wild {wildPokemon.Name} fainted!");
            // 20% level up chance
            if (encounterRandom.NextDouble() <= 0.20)
            {
                selectedUserPokemon.Level += 1;
                turnAnnouncements.Add($"{selectedUserPokemon.Name} leveled up to Lv.{selectedUserPokemon.Level}!");
                
                // Update the level in the database if the user is logged in
                if (currentUser != null)
                {
                    try
                    {
                        PokedexDB db = new PokedexDB();
                        if (db.UpdatePokemonLevel(currentUser.UserId, selectedUserPokemon.Name, selectedUserPokemon.Level))
                        {
                            turnAnnouncements.Add("Progress saved!");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error saving Pokemon level: {ex.Message}");
                    }
                }
            }
        }
        if (playerCurrentHP <= 0 && !playerKO)
        {
            playerCurrentHP = 0; playerKO = true;
            turnAnnouncements.Add($"{selectedUserPokemon.Name} fainted!");
        }

        // Show announcements in move box
        isAnnouncingTurn = true;
        needsRedraw = true;
    }

    /// <summary>
    /// Execute a move by name for the given attacker and target and add announcer text.
    /// </summary>
    private void ExecuteMove(Pokemon attacker, Pokemon target, string moveName, bool isPlayer)
    {
        // Announce the move
        turnAnnouncements.Add($"{attacker.Name} used {moveName}!");

        switch (moveName)
        {
            case "Tackle":
                UseTackle(attacker, target);
                break;
            case "Projectile":
                UseProjectile(attacker, target);
                break;
            case "Protect":
                UseProtect(attacker, target);
                break;
            case "Rest":
                UseRest(attacker, target);
                break;
            default:
                // Unknown move (shouldn't happen)
                break;
        }
    }

    /// <summary>
    /// Randomly selects an enemy move and stores its name.
    /// </summary>
    /// <returns>The selected move name.</returns>
    private string GetRandomEnemyMove()
    {
        string name = pokemonMoves[encounterRandom.Next(pokemonMoves.Length)];
        wildSelectedMoveName = name;
        return name;
    }

    // ---- Individual move implementations ----

    /// <summary>
    /// A basic physical attack.
    /// </summary>
    private void UseTackle(Pokemon attacker, Pokemon target)
    {
        // If target protected this turn, it blocks the attack
        if (IsProtected(target))
        {
            turnAnnouncements.Add($"But {target.Name} protected itself!");
            return;
        }

        // Simple damage formula using Attack/Defense
        int damage = Math.Max(1, ((attacker.Attack * 2) - target.Defense) / 5);
        ApplyDamage(target, damage);
        turnAnnouncements.Add($"It hits {target.Name} for {damage} damage!");
    }

    /// <summary>
    /// A ranged attack using special stats.
    /// </summary>
    private void UseProjectile(Pokemon attacker, Pokemon target)
    {
        if (IsProtected(target))
        {
            turnAnnouncements.Add($"But {target.Name} protected itself!");
            return;
        }
        int damage = Math.Max(1, ((attacker.SpAttack * 2) - target.SpDefense) / 5);
        ApplyDamage(target, damage);
        turnAnnouncements.Add($"A projectile strikes {target.Name} for {damage} damage!");
    }

    /// <summary>
    /// Protect blocks incoming damage for this turn.
    /// </summary>
    private void UseProtect(Pokemon attacker, Pokemon target)
    {
        if (selectedUserPokemon != null && ReferenceEquals(attacker, selectedUserPokemon))
        {
            playerProtectedThisTurn = true;
        }
        else if (wildPokemon != null && ReferenceEquals(attacker, wildPokemon))
        {
            wildProtectedThisTurn = true;
        }
        turnAnnouncements.Add($"{attacker.Name} is shielding itself!");
    }

    /// <summary>
    /// Rest recovers slightly.
    /// </summary>
    private void UseRest(Pokemon attacker, Pokemon target)
    {
        int healAmount;
        if (selectedUserPokemon != null && ReferenceEquals(attacker, selectedUserPokemon))
        {
            healAmount = Math.Max(1, (int)(playerMaxHP * 0.2f));
            int before = playerCurrentHP;
            playerCurrentHP = Math.Min(playerMaxHP, playerCurrentHP + healAmount);
            turnAnnouncements.Add($"{attacker.Name} recovers {playerCurrentHP - before} HP.");
        }
        else if (wildPokemon != null && ReferenceEquals(attacker, wildPokemon))
        {
            healAmount = Math.Max(1, (int)(wildMaxHP * 0.2f));
            int before = wildCurrentHP;
            wildCurrentHP = Math.Min(wildMaxHP, wildCurrentHP + healAmount);
            turnAnnouncements.Add($"{attacker.Name} recovers {wildCurrentHP - before} HP.");
        }
    }

    /// <summary>
    /// Returns whether the specified Pokemon is protected for this turn.
    /// </summary>
    private bool IsProtected(Pokemon p)
    {
        if (selectedUserPokemon != null && ReferenceEquals(p, selectedUserPokemon))
            return playerProtectedThisTurn;
        if (wildPokemon != null && ReferenceEquals(p, wildPokemon))
            return wildProtectedThisTurn;
        return false;
    }

    /// <summary>
    /// Apply damage to the target's current HP and clamp to 0.
    /// </summary>
    private void ApplyDamage(Pokemon target, int damage)
    {
        damage = Math.Max(0, damage);
        if (wildPokemon != null && ReferenceEquals(target, wildPokemon))
        {
            wildCurrentHP = Math.Max(0, wildCurrentHP - damage);
        }
        else if (selectedUserPokemon != null && ReferenceEquals(target, selectedUserPokemon))
        {
            playerCurrentHP = Math.Max(0, playerCurrentHP - damage);
        }
    }
}
