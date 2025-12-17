using System;
using System.Collections.Generic;

namespace InstanceMod.Core
{
    /// <summary>
    /// Represents a game instance with isolated resources and progression
    /// </summary>
    public class Instance
    {
        #region Properties
        
        /// <summary>
        /// Unique identifier for this instance
        /// </summary>
        public string Id { get; private set; }
        
        /// <summary>
        /// Display name of the instance
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Instance description
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Owner of this instance (Steam ID or entity ID)
        /// </summary>
        public string OwnerId { get; set; }
        
        /// <summary>
        /// When this instance was created
        /// </summary>
        public DateTime CreatedDate { get; private set; }
        
        /// <summary>
        /// Last time this instance was accessed
        /// </summary>
        public DateTime LastAccessDate { get; set; }
        
        /// <summary>
        /// Players currently in this instance
        /// </summary>
        public HashSet<string> ActivePlayers { get; private set; }
        
        /// <summary>
        /// Maximum number of players allowed (0 = unlimited)
        /// </summary>
        public int MaxPlayers { get; set; }
        
        /// <summary>
        /// Is this instance currently active?
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// Instance-specific configuration settings
        /// </summary>
        public Dictionary<string, object> Settings { get; private set; }
        
        /// <summary>
        /// Player progression data for this instance
        /// </summary>
        public Dictionary<string, PlayerInstanceData> PlayerData { get; private set; }
        
        #endregion
        
        #region Constructor
        
        /// <summary>
        /// Create a new instance
        /// </summary>
        public Instance(string id, string name, string ownerId)
        {
            Id = id ?? Guid.NewGuid().ToString();
            Name = name;
            OwnerId = ownerId;
            CreatedDate = DateTime.UtcNow;
            LastAccessDate = DateTime.UtcNow;
            ActivePlayers = new HashSet<string>();
            MaxPlayers = 0; // Unlimited by default
            IsActive = true;
            Settings = new Dictionary<string, object>();
            PlayerData = new Dictionary<string, PlayerInstanceData>();
            
            InitializeDefaultSettings();
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Add a player to this instance
        /// </summary>
        public bool AddPlayer(string playerId)
        {
            if (string.IsNullOrEmpty(playerId))
                return false;
                
            // Check max players limit
            if (MaxPlayers > 0 && ActivePlayers.Count >= MaxPlayers)
            {
                Log.Warning($"[InstanceMod] Instance {Name} is full ({ActivePlayers.Count}/{MaxPlayers})");
                return false;
            }
            
            if (ActivePlayers.Add(playerId))
            {
                LastAccessDate = DateTime.UtcNow;
                
                // Initialize player data if not exists
                if (!PlayerData.ContainsKey(playerId))
                {
                    PlayerData[playerId] = new PlayerInstanceData(playerId, Id);
                }
                
                Log.Out($"[InstanceMod] Player {playerId} joined instance {Name}");
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Remove a player from this instance
        /// </summary>
        public bool RemovePlayer(string playerId)
        {
            if (ActivePlayers.Remove(playerId))
            {
                LastAccessDate = DateTime.UtcNow;
                Log.Out($"[InstanceMod] Player {playerId} left instance {Name}");
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Check if a player is in this instance
        /// </summary>
        public bool HasPlayer(string playerId)
        {
            return ActivePlayers.Contains(playerId);
        }
        
        /// <summary>
        /// Get player data for a specific player
        /// </summary>
        public PlayerInstanceData GetPlayerData(string playerId)
        {
            if (PlayerData.TryGetValue(playerId, out var data))
                return data;
                
            return null;
        }
        
        /// <summary>
        /// Reset all instance data (dangerous operation)
        /// </summary>
        public void Reset()
        {
            ActivePlayers.Clear();
            PlayerData.Clear();
            LastAccessDate = DateTime.UtcNow;
            Log.Warning($"[InstanceMod] Instance {Name} has been reset");
        }
        
        /// <summary>
        /// Get instance statistics
        /// </summary>
        public string GetStats()
        {
            return $"Instance: {Name}\n" +
                   $"  ID: {Id}\n" +
                   $"  Active Players: {ActivePlayers.Count}" + 
                   (MaxPlayers > 0 ? $"/{MaxPlayers}" : "") + "\n" +
                   $"  Total Player Data: {PlayerData.Count}\n" +
                   $"  Created: {CreatedDate:yyyy-MM-dd HH:mm:ss} UTC\n" +
                   $"  Last Access: {LastAccessDate:yyyy-MM-dd HH:mm:ss} UTC\n" +
                   $"  Status: {(IsActive ? "Active" : "Inactive")}";
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Initialize default settings for a new instance
        /// </summary>
        private void InitializeDefaultSettings()
        {
            Settings["AllowPvP"] = true;
            Settings["SharedLoot"] = false;
            Settings["IsolateProgression"] = true;
            Settings["PersistentWorld"] = true;
        }
        
        #endregion
    }
}
