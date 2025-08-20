# Player Texture Zoom Functionality

This document explains the new zoom feature integrated into the Player class for the Pokedex game.

## Overview

The zoom functionality allows you to zoom into the player's sprite texture while maintaining the same player box size on screen. This is achieved by cropping the edges of the sprite texture based on the zoom level.

## How It Works

### Zoom Mechanics
- **Zoom Level 1.0**: Normal view (no cropping)
- **Zoom Level > 1.0**: Zoomed in view (crops edges to show center portion)
- **Zoom Level < 1.0**: Zoomed out view (shows more of the sprite, limited by sprite boundaries)

### Cropping Algorithm
When zooming in (zoom > 1.0):
1. Calculate crop factor: `1.0 / ZoomLevel`
2. Reduce sprite dimensions: `width * cropFactor` and `height * cropFactor`
3. Center the crop by calculating equal offsets from all edges
4. Ensure the cropped area stays within sprite boundaries

The player maintains the same visual size (`Player.Size`) on screen regardless of zoom level.

## Properties Added to Player Class

### Zoom Properties
- `ZoomLevel` (float): Current zoom level (default: 1.0)
- `MinZoomLevel` (float): Minimum allowed zoom (default: 0.5)
- `MaxZoomLevel` (float): Maximum allowed zoom (default: 3.0)

### Methods Added
- `SetZoomLevel(float zoomLevel)`: Sets zoom level within min/max bounds
- `ZoomIn(float amount = 0.1f)`: Increases zoom by specified amount
- `ZoomOut(float amount = 0.1f)`: Decreases zoom by specified amount
- `GetZoomLevel()`: Returns current zoom level for debugging

### Internal Methods
- `GetZoomedSpriteFrame(Rectangle baseSpriteRect)`: Calculates cropped rectangle based on zoom

## Keyboard Controls

The following keyboard controls have been added to Form1.cs:

- **+ (Plus) Key**: Zoom in by 0.1x
- **- (Minus) Key**: Zoom out by 0.1x  
- **R Key**: Reset zoom to 1.0x (normal)

## Debug Information

The window title now displays the current zoom level in the debug information:
```
Pokedex Game - FPS: 60.0 - Keys: - Dir: Down - Frame: 0 - Zoom: 1.5x
```

## Example Usage

```csharp
// Set specific zoom level
player.SetZoomLevel(2.0f); // 2x zoom

// Incremental zooming
player.ZoomIn(0.2f);       // Zoom in by 0.2x
player.ZoomOut(0.1f);      // Zoom out by 0.1x

// Check current zoom
float currentZoom = player.GetZoomLevel();
```

## Technical Details

### Sprite Frame Calculation
The zoom functionality modifies the `GetCurrentSpriteFrame()` method to:
1. Calculate the base sprite rectangle (unchanged)
2. Apply zoom cropping if zoom level ? 1.0
3. Return the appropriate source rectangle for rendering

### Rendering Impact
- The destination rectangle (screen size) remains constant at `Player.Size`
- Only the source rectangle (sprite portion) changes based on zoom
- Graphics quality is maintained using `InterpolationMode.NearestNeighbor`

### Performance Considerations
- Zoom calculations are only performed when zoom level ? 1.0
- Minimal performance impact due to simple mathematical operations
- No additional texture loading or memory allocation required

## Limitations

1. **Sprite Boundaries**: Cannot zoom out beyond original sprite dimensions
2. **Pixel Density**: Very high zoom levels may result in pixelated appearance
3. **Minimum Size**: Cropped area cannot be smaller than 1x1 pixel

## Future Enhancements

Potential improvements could include:
- Mouse wheel zoom support
- Smooth zoom transitions
- Per-direction zoom settings
- Zoom-dependent animation speed