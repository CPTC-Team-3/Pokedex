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
    /// The constructor for the Player class.
    /// </summary>
    /// <param name="startX">The starting X position of the player</param>
    /// <param name="startY">The starting Y position of the player</param>
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
                break;
            case 'S':
                deltaY = 1;
                break;
            case 'A':
                deltaX = -1;
                break;
            case 'D':
                deltaX = 1;
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
}