# Texture System Usage

This game now supports textures for both tiles and player sprite sheet animation.

## Folder Structure

Create a "Textures" folder in your application directory with the following structure:

```
Textures/
??? player_spritesheet.png
??? tiles/
    ??? grass.png
    ??? sand.png
    ??? rock.png
```

## Player Sprite Sheet Format

The player sprite sheet should be organized as follows:
- **Width**: 4 frames × sprite width (e.g., 4 × 32 = 128 pixels)
- **Height**: 3 rows × sprite height (e.g., 3 × 32 = 96 pixels)

### Sprite Sheet Layout:
```
Row 0: South-facing frames  [idle] [step] [idle] [step]
Row 1: East-facing frames   [idle] [step] [idle] [step]
Row 2: North-facing frames  [idle] [step] [idle] [step]
```

**Note**: West direction automatically uses East frames but horizontally flipped.

### Animation Behavior:
- **When moving**: Cycles through frames 0 ? 1 ? 0 ? 2 ? repeat
- **When idle**: Stays on frame 0 (idle pose)
- **Frames 1 and 3**: Use the idle pose to create a natural walking effect
- **Frame 2**: Uses the step frame for actual movement

## Tile Textures

Tile textures are automatically applied based on tile color:
- **Green tiles** ? grass.png
- **Sandy brown tiles** ? sand.png  
- **Gray tiles** ? rock.png

## Manual Texture Loading

You can also load textures programmatically:

```csharp
// Load player sprite sheet
player.SetSpriteSheet("path/to/player_spritesheet.png", 32, 32);

// Load tile texture
tile.SetTexture("path/to/tile_texture.png");

// Load all textures from directory
LoadTextures("path/to/textures/directory");
```

## Fallback Behavior

If texture files are not found, the game will fall back to the original color-based rendering, so the game will still work without texture files.