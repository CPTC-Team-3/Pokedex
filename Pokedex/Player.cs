namespace Pokedex;

/// <summary>
/// Represents a player in the game world with movement capabilities
/// </summary>
public class Player
{
    /// <summary>
    /// The player's X position in the grid.
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// The player's Y position in the grid.
    /// </summary>
    public int Y { get; set; }

    /// <summary>
    /// The size of the player in pixels.
    /// </summary>
    public int Size = 60;

    /// <summary>
    /// The actual X coordinate for smooth movement interpolation
    /// </summary>
    public float ActualX { get; private set; }

    /// <summary>
    /// The actual Y coordinate for smooth movement interpolation
    /// </summary>
    public float ActualY { get; private set; }

    /// <summary>
    /// Indicates whether the player is currently in motion
    /// </summary>
    public bool IsMoving { get; private set; }

    /// <summary>
    /// Base movement speed in pixels per frame
    /// </summary>
    public float MovementSpeed { get; private set; } = 5.0f;

    /// <summary>
    /// The sprite sheet image for the player character.
    /// </summary>
    public Image? SpriteSheet { get; set; }

    /// <summary>
    /// Width of each sprite frame in the sprite sheet.
    /// </summary>
    public int SpriteWidth { get; set; } = 32;

    /// <summary>
    /// Height of each sprite frame in the sprite sheet.
    /// </summary>
    public int SpriteHeight { get; set; } = 32;

    /// <summary>
    /// Current animation frame index (0-3).
    /// </summary>
    private int currentFrame = 0;

    /// <summary>
    /// Animation frame counter for timing.
    /// </summary>
    private int animationCounter = 0;

    /// <summary>
    /// Frames between animation updates.
    /// </summary>
    private int animationSpeed = 4;

    /// <summary>
    /// Current facing direction for sprite selection.
    /// </summary>
    public Direction CurrentDirection { get; private set; } = Direction.Down;

    /// <summary>
    /// Target X coordinate for smooth movement
    /// </summary>
    private float targetX;

    /// <summary>
    /// Target Y coordinate for smooth movement
    /// </summary>
    private float targetY;

    /// <summary>
    /// Current speed multiplier affected by tile properties
    /// </summary>
    private float currentSpeedMultiplier = 1.0f;

    /// <summary>
    /// Buffer storing current pressed movement keys
    /// </summary>
    private string keyBuffer = string.Empty;

    /// <summary>
    /// Current zoom level for the player sprite. 1.0 is normal size, 2.0 is double zoom, 0.5 is half zoom.
    /// </summary>
    public float ZoomLevel { get; set; } = 1.5f;

    /// <summary>
    /// Minimum allowed zoom level to prevent texture issues
    /// </summary>
    public float MinZoomLevel { get; set; } = 0.5f;

    /// <summary>
    /// Maximum allowed zoom level to prevent texture issues
    /// </summary>
    public float MaxZoomLevel { get; set; } = 3.0f;

    /// <summary>
    /// Enumeration for player facing directions
    /// </summary>
    public enum Direction
    {
        Down = 0,   // Row 0 in sprite sheet (South)
        East = 1,   // Row 1 in sprite sheet (East)
        Up = 2,     // Row 2 in sprite sheet (North)
        West = 3    // West uses East sprites but mirrored
    }

    /// <summary>
    /// The constructor for the Player class.
    /// </summary>
    /// <param name="startX">The starting X position of the player</param>
    /// <param name="startY">The starting Y position of the player</param>
    /// <param name="size">The size of the player in pixels</param>
    public Player(int startX, int startY, int size)
    {
        X = startX;
        Y = startY;
        ActualX = startX;
        ActualY = startY;
        targetX = startX;
        targetY = startY;
        Size = size;
    }

    /// <summary>
    /// Sets the sprite sheet from a file path.
    /// </summary>
    /// <param name="spriteSheetPath">Path to the sprite sheet file</param>
    /// <param name="spriteWidth">Width of each sprite frame</param>
    /// <param name="spriteHeight">Height of each sprite frame</param>
    /// <returns>True if sprite sheet was loaded successfully, false otherwise</returns>
    public bool SetSpriteSheet(string spriteSheetPath, int spriteWidth = 32, int spriteHeight = 32)
    {
        try
        {
            if (File.Exists(spriteSheetPath))
            {
                SpriteSheet = Image.FromFile(spriteSheetPath);
                SpriteWidth = spriteWidth;
                SpriteHeight = spriteHeight;
                return true;
            }
        }
        catch (Exception)
        {
            // If sprite sheet loading fails, keep the existing sprite sheet (or null)
        }
        return false;
    }

    /// <summary>
    /// Gets whether this player has a valid sprite sheet.
    /// </summary>
    public bool HasSpriteSheet => SpriteSheet != null;

    /// <summary>
    /// Updates the animation frame based on movement state.
    /// </summary>
    public void UpdateAnimation()
    {
        if (IsMoving)
        {
            animationCounter++;
            if (animationCounter >= animationSpeed)
            {
                animationCounter = 0;
                currentFrame = (currentFrame + 1) % 4;
            }
        }
        else
        {
            // When not moving, stay on idle frame (frame 0)
            currentFrame = 0;
            animationCounter = 0;
        }
    }

    /// <summary>
    /// Sets the zoom level for the player sprite texture
    /// </summary>
    /// <param name="zoomLevel">The zoom level (1.0 is normal, higher values zoom in, lower values zoom out)</param>
    public void SetZoomLevel(float zoomLevel)
    {
        ZoomLevel = Math.Clamp(zoomLevel, MinZoomLevel, MaxZoomLevel);
    }

    /// <summary>
    /// Increases the zoom level by the specified amount
    /// </summary>
    /// <param name="amount">Amount to increase zoom (default: 0.1)</param>
    public void ZoomIn(float amount = 0.1f)
    {
        SetZoomLevel(ZoomLevel + amount);
    }

    /// <summary>
    /// Decreases the zoom level by the specified amount
    /// </summary>
    /// <param name="amount">Amount to decrease zoom (default: 0.1)</param>
    public void ZoomOut(float amount = 0.1f)
    {
        SetZoomLevel(ZoomLevel - amount);
    }

    /// <summary>
    /// Gets the current sprite frame rectangle from the sprite sheet with zoom cropping applied.
    /// The cropping maintains the same output size while showing a zoomed portion of the texture.
    /// </summary>
    /// <returns>Rectangle representing the current frame in the sprite sheet with zoom cropping</returns>
    public Rectangle GetCurrentSpriteFrame()
    {
        int row = CurrentDirection == Direction.West ? (int)Direction.East : (int)CurrentDirection;
        int frame = currentFrame;
        
        // Map the 4-frame walking animation (0,1,2,3) to your 3-column sprite sheet
        // Correct mapping based on your sprite layout:
        // - Left column (0): Idle frame
        // - Middle column (1): First step
        // - Right column (2): Second step
        int spriteColumn = frame switch
        {
            0 => 0, // Idle pose - use left column (column 0)
            1 => 1, // First step - use middle column (column 1)  
            2 => 0, // Back to idle - use left column (column 0)
            3 => 2, // Second step - use right column (column 2)
            _ => 0  // Default to idle (left column)
        };

        // Calculate the base sprite rectangle
        Rectangle baseSpriteRect = new Rectangle(
            spriteColumn * (int)((double)SpriteWidth / 1.55) - 5, 
            row * (int)((double)SpriteHeight / 1.5) - 2, 
            SpriteWidth, 
            SpriteHeight
        );

        // Apply zoom cropping if zoom level is not 1.0
        if (Math.Abs(ZoomLevel - 1.0f) > 0.001f)
        {
            return GetZoomedSpriteFrame(baseSpriteRect);
        }

        return baseSpriteRect;
    }

    /// <summary>
    /// Calculates the cropped sprite frame rectangle based on the current zoom level.
    /// Higher zoom levels crop more from the edges to create a zoom effect.
    /// </summary>
    /// <param name="baseSpriteRect">The original sprite frame rectangle</param>
    /// <returns>Cropped rectangle that creates the zoom effect</returns>
    private Rectangle GetZoomedSpriteFrame(Rectangle baseSpriteRect)
    {
        // Calculate the crop amount based on zoom level
        // When zoom > 1.0, we crop from edges to show a smaller portion of the texture
        // When zoom < 1.0, we expand the view (limited by sprite boundaries)
        
        float cropFactor = 1.0f / ZoomLevel;
        
        // Calculate new dimensions (smaller when zoomed in)
        int croppedWidth = (int)(baseSpriteRect.Width * cropFactor);
        int croppedHeight = (int)(baseSpriteRect.Height * cropFactor);
        
        // Ensure minimum size to prevent issues
        croppedWidth = Math.Max(croppedWidth, 1);
        croppedHeight = Math.Max(croppedHeight, 1);
        
        // Calculate centering offsets to crop from all edges equally
        int offsetX = (baseSpriteRect.Width - croppedWidth) / 2;
        int offsetY = (baseSpriteRect.Height - croppedHeight) / 2;
        
        // Create the cropped rectangle
        Rectangle croppedRect = new Rectangle(
            baseSpriteRect.X + offsetX,
            baseSpriteRect.Y + offsetY,
            croppedWidth,
            croppedHeight
        );
        
        // Ensure the cropped rectangle stays within the original sprite bounds
        croppedRect.X = Math.Max(croppedRect.X, baseSpriteRect.X);
        croppedRect.Y = Math.Max(croppedRect.Y, baseSpriteRect.Y);
        croppedRect.Width = Math.Min(croppedRect.Width, baseSpriteRect.Right - croppedRect.X);
        croppedRect.Height = Math.Min(croppedRect.Height, baseSpriteRect.Bottom - croppedRect.Y);
        
        return croppedRect;
    }

    /// <summary>
    /// Gets whether the sprite should be flipped horizontally (for West direction).
    /// </summary>
    public bool ShouldFlipHorizontally => CurrentDirection == Direction.West;

    /// <summary>
    /// Gets the current zoom level for debugging purposes
    /// </summary>
    /// <returns>The current zoom level value</returns>
    public float GetZoomLevel()
    {
        return ZoomLevel;
    }

    /// <summary>
    /// Adds a key to the movement buffer
    /// </summary>
    /// <param name="key">The key to add (must be W, A, S, or D)</param>
    public void AddKeyToBuffer(char key)
    {
        key = char.ToUpper(key);
        if (!"WASD".Contains(key)) return;
        
        // Only add the key if it's not already in the buffer
        if (!keyBuffer.Contains(key))
        {
            keyBuffer += key;
        }
    }

    /// <summary>
    /// Removes a key from the movement buffer
    /// </summary>
    /// <param name="key">The key to remove</param>
    public void RemoveKeyFromBuffer(char key)
    {
        key = char.ToUpper(key);
        keyBuffer = keyBuffer.Replace(key.ToString(), "");
    }

    /// <summary>
    /// Attempts to move the player based on the first key in the buffer
    /// </summary>
    /// <param name="tiles">The list of tiles in the game</param>
    /// <returns>True if movement was initiated, false otherwise</returns>
    private bool TryMoveFromBuffer(List<Tile> tiles)
    {
        if (string.IsNullOrEmpty(keyBuffer) || IsMoving) return false;

        int deltaX = 0, deltaY = 0;
        char firstKey = keyBuffer[0];

        switch (firstKey)
        {
            case 'W':
                deltaY = -1;
                CurrentDirection = Direction.Up;
                break;
            case 'S':
                deltaY = 1;
                CurrentDirection = Direction.Down;
                break;
            case 'A':
                deltaX = -1;
                CurrentDirection = Direction.West;
                break;
            case 'D':
                deltaX = 1;
                CurrentDirection = Direction.East;
                break;
        }

        return Move(deltaX, deltaY, tiles);
    }

    /// <summary>
    /// Internal movement logic
    /// </summary>
    private bool Move(int deltaX, int deltaY, List<Tile> tiles)
    {
        if (IsMoving) return false;

        int newX = X + (deltaX * Size);
        int newY = Y + (deltaY * Size);

        Tile? targetTile = tiles.Find(t => t.X == newX && t.Y == newY);
        Tile? currentTile = tiles.Find(t => t.X == X && t.Y == Y);

        if (targetTile != null && targetTile.IsWalkable)
        {
            // Set the speed multiplier based on the current tile's speed
            currentSpeedMultiplier = currentTile?.Speed ?? 1.0f;
            
            // Update grid position
            X = newX;
            Y = newY;
            
            // Set target for smooth movement
            targetX = newX;
            targetY = newY;
            IsMoving = true;
            
            return true;
        }

        return false;
    }

    /// <summary>
    /// Updates the smooth movement animation and processes the key buffer
    /// </summary>
    /// <returns>True if still moving or movement was initiated, false otherwise</returns>
    public bool UpdateMovement(List<Tile> tiles)
    {
        // Update sprite animation
        UpdateAnimation();

        if (IsMoving)
        {
            float speed = MovementSpeed * currentSpeedMultiplier;
            float dx = targetX - ActualX;
            float dy = targetY - ActualY;
            float distance = (float)Math.Sqrt(dx * dx + dy * dy);

            if (distance < 1)
            {
                ActualX = targetX;
                ActualY = targetY;
                IsMoving = false;
                // Try to move again immediately if there are keys in the buffer
                return TryMoveFromBuffer(tiles);
            }

            ActualX += (dx / distance) * speed;
            ActualY += (dy / distance) * speed;
            return true;
        }
        else
        {
            return TryMoveFromBuffer(tiles);
        }
    }

    /// <summary>
    /// Gets the current visual X coordinate for rendering
    /// </summary>
    /// <returns>The interpolated X position for smooth movement</returns>
    public float GetVisualX()
    {
        return ActualX;
    }

    /// <summary>
    /// Gets the current visual Y coordinate for rendering
    /// </summary>
    /// <returns>The interpolated Y position for smooth movement</returns>
    public float GetVisualY()
    {
        return ActualY;
    }

    /// <summary>
    /// Gets the current key buffer for debugging purposes
    /// </summary>
    /// <returns>A string containing the currently pressed movement keys</returns>
    public string GetKeyBuffer()
    {
        return keyBuffer;
    }

    /// <summary>
    /// Gets the current animation frame for debugging purposes
    /// </summary>
    /// <returns>The current frame index (0-3)</returns>
    public int GetCurrentFrame()
    {
        return currentFrame;
    }
}