using System;
using System.Collections.Generic;
using System.Linq;

namespace InstanceMod.Core
{
    /// <summary>
    /// Manages all game instances and player assignments
    /// </summary>
    public static class InstanceManager
    {
        #region Fields
        
        /// <summary>
        /// All registered instances (Instance ID -> Instance)
        /// </summary>
        private static Dictionary<string, Instance> instances = new Dictionary<string, Instance>();
        
        /// <summary>
        /// Player to instance mapping (Player ID -> Instance ID)
        /// </summary>
        private static Dictionary<string, string> playerInstances = new Dictionary<string, string>();
        
        /// <summary>
        /// Lock object for thread-safe operations
        /// </summary>
        private static readonly object lockObject = new object();
        
        /// <summary>
        /// Default instance for players not assigned to any instance
        /// </summary>
        private static Instance defaultInstance;
        
        /// <summary>
        /// Is the manager initialized?
        /// </summary>
        private static bool isInitialized = false;
        
        #endregion
        
        #region Public Properties
        
        /// <summary>
        /// Total number of instances
        /// </summary>
        public static int InstanceCount
        {
            get
            {
                lock (lockObject)
                {
                    return instances.Count;
                }
            }
        }
        
        /// <summary>
        /// Default instance ID
        /// </summary>
        public static string DefaultInstanceId => defaultInstance?.Id ?? "default";
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initialize the instance manager
        /// </summary>
        public static void Initialize()
        {
            if (isInitialized)
            {
                Log.Warning("[InstanceMod] InstanceManager already initialized");
                return;
            }
            
            try
            {
                lock (lockObject)
                {
                    // Create default instance
                    defaultInstance = new Instance("default", "Default Instance", "system");
                    defaultInstance.Description = "Default instance for all players";
                    defaultInstance.MaxPlayers = 0; // Unlimited
                    
                    instances[defaultInstance.Id] = defaultInstance;
                    
                    // Load saved instances from disk (TODO: implement persistence)
                    LoadInstances();
                    
                    isInitialized = true;
                    Log.Out("[InstanceMod] InstanceManager initialized successfully");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[InstanceMod] Failed to initialize InstanceManager: {ex.Message}");
                throw;
            }
        }
        
        #endregion
        
        #region Instance Management
        
        /// <summary>
        /// Create a new instance
        /// </summary>
        public static Instance CreateInstance(string name, string ownerId, string description = "")
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Instance name cannot be empty");
                
            lock (lockObject)
            {
                // Generate unique ID
                string id = GenerateUniqueId(name);
                
                // Check if instance with this name already exists
                if (instances.Values.Any(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                {
                    Log.Warning($"[InstanceMod] Instance with name '{name}' already exists");
                    return null;
                }
                
                var instance = new Instance(id, name, ownerId)
                {
                    Description = description
                };
                
                instances[id] = instance;
                
                // Save to disk (TODO: implement persistence)
                SaveInstances();
                
                Log.Out($"[InstanceMod] Created new instance: {name} (ID: {id})");
                return instance;
            }
        }
        
        /// <summary>
        /// Get an instance by ID
        /// </summary>
        public static Instance GetInstance(string instanceId)
        {
            if (string.IsNullOrEmpty(instanceId))
                return defaultInstance;
                
            lock (lockObject)
            {
                if (instances.TryGetValue(instanceId, out var instance))
                    return instance;
                    
                return null;
            }
        }
        
        /// <summary>
        /// Get an instance by name
        /// </summary>
        public static Instance GetInstanceByName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;
                
            lock (lockObject)
            {
                return instances.Values.FirstOrDefault(i => 
                    i.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            }
        }
        
        /// <summary>
        /// Get all instances
        /// </summary>
        public static List<Instance> GetAllInstances()
        {
            lock (lockObject)
            {
                return instances.Values.ToList();
            }
        }
        
        /// <summary>
        /// Delete an instance
        /// </summary>
        public static bool DeleteInstance(string instanceId)
        {
            if (string.IsNullOrEmpty(instanceId))
                return false;
                
            // Cannot delete default instance
            if (instanceId == defaultInstance.Id)
            {
                Log.Warning("[InstanceMod] Cannot delete default instance");
                return false;
            }
            
            lock (lockObject)
            {
                if (!instances.ContainsKey(instanceId))
                    return false;
                    
                var instance = instances[instanceId];
                
                // Remove all players from this instance
                foreach (var playerId in instance.ActivePlayers.ToList())
                {
                    RemovePlayerFromInstance(playerId);
                }
                
                instances.Remove(instanceId);
                SaveInstances();
                
                Log.Out($"[InstanceMod] Deleted instance: {instance.Name} (ID: {instanceId})");
                return true;
            }
        }
        
        #endregion
        
        #region Player Management
        
        /// <summary>
        /// Assign a player to an instance
        /// </summary>
        public static bool AssignPlayerToInstance(string playerId, string instanceId)
        {
            if (string.IsNullOrEmpty(playerId))
                return false;
                
            lock (lockObject)
            {
                // Get target instance
                var targetInstance = GetInstance(instanceId);
                if (targetInstance == null)
                {
                    Log.Warning($"[InstanceMod] Instance {instanceId} not found");
                    return false;
                }
                
                // Remove from current instance if any
                if (playerInstances.TryGetValue(playerId, out string currentInstanceId))
                {
                    var currentInstance = GetInstance(currentInstanceId);
                    currentInstance?.RemovePlayer(playerId);
                }
                
                // Add to new instance
                if (targetInstance.AddPlayer(playerId))
                {
                    playerInstances[playerId] = instanceId;
                    return true;
                }
                
                return false;
            }
        }
        
        /// <summary>
        /// Remove a player from their current instance
        /// </summary>
        public static bool RemovePlayerFromInstance(string playerId)
        {
            if (string.IsNullOrEmpty(playerId))
                return false;
                
            lock (lockObject)
            {
                if (playerInstances.TryGetValue(playerId, out string instanceId))
                {
                    var instance = GetInstance(instanceId);
                    if (instance != null)
                    {
                        instance.RemovePlayer(playerId);
                        playerInstances.Remove(playerId);
                        return true;
                    }
                }
                
                return false;
            }
        }
        
        /// <summary>
        /// Get the instance a player is currently in
        /// </summary>
        public static Instance GetPlayerInstance(string playerId)
        {
            if (string.IsNullOrEmpty(playerId))
                return defaultInstance;
                
            lock (lockObject)
            {
                if (playerInstances.TryGetValue(playerId, out string instanceId))
                {
                    return GetInstance(instanceId);
                }
                
                // Return default instance if player not assigned
                return defaultInstance;
            }
        }
        
        /// <summary>
        /// Get player's instance data
        /// </summary>
        public static PlayerInstanceData GetPlayerData(string playerId, string instanceId = null)
        {
            if (string.IsNullOrEmpty(playerId))
                return null;
                
            lock (lockObject)
            {
                // Use current instance if not specified
                if (string.IsNullOrEmpty(instanceId))
                {
                    instanceId = playerInstances.ContainsKey(playerId) 
                        ? playerInstances[playerId] 
                        : DefaultInstanceId;
                }
                
                var instance = GetInstance(instanceId);
                return instance?.GetPlayerData(playerId);
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        /// <summary>
        /// Called when the game starts
        /// </summary>
        public static void OnGameStarted()
        {
            Log.Out("[InstanceMod] Game started - Instance system active");
        }
        
        /// <summary>
        /// Called when the game shuts down
        /// </summary>
        public static void OnGameShutdown()
        {
            Log.Out("[InstanceMod] Game shutting down - Saving all instance data");
            SaveInstances();
        }
        
        /// <summary>
        /// Called when a player spawns
        /// </summary>
        public static void OnPlayerSpawned(ClientInfo _cInfo, RespawnType _respawnReason, Vector3i _pos)
        {
            if (_cInfo == null)
                return;
                
            string playerId = _cInfo.CrossplatformId.CombinedString;
            
            // Assign to default instance if not in any instance
            if (!playerInstances.ContainsKey(playerId))
            {
                AssignPlayerToInstance(playerId, DefaultInstanceId);
            }
            
            // Update player data
            var playerData = GetPlayerData(playerId);
            if (playerData != null)
            {
                playerData.LastPosition = _pos;
                playerData.UpdateAccessTime();
            }
        }
        
        /// <summary>
        /// Called when a player disconnects
        /// </summary>
        public static void OnPlayerDisconnected(ClientInfo _cInfo, bool _bShutdown)
        {
            if (_cInfo == null)
                return;
                
            string playerId = _cInfo.CrossplatformId.CombinedString;
            
            // Update player data but don't remove from instance
            var playerData = GetPlayerData(playerId);
            if (playerData != null)
            {
                playerData.UpdateAccessTime();
            }
            
            // Remove from active players list
            var instance = GetPlayerInstance(playerId);
            instance?.RemovePlayer(playerId);
        }
        
        /// <summary>
        /// Save player instance data
        /// </summary>
        public static void SavePlayerInstanceData(ClientInfo _cInfo, PlayerDataFile _playerDataFile)
        {
            if (_cInfo == null || _playerDataFile == null)
                return;
                
            string playerId = _cInfo.CrossplatformId.CombinedString;
            
            // Update player data
            var playerData = GetPlayerData(playerId);
            if (playerData != null)
            {
                playerData.UpdateAccessTime();
                // TODO: Save instance-specific data to PlayerDataFile
            }
        }
        
        #endregion
        
        #region Persistence (TODO: Implement)
        
        /// <summary>
        /// Load instances from disk
        /// </summary>
        private static void LoadInstances()
        {
            // TODO: Implement loading from JSON/XML file
            Log.Out("[InstanceMod] Loading saved instances (not yet implemented)");
        }
        
        /// <summary>
        /// Save instances to disk
        /// </summary>
        private static void SaveInstances()
        {
            // TODO: Implement saving to JSON/XML file
            Log.Out("[InstanceMod] Saving instances (not yet implemented)");
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Generate a unique instance ID
        /// </summary>
        private static string GenerateUniqueId(string name)
        {
            // Create ID from name + timestamp
            string baseId = name.ToLower().Replace(" ", "_");
            string uniqueId = $"{baseId}_{DateTime.UtcNow.Ticks}";
            
            // Ensure uniqueness
            int counter = 1;
            string finalId = uniqueId;
            while (instances.ContainsKey(finalId))
            {
                finalId = $"{uniqueId}_{counter}";
                counter++;
            }
            
            return finalId;
        }
        
        #endregion
    }
}
