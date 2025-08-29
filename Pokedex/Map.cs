namespace Pokedex;

/// <summary>
/// Represents different types of terrain tiles in the game world
/// </summary>
public enum TileType
{
    Grass,
    Dirt,
    Stone,
    Water,
    Sand
}

/// <summary>
/// Handles the generation and management of game world maps
/// </summary>
public class Map
{
    /// <summary>
    /// Generates a complete map of tiles with varied terrain
    /// </summary>
    /// <param name="width">Width of the map in tiles</param>
    /// <param name="height">Height of the map in tiles</param>
    /// <param name="tileSize">Size of each tile in pixels</param>
    /// <returns>A list of tiles representing the complete map</returns>
    public static List<Tile> GenerateMap(int width, int height, int tileSize)
    {
        var tiles = new List<Tile>();
        var random = new Random(42); // Fixed seed for consistent map generation

        // Generate noise map for terrain variation
        var terrainMap = GenerateTerrainMap(width, height, random);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                TileType tileType = terrainMap[x, y];
                Tile tile = CreateTileFromType(x, y, tileType, tileSize);
                tiles.Add(tile);
            }
        }

        // Add some paths through the map
        AddPathsToMap(tiles, width, height, tileSize);

        return tiles;
    }

    /// <summary>
    /// Generates a 2D terrain map using procedural generation
    /// </summary>
    private static TileType[,] GenerateTerrainMap(int width, int height, Random random)
    {
        var terrainMap = new TileType[width, height];

        // Fill with base grass terrain
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                terrainMap[x, y] = TileType.Grass;
            }
        }

        // Add water bodies
        AddWaterBodies(terrainMap, width, height, random);

        // Add stone formations
        AddStoneFormations(terrainMap, width, height, random);

        // Add sand near water
        AddSandNearWater(terrainMap, width, height);

        return terrainMap;
    }

    /// <summary>
    /// Adds water bodies to the terrain map
    /// </summary>
    private static void AddWaterBodies(TileType[,] terrainMap, int width, int height, Random random)
    {
        int numLakes = random.Next(2, 5);
        
        for (int i = 0; i < numLakes; i++)
        {
            int centerX = random.Next(width / 4, 3 * width / 4);
            int centerY = random.Next(height / 4, 3 * height / 4);
            int radius = random.Next(3, 8);

            for (int x = Math.Max(0, centerX - radius); x < Math.Min(width, centerX + radius); x++)
            {
                for (int y = Math.Max(0, centerY - radius); y < Math.Min(height, centerY + radius); y++)
                {
                    double distance = Math.Sqrt(Math.Pow(x - centerX, 2) + Math.Pow(y - centerY, 2));
                    if (distance <= radius)
                    {
                        terrainMap[x, y] = TileType.Water;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Adds stone formations to the terrain map
    /// </summary>
    private static void AddStoneFormations(TileType[,] terrainMap, int width, int height, Random random)
    {
        int numRockFormations = random.Next(3, 7);
        
        for (int i = 0; i < numRockFormations; i++)
        {
            int centerX = random.Next(width);
            int centerY = random.Next(height);
            int size = random.Next(2, 5);

            for (int x = Math.Max(0, centerX - size); x < Math.Min(width, centerX + size); x++)
            {
                for (int y = Math.Max(0, centerY - size); y < Math.Min(height, centerY + size); y++)
                {
                    if (terrainMap[x, y] != TileType.Water && random.NextDouble() < 0.6)
                    {
                        terrainMap[x, y] = TileType.Stone;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Adds sand tiles near water bodies
    /// </summary>
    private static void AddSandNearWater(TileType[,] terrainMap, int width, int height)
    {
        var sandPositions = new List<(int x, int y)>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (terrainMap[x, y] == TileType.Water)
                {
                    // Check adjacent tiles for potential sand placement
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            int nx = x + dx;
                            int ny = y + dy;
                            
                            if (nx >= 0 && nx < width && ny >= 0 && ny < height &&
                                terrainMap[nx, ny] == TileType.Grass)
                            {
                                sandPositions.Add((nx, ny));
                            }
                        }
                    }
                }
            }
        }

        // Apply sand to positions adjacent to water
        foreach (var (x, y) in sandPositions)
        {
            terrainMap[x, y] = TileType.Sand;
        }
    }

    /// <summary>
    /// Adds dirt paths through the map for easier navigation
    /// </summary>
    private static void AddPathsToMap(List<Tile> tiles, int width, int height, int tileSize)
    {
        // Create a horizontal path through the middle
        int midY = height / 2;
        for (int x = 0; x < width; x++)
        {
            var tile = tiles.Find(t => t.X == x * tileSize && t.Y == midY * tileSize);
            if (tile != null && tile.TileColor != GetColorForTileType(TileType.Water) && 
                tile.TileColor != GetColorForTileType(TileType.Stone))
            {
                UpdateTileToType(tile, TileType.Dirt);
            }
        }

        // Create a vertical path through the middle
        int midX = width / 2;
        for (int y = 0; y < height; y++)
        {
            var tile = tiles.Find(t => t.X == midX * tileSize && t.Y == y * tileSize);
            if (tile != null && tile.TileColor != GetColorForTileType(TileType.Water) && 
                tile.TileColor != GetColorForTileType(TileType.Stone))
            {
                UpdateTileToType(tile, TileType.Dirt);
            }
        }
    }

    /// <summary>
    /// Creates a tile from the specified tile type
    /// </summary>
    private static Tile CreateTileFromType(int gridX, int gridY, TileType tileType, int tileSize)
    {
        Color color = GetColorForTileType(tileType);
        bool isWalkable = GetWalkableForTileType(tileType);
        float speed = GetSpeedForTileType(tileType);
        bool wildZone = GetWildZoneForTileType(tileType);

        return new Tile(gridX * tileSize, gridY * tileSize, color, isWalkable, speed, wildZone);
    }

    /// <summary>
    /// Updates an existing tile to match the specified tile type
    /// </summary>
    private static void UpdateTileToType(Tile tile, TileType tileType)
    {
        tile.TileColor = GetColorForTileType(tileType);
        tile.IsWalkable = GetWalkableForTileType(tileType);
        tile.Speed = GetSpeedForTileType(tileType);
        tile.WildZone = GetWildZoneForTileType(tileType);
    }

    /// <summary>
    /// Gets the color associated with a tile type
    /// </summary>
    private static Color GetColorForTileType(TileType tileType)
    {
        return tileType switch
        {
            TileType.Grass => Color.Green,
            TileType.Dirt => Color.SaddleBrown,
            TileType.Stone => Color.Gray,
            TileType.Water => Color.Blue,
            TileType.Sand => Color.SandyBrown,
            _ => Color.Black
        };
    }

    /// <summary>
    /// Gets whether a tile type is walkable
    /// </summary>
    private static bool GetWalkableForTileType(TileType tileType)
    {
        return tileType switch
        {
            TileType.Grass => true,
            TileType.Dirt => true,
            TileType.Stone => false,
            TileType.Water => false,
            TileType.Sand => true,
            _ => false
        };
    }

    /// <summary>
    /// Gets the speed multiplier for a tile type
    /// </summary>
    private static float GetSpeedForTileType(TileType tileType)
    {
        return tileType switch
        {
            TileType.Grass => 1.0f,
            TileType.Dirt => 1.2f, // Paths are slightly faster
            TileType.Stone => 1.0f, // Not walkable anyway
            TileType.Water => 1.0f, // Not walkable anyway
            TileType.Sand => 0.7f, // Sand is slower
            _ => 1.0f
        };
    }

    /// <summary>
    /// Gets whether a tile type is a wild zone for Pokemon encounters
    /// </summary>
    private static bool GetWildZoneForTileType(TileType tileType)
    {
        return tileType switch
        {
            TileType.Grass => true,  // Grass is a wild zone
            TileType.Dirt => false,  // Paths are not wild zones
            TileType.Stone => false, // Stone is not walkable
            TileType.Water => false, // Water is not walkable
            TileType.Sand => true,   // Sand is a wild zone
            _ => false
        };
    }
}