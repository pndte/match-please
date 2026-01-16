# Local Player Camera Follow - Usage Guide

## Overview
The `LocalPlayerCameraFollow` script makes the camera follow only the local player's character in a Unity Netcode multiplayer game. It follows the project's architecture patterns using Zenject for dependency injection and JetBrains Lifetimes for lifecycle management.

## Files Created
1. **CameraFollowConfig.cs** - ScriptableObject configuration for camera behavior
2. **LocalPlayerCameraFollow.cs** - Main camera follow script

## Setup Instructions

### 1. Create a Camera Follow Configuration Asset
1. In Unity, right-click in the Project window
2. Select `Create > Configs > CameraFollowConfig`
3. Name it (e.g., "MainCameraFollowConfig")
4. Configure the settings:
   - **Smooth Speed**: How smoothly the camera follows (0.01 = very smooth, 1.0 = instant)
   - **Offset**: Camera offset from player (e.g., `(0, 0, -10)` for 2D games)
   - **Use Boundaries**: Enable to restrict camera movement
   - **Min/Max Bounds**: Camera movement boundaries (if enabled)

### 2. Attach Script to Camera
1. Select your Main Camera in the scene
2. Add the `LocalPlayerCameraFollow` component
3. Assign the `CameraFollowConfig` asset you created
4. Enable/disable logging as needed

### 3. Optional: Zenject Integration
If you want to use Zenject to inject the Camera component:

```csharp
// In your scene installer or context
Container.Bind<Camera>().FromInstance(Camera.main).AsSingle();
```

The script will work without Zenject injection - it will automatically find the camera component.

## How It Works

### Automatic Local Player Detection
- The script automatically searches for the local player's NetworkObject
- It checks `NetworkObject.IsPlayerObject` and `NetworkObject.OwnerClientId`
- Only follows the player owned by the local client
- Handles cases where the player hasn't spawned yet

### Smooth Following
- Uses `Vector3.Lerp` for smooth camera movement
- Configurable smoothing speed via the config asset
- Updates in `LateUpdate()` for smooth following after player movement

### Lifecycle Management
- Uses JetBrains Lifetimes for proper cleanup
- Initialization flags prevent race conditions
- Automatically re-searches if the player is lost/despawned

### Multiplayer Safe
- Only follows the LOCAL player (not other players in the game)
- Works correctly in client, host, and server scenarios
- Handles player spawning/despawning gracefully

## API Reference

### Public Methods

#### `SetTargetManually(Transform target)`
Manually set a specific target to follow (useful for testing or cutscenes).

```csharp
cameraFollow.SetTargetManually(someTransform);
```

#### `ClearTarget()`
Clears the current target and resets to search for the local player again.

```csharp
cameraFollow.ClearTarget();
```

## Configuration Options

### CameraFollowConfig Settings

| Property | Type | Description | Default |
|----------|------|-------------|---------|
| SmoothSpeed | float | Camera follow smoothness (0.01-1.0) | 0.125 |
| Offset | Vector3 | Offset from player position | (0, 0, -10) |
| UseBoundaries | bool | Enable camera boundaries | false |
| MinBounds | Vector3 | Minimum camera position | (-100, -100, -100) |
| MaxBounds | Vector3 | Maximum camera position | (100, 100, 100) |

## Debug Features

### Logging
Enable `Enable Logging` in the inspector to see:
- When the script initializes
- When the local player is found
- When the target is lost
- Other important events

### Gizmos (Editor Only)
When the camera is selected in the editor:
- Yellow wire cube shows camera boundaries (if enabled)
- Green line connects camera to target
- Green wire sphere shows target position

## Troubleshooting

### Camera doesn't follow the player
1. Check that NetworkManager is running and the client is connected
2. Verify the player has a NetworkObject component
3. Ensure the player is spawned as a player object (using `SpawnAsPlayerObject`)
4. Enable logging to see what's happening

### Camera follows the wrong player
- This shouldn't happen - the script specifically checks `OwnerClientId == LocalClientId`
- If it does, check that your player spawning is using `SpawnAsPlayerObject` correctly

### Camera movement is jerky
- Increase the `SmoothSpeed` value in the config for more responsive following
- Ensure the script is on the camera (not a child object)
- Check that player movement is smooth

### Camera doesn't initialize
- Check that `CameraFollowConfig` is assigned in the inspector
- Verify a camera component exists (either on the same GameObject or as Camera.main)
- Check the console for error messages

## Example Configuration

### For 2D Games
```
Smooth Speed: 0.1
Offset: (0, 0, -10)
Use Boundaries: true
Min Bounds: (-50, -30, -10)
Max Bounds: (50, 30, -10)
```

### For 3D Games (Top-Down)
```
Smooth Speed: 0.15
Offset: (0, 10, -8)
Use Boundaries: false
```

### For 3D Games (Third Person)
```
Smooth Speed: 0.2
Offset: (0, 2, -5)
Use Boundaries: false
```

## Integration with Existing Code

The script is designed to work independently and doesn't require changes to your existing player code. It automatically detects the local player through Unity Netcode's NetworkManager.

If you need to notify the camera of player changes, you can access it via:

```csharp
var cameraFollow = Camera.main.GetComponent<LocalPlayerCameraFollow>();
if (cameraFollow != null)
{
    cameraFollow.ClearTarget(); // Force re-search
}
```

