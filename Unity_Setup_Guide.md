# Tower Defense Unity Setup Guide

## Project Structure Overview

Your Tower Defense game consists of several key systems that work together. Follow this guide to set up all GameObjects, components, and configurations in Unity.

## 1. Scene Setup

### Create Main Scene
1. Create a new Scene named `GameScene`
2. Set up the scene with proper lighting and camera positioning

### Scene Hierarchy Structure
```
GameScene
├── Main Camera
├── Directional Light
├── Canvas (UI)
├── Managers
├── Environment
├── Path
└── BuildNodes
```

## 2. Manager GameObjects Setup

### GameManager
1. Create empty GameObject named `GameManager`
2. Attach `GameManager.cs` script
3. Configure in Inspector:
   - Starting Money: 100
   - Starting Lives: 20
   - Enemy Spawn Point: Assign first waypoint
   - Enemy Target: Assign last waypoint
   - Waves: Create WaveData assets and assign to list

### BuildManager
1. Create empty GameObject named `BuildManager`
2. Attach `BuildManager.cs` script
3. Configure materials:
   - Hover Material: Create material with bright color
   - Buildable Material: Create material with green tint
   - Not Buildable Material: Create material with red tint

### WaveSpawner
1. Create empty GameObject named `WaveSpawner`
2. Attach `WaveSpawner.cs` script
3. Configure:
   - Spawn Point: Assign first waypoint transform
   - Time Between Waves: 5.0
   - Auto Start: Check if you want waves to start automatically

## 3. Path System Setup

### Waypoints
1. Create empty GameObject named `Waypoints`
2. Attach `Waypoints.cs` script
3. Add child empty GameObjects for each waypoint:
   - Name them `Waypoint (0)`, `Waypoint (1)`, etc.
   - Position them to create the enemy path
   - Ensure they form a clear path from spawn to target

### Visual Path (Optional)
1. Create line renderer or use simple cubes to visualize the path
2. Connect waypoints visually to help with level design

## 4. Build Nodes Setup

### Node Prefab Creation
1. Create a Cube GameObject
2. Attach `Node.cs` script
3. Configure:
   - Hover Color: White or light color
   - Not Enough Money Color: Red
   - Position Offset: (0, 0.5, 0) to build towers slightly above
4. Save as Prefab named `BuildNode`

### Placing Build Nodes
1. Duplicate the BuildNode prefab throughout the scene
2. Position nodes strategically around the path
3. Ensure nodes don't block the enemy path
4. Tag all nodes with "BuildNode" tag (create if doesn't exist)

## 5. Tower System Setup

### Tower Prefabs
Create two tower prefabs following this structure:

#### Machine Gun Tower Prefab
1. Create empty GameObject named `MachineGunTower`
2. Add child objects:
   - `Base`: Cube for tower base
   - `Turret`: Cylinder for rotating part
   - `FirePoint`: Empty GameObject positioned at gun barrel
   - `RangeDisplay`: Sphere with transparent material (initially disabled)
3. Attach `Tower.cs` script to main object
4. Configure Tower.cs:
   - Fire Point: Assign FirePoint transform
   - Part To Rotate: Assign Turret transform
   - Range Display: Assign RangeDisplay transform
5. Save as Prefab

#### Cannon Tower Prefab
1. Similar setup to Machine Gun Tower
2. Use different models/shapes for visual distinction
3. Same script configuration

### Projectile Prefabs
1. Create Sphere GameObject named `MachineGunProjectile`
2. Attach `Projectile.cs` script
3. Add Rigidbody component (kinematic)
4. Scale to appropriate size
5. Save as Prefab
6. Repeat for Cannon projectile with different appearance

## 6. Enemy System Setup

### Enemy Prefabs
Create two enemy prefabs:

#### Runner Enemy
1. Create Capsule GameObject named `RunnerEnemy`
2. Attach `Enemy.cs` and `EnemyMovement.cs` scripts
3. Add UI child object:
   - Create Canvas with World Space render mode
   - Add Image for health bar background
   - Add child Image for health bar fill
4. Configure Enemy.cs:
   - Health Bar: Assign fill Image
   - Health Bar UI: Assign canvas transform
5. Tag with "Enemy"
6. Save as Prefab

#### Tanker Enemy
1. Similar setup to Runner Enemy
2. Use different model/color for visual distinction
3. Same script configuration

## 7. UI System Setup

### Canvas Setup
1. Create Canvas with Screen Space - Overlay
2. Add Canvas Scaler component
3. Configure for UI scaling across devices

### Game UI Elements
Create these UI elements as children of Canvas:

#### Top UI Panel
1. Create Panel named `TopUI`
2. Add Text elements:
   - `MoneyText`: Display current money
   - `LivesText`: Display current lives
   - `WaveText`: Display current wave
   - `CountdownText`: Display next wave countdown
3. Position at top of screen

#### Tower Selection UI
1. Create Panel named `TowerSelectionUI`
2. Add Button elements for each tower type
3. Each button should have:
   - Image for tower icon
   - Text for cost
4. Position at bottom of screen

#### Game Over UI
1. Create Panel named `GameOverUI`
2. Add Text for "Game Over" message
3. Add Button for restart
4. Initially set inactive

#### Win UI
1. Create Panel named `WinUI`
2. Add Text for "You Win!" message
3. Add Button for restart
4. Initially set inactive

#### Node UI
1. Create Panel named `NodeUI`
2. Add Buttons:
   - Sell Button
   - Upgrade Button
3. Add Text elements:
   - Sell Value Text
   - Upgrade Cost Text
4. Position appropriately
5. Initially set inactive

### UI Manager Setup
1. Create empty GameObject named `UIManager`
2. Attach `UIManager.cs` script
3. Assign all UI elements to corresponding fields in inspector

### Tower Select UI Setup
1. Attach `TowerSelectUI.cs` to TowerSelectionUI panel
2. Assign tower buttons array
3. Create TowerData ScriptableObjects and assign to array

## 8. ScriptableObject Assets Creation

### Tower Data Assets
1. Right-click in Project window
2. Select Create > Tower Defense > Tower Data
3. Create assets for each tower type:
   - `MachineGunTowerData`
   - `CannonTowerData`
   - `UpgradedMachineGunTowerData`
   - `UpgradedCannonTowerData`

#### Machine Gun Tower Data Configuration
- Tower Name: "Machine Gun"
- Cost: 50
- Prefab: Assign MachineGunTower prefab
- Damage: 10
- Range: 5
- Fire Rate: 2
- Tower Type: MachineGun
- Projectile Prefab: Assign MachineGunProjectile
- Projectile Speed: 10
- Upgrade To: Assign UpgradedMachineGunTowerData
- Upgrade Cost: 100

#### Cannon Tower Data Configuration
- Tower Name: "Cannon"
- Cost: 100
- Prefab: Assign CannonTower prefab
- Damage: 50
- Range: 4
- Fire Rate: 0.5
- Tower Type: Cannon
- Projectile Prefab: Assign CannonProjectile
- Projectile Speed: 5
- Upgrade To: Assign UpgradedCannonTowerData
- Upgrade Cost: 200

### Enemy Data Assets
1. Create Enemy Data assets:
   - `RunnerEnemyData`
   - `TankerEnemyData`

#### Runner Enemy Data Configuration
- Enemy Name: "Runner"
- Prefab: Assign RunnerEnemy prefab
- Health: 50
- Speed: 5
- Reward: 10
- Enemy Type: Runner
- Enemy Color: Blue

#### Tanker Enemy Data Configuration
- Enemy Name: "Tanker"
- Prefab: Assign TankerEnemy prefab
- Health: 150
- Speed: 2
- Reward: 25
- Enemy Type: Tanker
- Enemy Color: Red

### Wave Data Assets
1. Create Wave Data assets for each wave:
   - `Wave1Data`
   - `Wave2Data`
   - `Wave3Data`
   - etc.

#### Example Wave Configurations
**Wave 1:**
- Wave Number: 1
- Spawn Delay: 0.5
- Time Between Waves: 5
- Enemies: 10 Runners

**Wave 2:**
- Wave Number: 2
- Spawn Delay: 0.5
- Time Between Waves: 5
- Enemies: 5 Runners, 2 Tankers

**Wave 3:**
- Wave Number: 3
- Spawn Delay: 0.3
- Time Between Waves: 5
- Enemies: 15 Runners, 5 Tankers

## 9. Tags and Layers Setup

### Required Tags
Create these tags in Tag Manager:
- `Enemy`
- `Tower`
- `BuildNode`
- `Waypoint`

### Apply Tags
- Apply "Enemy" tag to all enemy prefabs
- Apply "Tower" tag to all tower prefabs
- Apply "BuildNode" tag to all build node objects
- Apply "Waypoint" tag to waypoint objects

## 10. Materials Setup

### Tower Materials
1. Create materials for tower visual feedback:
   - `TowerHover`: Bright/white material
   - `TowerBuildable`: Green tinted material
   - `TowerNotBuildable`: Red tinted material

### Enemy Materials
1. Create materials for enemy types:
   - `RunnerMaterial`: Blue material
   - `TankerMaterial`: Red material

### UI Materials
1. Create transparent material for range display
2. Create materials for health bars (red for fill, dark for background)

## 11. Physics Setup

### Collision Layers
1. Set up collision layers if needed for optimization
2. Configure layer collision matrix in Physics settings

### Colliders
- Ensure enemy prefabs have colliders for tower targeting
- Node prefabs need colliders for mouse interaction
- Tower prefabs need colliders for selection

## 12. Testing Setup

### Play Mode Testing
1. Assign all references in managers
2. Test each system individually:
   - Tower building
   - Enemy spawning
   - Combat system
   - UI interactions
   - Wave progression

### Debug Features
1. Add debug visualizations:
   - Tower range displays
   - Enemy health bars
   - Path visualization
   - Node highlight system

## 13. Performance Optimization

### Object Pooling (Advanced)
Consider implementing object pooling for:
- Enemy spawning
- Projectile shooting
- UI element updates

### Update Optimization
- Use InvokeRepeating for tower target updates
- Limit UI updates to necessary changes only
- Consider using events for cross-system communication

## 14. Build Settings

### Player Settings
1. Configure appropriate settings for target platform
2. Set up icons and splash screens
3. Configure input settings for mobile if needed

### Quality Settings
1. Adjust quality levels for target devices
2. Optimize graphics settings for performance

## Common Issues and Solutions

### Enemy Not Moving
- Check if Waypoints script is attached and configured
- Verify waypoint positions form a valid path
- Ensure EnemyMovement script is attached

### Towers Not Shooting
- Verify enemy tag is correctly applied
- Check tower range and fire rate settings
- Ensure projectile prefab is assigned

### UI Not Updating
- Verify UIManager singleton is properly initialized
- Check all UI element references are assigned
- Ensure update methods are being called

### Build Nodes Not Working
- Verify Node script is attached
- Check BuildManager references
- Ensure mouse colliders are properly configured

This completes the Unity setup for your Tower Defense game. Test each system thoroughly and adjust values as needed for game balance.