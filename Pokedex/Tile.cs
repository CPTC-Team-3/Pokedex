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
    /// The color of the tile, used for rendering when no texture is available.
    /// </summary>
    public Color TileColor { get; set; }

    /// <summary>
    /// The texture image for this tile.
    /// </summary>
    public Image? Texture { get; set; }

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
        Texture = null;
        IsWalkable = isWalkable;
        Speed = speed;
    }

    /// <summary>
    /// The constructor for the Tile class with texture.
    /// </summary>
    /// <param name="x">The X position of this tile</param>
    /// <param name="y">The Y position of this tile</param>
    /// <param name="texture">The texture image for this tile</param>
    /// <param name="isWalkable">A value indicating whether this tile can be used for walking</param>
    /// <param name="speed">The transition speed multiplier for players moving from this tile (default is 1.0)</param>
    public Tile(int x, int y, Image texture, bool isWalkable, float speed = 1.0f)
    {
        X = x;
        Y = y;
        TileColor = Color.White; // Default fallback color
        Texture = texture;
        IsWalkable = isWalkable;
        Speed = speed;
    }

    /// <summary>
    /// The constructor for the Tile class with both texture and color.
    /// </summary>
    /// <param name="x">The X position of this tile</param>
    /// <param name="y">The Y position of this tile</param>
    /// <param name="texture">The texture image for this tile</param>
    /// <param name="fallbackColor">The fallback color if texture fails to load</param>
    /// <param name="isWalkable">A value indicating whether this tile can be used for walking</param>
    /// <param name="speed">The transition speed multiplier for players moving from this tile (default is 1.0)</param>
    public Tile(int x, int y, Image texture, Color fallbackColor, bool isWalkable, float speed = 1.0f)
    {
        X = x;
        Y = y;
        TileColor = fallbackColor;
        Texture = texture;
        IsWalkable = isWalkable;
        Speed = speed;
    }

    /// <summary>
    /// Sets the texture for this tile from a file path.
    /// </summary>
    /// <param name="texturePath">Path to the texture file</param>
    /// <returns>True if texture was loaded successfully, false otherwise</returns>
    public bool SetTexture(string texturePath)
    {
        try
        {
            if (File.Exists(texturePath))
            {
                Texture = Image.FromFile(texturePath);
                return true;
            }
        }
        catch (Exception)
        {
            // If texture loading fails, keep the existing texture (or null)
        }
        return false;
    }

    /// <summary>
    /// Gets whether this tile has a valid texture.
    /// </summary>
    public bool HasTexture => Texture != null;
}