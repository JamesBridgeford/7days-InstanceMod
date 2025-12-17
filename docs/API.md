# API Documentation

## Overview

The Instance Mod provides a comprehensive API for managing isolated game instances within 7 Days to Die.

## Core Classes

### InstanceManager

Central manager for all instance operations.

```csharp
public static class InstanceManager
```

#### Methods

##### CreateInstance

Creates a new game instance.

```csharp
public static Instance CreateInstance(string name, string ownerId, string description = "")
```

**Parameters:**
- `name` - Unique display name for the instance
- `ownerId` - Player ID or "system" for system-created instances
- `description` - Optional description text

**Returns:** `Instance` object or `null` if creation failed

**Example:**
```csharp
var instance = InstanceManager.CreateInstance("PvP Arena", playerID, "Competitive PvP instance");
```

---

##### GetInstance

Retrieves an instance by ID.

```csharp
public static Instance GetInstance(string instanceId)
```

**Parameters:**
- `instanceId` - Unique instance identifier

**Returns:** `Instance` object or `null` if not found

---

##### GetInstanceByName

Retrieves an instance by display name.

```csharp
public static Instance GetInstanceByName(string name)
```

**Parameters:**
- `name` - Instance display name (case-insensitive)

**Returns:** `Instance` object or `null` if not found

---

##### GetAllInstances

Gets all registered instances.

```csharp
public static List<Instance> GetAllInstances()
```

**Returns:** List of all `Instance` objects

---

##### DeleteInstance

Deletes an instance (cannot delete default instance).

```csharp
public static bool DeleteInstance(string instanceId)
```

**Parameters:**
- `instanceId` - ID of instance to delete

**Returns:** `true` if deleted, `false` otherwise

---

##### AssignPlayerToInstance

Assigns a player to a specific instance.

```csharp
public static bool AssignPlayerToInstance(string playerId, string instanceId)
```

**Parameters:**
- `playerId` - Player's unique identifier
- `instanceId` - Target instance ID

**Returns:** `true` if assigned successfully

**Example:**
```csharp
bool success = InstanceManager.AssignPlayerToInstance(clientInfo.CrossplatformId.CombinedString, "pvp_arena");
```

---

##### RemovePlayerFromInstance

Removes a player from their current instance.

```csharp
public static bool RemovePlayerFromInstance(string playerId)
```

---

##### GetPlayerInstance

Gets the instance a player is currently in.

```csharp
public static Instance GetPlayerInstance(string playerId)
```

**Returns:** Player's current `Instance` or default instance if not assigned

---

##### GetPlayerData

Retrieves player's data for a specific instance.

```csharp
public static PlayerInstanceData GetPlayerData(string playerId, string instanceId = null)
```

**Parameters:**
- `playerId` - Player identifier
- `instanceId` - Instance ID (uses player's current instance if null)

**Returns:** `PlayerInstanceData` object or `null`

---

#### Properties

```csharp
public static int InstanceCount { get; }
public static string DefaultInstanceId { get; }
```

---

### Instance

Represents a single game instance.

```csharp
public class Instance
```

#### Properties

```csharp
public string Id { get; }                    // Unique identifier
public string Name { get; set; }              // Display name
public string Description { get; set; }       // Description
public string OwnerId { get; set; }           // Owner player ID
public DateTime CreatedDate { get; }          // Creation timestamp
public DateTime LastAccessDate { get; set; }  // Last access timestamp
public HashSet<string> ActivePlayers { get; } // Currently active players
public int MaxPlayers { get; set; }           // Max player limit (0 = unlimited)
public bool IsActive { get; set; }            // Is instance active?
public Dictionary<string, object> Settings { get; } // Configuration settings
public Dictionary<string, PlayerInstanceData> PlayerData { get; } // Player data
```

#### Methods

##### AddPlayer

```csharp
public bool AddPlayer(string playerId)
```

Adds a player to this instance. Returns `false` if instance is full.

---

##### RemovePlayer

```csharp
public bool RemovePlayer(string playerId)
```

Removes a player from this instance.

---

##### HasPlayer

```csharp
public bool HasPlayer(string playerId)
```

Checks if a player is in this instance.

---

##### GetPlayerData

```csharp
public PlayerInstanceData GetPlayerData(string playerId)
```

Gets player-specific data for this instance.

---

##### Reset

```csharp
public void Reset()
```

Resets all instance data (clears players and progression).

---

##### GetStats

```csharp
public string GetStats()
```

Returns formatted statistics string.

---

### PlayerInstanceData

Stores player-specific data for an instance.

```csharp
public class PlayerInstanceData
```

#### Properties

```csharp
public string PlayerId { get; }               // Player identifier
public string InstanceId { get; }             // Instance identifier
public int Level { get; set; }                // Player level
public int Experience { get; set; }           // Total XP
public Vector3i LastPosition { get; set; }    // Last known position
public int PlayTimeMinutes { get; set; }      // Total playtime
public DateTime FirstJoinDate { get; }        // First join timestamp
public DateTime LastAccessDate { get; set; }  // Last access timestamp
public int DeathCount { get; set; }           // Total deaths
public int ZombieKills { get; set; }          // Zombie kills
public int PlayerKills { get; set; }          // Player kills (PvP)
public Dictionary<string, object> CustomData { get; } // Custom data storage
public Dictionary<string, int> SkillLevels { get; }   // Skill progression
```

#### Methods

##### UpdateAccessTime

```csharp
public void UpdateAccessTime()
```

Updates the last access timestamp to current time.

---

##### AddExperience

```csharp
public void AddExperience(int amount)
```

Adds experience points and handles level-up calculations.

---

##### RecordDeath

```csharp
public void RecordDeath()
```

Increments death counter.

---

##### RecordZombieKill / RecordPlayerKill

```csharp
public void RecordZombieKill()
public void RecordPlayerKill()
```

Records kill statistics.

---

##### UpdateSkill / GetSkillLevel

```csharp
public void UpdateSkill(string skillName, int level)
public int GetSkillLevel(string skillName)
```

Manages skill progression data.

---

##### SetCustomData / GetCustomData

```csharp
public void SetCustomData(string key, object value)
public T GetCustomData<T>(string key, T defaultValue = default)
```

Stores and retrieves custom data values.

---

##### GetStats

```csharp
public string GetStats()
```

Returns formatted statistics string.

---

## Console Commands

### instance

Main command for instance management.

```
instance <subcommand> [parameters]
```

**Aliases:** `inst`

#### Subcommands

##### create

Creates a new instance.

```
instance create <name> [description]
```

**Example:**
```
instance create "PvP Arena" "Competitive player vs player instance"
```

---

##### list

Lists all instances.

```
instance list
```

---

##### info

Shows instance information.

```
instance info [name]
```

If no name provided, shows current player's instance.

---

##### join

Joins an instance.

```
instance join <name>
```

---

##### leave

Leaves current instance (returns to default).

```
instance leave
```

---

##### delete

Deletes an instance (admin only).

```
instance delete <name>
```

---

##### reset

Resets instance data (admin only).

```
instance reset <name>
```

---

##### stats

Shows instance statistics.

```
instance stats [name]
```

---

## ModEvents Integration

The mod hooks into standard ModEvents:

### GameStartDone

Called when game/server starts. Initializes instance system.

### GameShutdown

Called on shutdown. Saves all instance data.

### PlayerSpawnedInWorld

Called when player spawns. Assigns to default instance if not already assigned.

### PlayerDisconnected

Called on disconnect. Updates player data and removes from active players list.

### SavePlayerData

Called before saving player data. Persists instance-specific data.

## Usage Examples

### Creating and Managing Instances

```csharp
// Create a new instance
var instance = InstanceManager.CreateInstance("Hardcore Mode", playerId, "No respawns!");
if (instance != null)
{
    instance.MaxPlayers = 10;
    instance.Settings["AllowPvP"] = true;
}

// Assign player to instance
bool joined = InstanceManager.AssignPlayerToInstance(playerId, instance.Id);

// Get player's current instance
var currentInstance = InstanceManager.GetPlayerInstance(playerId);
```

### Tracking Player Progression

```csharp
// Get player data
var playerData = InstanceManager.GetPlayerData(playerId);
if (playerData != null)
{
    // Add experience
    playerData.AddExperience(100);
    
    // Record kills
    playerData.RecordZombieKill();
    
    // Update custom data
    playerData.SetCustomData("QuestsCompleted", 5);
    
    // Check stats
    int deaths = playerData.DeathCount;
    int level = playerData.Level;
}
```

### Listing Instances

```csharp
var instances = InstanceManager.GetAllInstances();
foreach (var inst in instances)
{
    Log.Out($"Instance: {inst.Name}");
    Log.Out($"  Players: {inst.ActivePlayers.Count}/{inst.MaxPlayers}");
    Log.Out($"  Created: {inst.CreatedDate}");
}
```

## Extension Points

The mod is designed to be extensible. Future versions may include:

- Custom instance types
- Instance-specific loot tables
- World state isolation
- Cross-instance teleportation
- Instance templates
- Scheduled events per instance

## Error Handling

All public APIs include error handling. Check logs for detailed error messages:

```csharp
try
{
    var instance = InstanceManager.CreateInstance(name, ownerId);
}
catch (Exception ex)
{
    Log.Error($"[InstanceMod] Failed to create instance: {ex.Message}");
}
```

---

**Version:** 0.1.0  
**Last Updated:** 2025-12-17
