# Architecture Documentation

## System Overview

The Instance Mod implements a hierarchical instance management system that isolates game experiences while maintaining server efficiency.

```
┌─────────────────────────────────────────────────────────────┐
│                    7 Days to Die Game                       │
└───────────────────────────┬─────────────────────────────────┘
                            │
                            │ ModAPI Integration
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                      API.cs (Entry Point)                    │
│  - InitMod()                                                 │
│  - Register ModEvents                                        │
│  - Initialize Harmony                                        │
└───────────────────────────┬─────────────────────────────────┘
                            │
        ┌───────────────────┼───────────────────┐
        │                   │                   │
        ▼                   ▼                   ▼
┌──────────────┐  ┌──────────────────┐  ┌────────────────┐
│   ModEvents  │  │  Harmony Patches │  │   Commands     │
│   Handlers   │  │   (Future)       │  │   System       │
└──────┬───────┘  └──────────────────┘  └────────┬───────┘
       │                                          │
       │                                          │
       └──────────────────┬───────────────────────┘
                          ▼
              ┌──────────────────────┐
              │   InstanceManager    │
              │   (Core System)      │
              │                      │
              │ - CreateInstance()   │
              │ - GetInstance()      │
              │ - AssignPlayer()     │
              │ - Persistence (TBD)  │
              └──────────┬───────────┘
                         │
          ┌──────────────┼──────────────┐
          ▼              ▼              ▼
    ┌──────────┐  ┌──────────┐  ┌──────────────────┐
    │ Instance │  │ Instance │  │ Default Instance │
    │   Data   │  │   Data   │  │                  │
    └────┬─────┘  └────┬─────┘  └────┬─────────────┘
         │             │              │
         │             │              │
         └─────────────┴──────────────┘
                       │
                       ▼
            ┌────────────────────────┐
            │  PlayerInstanceData    │
            │                        │
            │ - Level & Experience   │
            │ - Statistics           │
            │ - Custom Data          │
            │ - Skill Progression    │
            └────────────────────────┘
```

## Core Components

### 1. API.cs - ModAPI Entry Point

**Responsibilities:**
- Initialize the mod
- Register event handlers
- Set up Harmony patching
- Initialize InstanceManager

**Key Methods:**
```csharp
void InitMod(Mod _modInstance)
void InitializeHarmony()
void RegisterEventHandlers()
```

**Event Handlers:**
- `OnGameStartDone()` - Game initialization
- `OnGameShutdown()` - Cleanup and save
- `OnPlayerSpawned()` - Player spawn handling
- `OnPlayerDisconnected()` - Player cleanup
- `OnSavePlayerData()` - Data persistence
- `OnChatMessage()` - Chat integration (future)

### 2. InstanceManager - Core System

**Responsibilities:**
- Manage all instances
- Track player assignments
- Handle instance lifecycle
- Provide thread-safe operations

**Data Structures:**
```csharp
Dictionary<string, Instance> instances           // All instances
Dictionary<string, string> playerInstances       // Player -> Instance mapping
Instance defaultInstance                         // Fallback instance
object lockObject                                // Thread safety
```

**Key Operations:**
```csharp
CreateInstance()      // Create new instance
GetInstance()         // Retrieve instance by ID
GetInstanceByName()   // Retrieve by name
DeleteInstance()      // Remove instance
AssignPlayerToInstance()  // Player assignment
GetPlayerInstance()   // Get player's current instance
GetPlayerData()       // Retrieve player data
```

### 3. Instance - Data Model

**Represents:** Single game instance

**Properties:**
```csharp
string Id                    // Unique identifier
string Name                  // Display name
string Description           // Description text
string OwnerId              // Owner player ID
DateTime CreatedDate         // Creation timestamp
DateTime LastAccessDate      // Last access time
HashSet<string> ActivePlayers  // Current players
int MaxPlayers              // Player limit (0=unlimited)
bool IsActive               // Active status
Dictionary<string, object> Settings  // Configuration
Dictionary<string, PlayerInstanceData> PlayerData  // Player data
```

**Key Methods:**
```csharp
bool AddPlayer(string playerId)
bool RemovePlayer(string playerId)
bool HasPlayer(string playerId)
PlayerInstanceData GetPlayerData(string playerId)
void Reset()
string GetStats()
```

### 4. PlayerInstanceData - Player Progression

**Represents:** Player data within an instance

**Properties:**
```csharp
string PlayerId              // Player identifier
string InstanceId            // Instance identifier
int Level                    // Player level
int Experience               // Total XP
Vector3i LastPosition        // Last position
int PlayTimeMinutes          // Total playtime
DateTime FirstJoinDate       // First join timestamp
DateTime LastAccessDate      // Last access time
int DeathCount              // Total deaths
int ZombieKills             // Zombie kills
int PlayerKills             // PvP kills
Dictionary<string, object> CustomData  // Custom storage
Dictionary<string, int> SkillLevels    // Skill progression
```

**Key Methods:**
```csharp
void UpdateAccessTime()
void AddExperience(int amount)
void RecordDeath()
void RecordZombieKill()
void RecordPlayerKill()
void UpdateSkill(string skillName, int level)
int GetSkillLevel(string skillName)
void SetCustomData(string key, object value)
T GetCustomData<T>(string key, T defaultValue)
string GetStats()
```

### 5. InstanceCommand - Console Interface

**Responsibilities:**
- Parse console commands
- Execute instance operations
- Validate permissions
- Format output

**Subcommands:**
```csharp
create    // Create new instance
list      // List all instances
info      // Show instance details
join      // Join instance
leave     // Leave instance
delete    // Delete instance (admin)
reset     // Reset instance (admin)
stats     // Show statistics
```

## Data Flow Diagrams

### Player Join Flow

```
Player Spawns
     │
     ▼
OnPlayerSpawned()
     │
     ▼
Check if player in any instance?
     │
     ├─ No ──▶ AssignPlayerToInstance(playerId, DefaultInstanceId)
     │
     └─ Yes ─▶ Update player data
                   │
                   ▼
              Set LastPosition
              Update LastAccessDate
```

### Instance Creation Flow

```
Console Command: instance create <name>
     │
     ▼
InstanceCommand.ExecuteCreate()
     │
     ▼
Validate parameters
     │
     ▼
InstanceManager.CreateInstance(name, ownerId, description)
     │
     ├─ Generate unique ID
     │
     ├─ Check name uniqueness
     │
     ├─ Create Instance object
     │
     ├─ Initialize default settings
     │
     ├─ Add to instances dictionary
     │
     └─ Save to disk (TODO)
          │
          ▼
     Return Instance or null
```

### Player Assignment Flow

```
instance join <name>
     │
     ▼
Get target instance by name
     │
     ▼
Remove player from current instance
     │
     ▼
Check instance capacity
     │
     ├─ Full ──▶ Return false
     │
     └─ Has space
          │
          ▼
     Add player to instance
          │
          ▼
     Initialize PlayerInstanceData if needed
          │
          ▼
     Update playerInstances mapping
          │
          ▼
     Return success
```

## Threading Model

### Thread Safety

The InstanceManager uses a lock-based synchronization model:

```csharp
private static readonly object lockObject = new object();

public static Instance CreateInstance(string name, string ownerId)
{
    lock (lockObject)
    {
        // Thread-safe operation
        // All dictionary modifications protected
    }
}
```

**Critical Sections:**
- Instance creation/deletion
- Player assignment/removal
- Dictionary access
- Player data retrieval

**Thread-Safe Operations:**
- All public methods in InstanceManager
- Instance player list modifications
- PlayerInstanceData updates

## Memory Management

### Data Lifecycle

```
Server Start
     │
     ▼
Initialize InstanceManager
     │
     ├─ Create default instance
     ├─ Load saved instances (TODO)
     └─ Initialize dictionaries
          │
          ▼
     Runtime operations
          │
          ├─ Create instances (as needed)
          ├─ Track players (in-memory)
          └─ Update player data
               │
               ▼
     Server Shutdown
          │
          ├─ Save all instances (TODO)
          ├─ Save player data
          └─ Cleanup resources
```

### Memory Considerations

**Per Instance:**
- Instance object: ~1-2 KB
- Player data dictionary: ~100 bytes per player
- Active players HashSet: ~50 bytes per player
- Settings dictionary: ~500 bytes

**Estimated Memory Usage:**
- 10 instances with 10 players each: ~20 KB
- 100 instances with 50 players each: ~500 KB

## Persistence Architecture (TODO)

### Planned Persistence Layer

```
InstanceManager
     │
     ▼
PersistenceManager (Future)
     │
     ├─ JSON serialization
     ├─ File I/O operations
     └─ Backup system
          │
          ▼
     File Structure:
     ├─ instances.json      (All instance data)
     ├─ players.json        (All player data)
     └─ backups/            (Backup files)
```

### Save Points

1. **Game Shutdown** - Full save
2. **Periodic Autosave** - Every 5 minutes (planned)
3. **Instance Deletion** - Immediate save
4. **Player Data Changes** - Batched saves

## Extension Points

### 1. Harmony Patches (Future)

Planned patches for world state isolation:

```csharp
// Example: Isolate loot containers per instance
[HarmonyPatch(typeof(LootContainer))]
[HarmonyPatch("OnContainerOpen")]
class LootContainer_OnContainerOpen_Patch
{
    static void Prefix(LootContainer __instance, EntityPlayer player)
    {
        // Get player's instance
        // Load instance-specific loot
    }
}
```

### 2. Custom Settings

Extensible configuration system:

```csharp
instance.Settings["AllowPvP"] = true;
instance.Settings["SharedLoot"] = false;
instance.Settings["IsolateProgression"] = true;
instance.Settings["DifficultyMultiplier"] = 1.5;
```

### 3. Event Hooks

Future event system for mods:

```csharp
// Planned
InstanceEvents.OnInstanceCreated
InstanceEvents.OnPlayerJoinedInstance
InstanceEvents.OnPlayerLeftInstance
InstanceEvents.OnInstanceDeleted
```

## Performance Considerations

### Optimization Strategies

1. **Dictionary lookups**: O(1) average case
2. **Lock contention**: Minimize time in critical sections
3. **Memory pooling**: Reuse PlayerInstanceData objects (future)
4. **Lazy loading**: Load instance data on-demand (future)
5. **Caching**: Cache frequently accessed data

### Scalability

**Current Limits:**
- Instances: Limited by memory (~10,000+ possible)
- Players per instance: Configurable (0 = unlimited)
- Total players: Limited by game server capacity

**Bottlenecks:**
- File I/O during save operations (when implemented)
- Lock contention with many concurrent operations
- Memory usage with large player data sets

## Error Handling

### Exception Handling Strategy

```csharp
try
{
    // Risky operation
}
catch (ArgumentException ex)
{
    Log.Error($"[InstanceMod] Invalid argument: {ex.Message}");
    // Return safe default
}
catch (Exception ex)
{
    Log.Error($"[InstanceMod] Unexpected error: {ex.Message}");
    Log.Error($"[InstanceMod] Stack trace: {ex.StackTrace}");
    // Fail gracefully
}
```

### Error Recovery

- **Invalid instance**: Return default instance
- **Missing player data**: Create new PlayerInstanceData
- **Failed save**: Log error, continue operation
- **Null checks**: Validate all inputs

## Testing Strategy

### Unit Testing (Future)

```csharp
[TestClass]
public class InstanceManagerTests
{
    [TestMethod]
    public void CreateInstance_ValidName_ReturnsInstance()
    {
        var instance = InstanceManager.CreateInstance("Test", "owner");
        Assert.IsNotNull(instance);
    }
}
```

### Integration Testing

- Manual in-game testing
- Multi-player scenarios
- Stress testing with many instances
- Persistence testing (when implemented)

---

**Version:** 0.1.0  
**Last Updated:** 2025-12-17  
**Status:** Active Development
