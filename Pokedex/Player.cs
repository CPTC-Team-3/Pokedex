namespace Pokedex
{
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
        /// The constructor for the Player class.
        /// </summary>
        /// <param name="startX">The starting X position of the player</param>
        /// <param name="startY">The starting Y position of the player</param>
        public Player(int startX, int startY, int size)
        {
            X = startX;
            Y = startY;
            Size = size; // Set the size based on the provided tile size
        }

        /// <summary>
        /// Moves the object by the specified horizontal and vertical offsets, if the target position is walkable.
        /// </summary>
        /// <remarks>The movement is calculated based on the object's current position and size. The
        /// target position is determined by adding the scaled offsets (<paramref name="deltaX"/> and <paramref
        /// name="deltaY"/> multiplied by the object's size)  to the current position. If the target tile exists in the
        /// provided <paramref name="tiles"/> list and is walkable,  the object's position is updated to the target
        /// position.</remarks>
        /// <param name="deltaX">The horizontal offset, in tile units, to move the object.</param>
        /// <param name="deltaY">The vertical offset, in tile units, to move the object.</param>
        /// <param name="tiles">A list of tiles representing the grid. Each tile is checked to determine if the target position is walkable.</param>
        public void Move(int deltaX, int deltaY, List<Tile> tiles)
        {
            int newX = X + (deltaX * Size);
            int newY = Y + (deltaY * Size);

            Tile? targetTile = tiles.Find(t => t.X == newX && t.Y == newY);

            if (targetTile != null && targetTile.IsWalkable)
            {
                X = newX;
                Y = newY;
            }
        }
    }
}