namespace Pokedex;
public class Tile
{
    /// <summary>
    /// The X position of the tile in the grid.
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// The Y position of the tile in the grid.
    /// </summary>
    public int Y { get; set; }

    /// <summary>
    /// The color of the tile, used for rendering.
    /// </summary>
    public Color TileColor { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the current object can be traversed or walked on.
    /// </summary>
    public bool IsWalkable { get; set; }

    /// <summary>
    /// The transition speed multiplier for players moving from this tile.
    /// 1.0 is normal speed, 2.0 is double speed, 0.5 is half speed.
    /// </summary>
    public float Speed { get; set; }

    /// <summary>
    /// The constructor for the Tile class.
    /// </summary>
    /// <param name="x">The X position of this tile</param>
    /// <param name="y">The Y position of this tile</param>
    /// <param name="color">The rendering color of this tile</param>
    /// <param name="isWalkable">A value indicating whether this tile can be used for walking</param>
    /// <param name="speed">The transition speed multiplier for players moving from this tile (default is 1.0)</param>
    public Tile(int x, int y, Color color, bool isWalkable, float speed = 1.0f)
    {
        X = x;
        Y = y;
        TileColor = color;
        IsWalkable = isWalkable;
        Speed = speed;
    }
}