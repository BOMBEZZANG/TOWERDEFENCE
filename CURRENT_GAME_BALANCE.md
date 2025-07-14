# Tower Defense Game - Current Balance Information

## Game Overview
- **Starting Money**: $100 (from GameManager)
- **Starting Lives**: 20 (from GameManager)
- **Game Speed Range**: 0.1x to 20x (adjustable in Inspector)

## Tower Data

### Machine Gun Tower
- **Name**: Machine Gun
- **Cost**: $50
- **Damage**: 10
- **Range**: 5
- **Fire Rate**: 2 shots/second
- **Turn Speed**: 10
- **Upgrade Cost**: $30
- **Projectile Speed**: 10

### Cannon Tower
- **Name**: Cannon
- **Cost**: $80
- **Damage**: 30
- **Range**: 4
- **Fire Rate**: 0.5 shots/second
- **Turn Speed**: 10
- **Upgrade Cost**: $60
- **Projectile Speed**: 5

## Enemy Data

### Runner Enemy
- **Name**: Runner
- **Health**: 30 HP
- **Speed**: 1.8 units/second
- **Reward**: $10
- **Type**: Fast, Low Health
- **Color**: Blue (RGB: 0, 0.45, 0.99)

### Tanker Enemy
- **Name**: Tanker
- **Health**: 100 HP
- **Speed**: 1.2 units/second
- **Reward**: $30
- **Type**: Slow, High Health
- **Color**: Red (RGB: 1, 0, 0)

## Wave Data

### Wave 1
- **Wave Number**: 1
- **Spawn Delay**: 1 second
- **Time Between Waves**: 5 seconds
- **Enemies**: 
  - 5x Runner enemies
  - Spawn delay: 1.5 seconds between enemies

### Wave 2
- **Wave Number**: 2
- **Spawn Delay**: 1 second
- **Time Between Waves**: 10 seconds
- **Enemies**: 
  - 10x Runner enemies
  - Spawn delay: 1 second between enemies

### Wave 3
- **Wave Number**: 3
- **Spawn Delay**: 0.3 seconds
- **Time Between Waves**: 5 seconds
- **Enemies**: 
  - First group: 5x Runner enemies (1 second spawn delay)
  - Second group: 4x Tanker enemies (1 second spawn delay)
  - Third group: 5x Runner enemies (2 second spawn delay)

## Balance Analysis

### Tower Efficiency
- **Machine Gun**: Low damage, high fire rate, affordable
  - DPS: 20 (10 damage × 2 fire rate)
  - Cost per DPS: $2.50
  
- **Cannon**: High damage, low fire rate, expensive
  - DPS: 15 (30 damage × 0.5 fire rate)
  - Cost per DPS: $5.33

### Enemy Analysis
- **Runner**: Fast but weak, good money efficiency
  - HP per Dollar Reward: 3 HP/$1
  - Time to cross map: ~Variable based on path length
  
- **Tanker**: Slow but tanky, high reward
  - HP per Dollar Reward: 3.33 HP/$1
  - Requires focused fire or upgraded towers

### Wave Progression
- **Wave 1**: Tutorial wave, 5 weak enemies
- **Wave 2**: Endurance test, double the enemies
- **Wave 3**: Mixed composition, introduces tankers

### Economic Balance
- **Starting Economy**: $100 starting money
- **Tower Investment**: Machine Gun provides better early game value
- **Upgrade Path**: Both towers have reasonable upgrade costs (30-60% of base cost)

## Potential Balance Issues
1. **Machine Gun dominance**: Higher DPS per dollar than Cannon
2. **Wave 2 difficulty spike**: 10 enemies vs 5 in Wave 1
3. **Limited tower variety**: Only 2 tower types available
4. **Economy scaling**: Fixed rewards may not scale well with increasing difficulty

## Data Collection
- Metrics are automatically collected via GameMetricsCollector
- Session data saved to: `GameMetrics/game_sessions_data.json`
- Target success rate: 90% (configurable)
- Target play time: 3 minutes (configurable)

---
*Generated on: $(date)*
*File location: TowerDefence/CURRENT_GAME_BALANCE.md*