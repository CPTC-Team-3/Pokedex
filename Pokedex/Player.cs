namespace Pokedex;

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

    // New properties for smooth movement
    public float ActualX { get; private set; }
    public float ActualY { get; private set; }
    public bool IsMoving { get; private set; }
    public float MovementSpeed { get; private set; } = 5.0f; // Base movement speed
    private float targetX;
    private float targetY;
    private float currentSpeedMultiplier = 1.0f;

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
    /// Moves the object by the specified horizontal and vertical offsets, if the target position is walkable.
    /// </summary>
    /// <param name="deltaX">The horizontal offset, in tile units, to move the object.</param>
    /// <param name="deltaY">The vertical offset, in tile units, to move the object.</param>
    /// <param name="tiles">A list of tiles representing the grid.</param>
    /// <returns>True if movement was initiated, false otherwise.</returns>
    public bool Move(int deltaX, int deltaY, List<Tile> tiles)
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
    /// Updates the smooth movement animation.
    /// </summary>
    /// <returns>True if still moving, false if reached destination.</returns>
    public bool UpdateMovement()
    {
        if (!IsMoving) return false;

        float speed = MovementSpeed * currentSpeedMultiplier;
        float dx = targetX - ActualX;
        float dy = targetY - ActualY;
        float distance = (float)Math.Sqrt(dx * dx + dy * dy);

        if (distance < 1)
        {
            ActualX = targetX;
            ActualY = targetY;
            IsMoving = false;
            return false;
        }

        ActualX += (dx / distance) * speed;
        ActualY += (dy / distance) * speed;
        return true;
    }

    /// <summary>
    /// Gets the current visual position X coordinate for rendering.
    /// </summary>
    public float GetVisualX() => ActualX;

    /// <summary>
    /// Gets the current visual position Y coordinate for rendering.
    /// </summary>
    public float GetVisualY() => ActualY;
}