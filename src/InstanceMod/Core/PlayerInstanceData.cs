using System;
using System.Collections.Generic;

namespace InstanceMod.Core
{
    /// <summary>
    /// Stores player-specific data for a particular instance
    /// </summary>
    public class PlayerInstanceData
    {
        #region Properties
        
        /// <summary>
        /// Player identifier (Steam ID or entity ID)
        /// </summary>
        public string PlayerId { get; private set; }
        
        /// <summary>
        /// Instance identifier this data belongs to
        /// </summary>
        public string InstanceId { get; private set; }
        
        /// <summary>
        /// Player's progression level in this instance
        /// </summary>
        public int Level { get; set; }
        
        /// <summary>
        /// Total experience points in this instance
        /// </summary>
        public int Experience { get; set; }
        
        /// <summary>
        /// Player's last known position in this instance
        /// </summary>
        public Vector3i LastPosition { get; set; }
        
        /// <summary>
        /// Total playtime in this instance (minutes)
        /// </summary>
        public int PlayTimeMinutes { get; set; }
        
        /// <summary>
        /// First time player joined this instance
        /// </summary>
        public DateTime FirstJoinDate { get; private set; }
        
        /// <summary>
        /// Last time player accessed this instance
        /// </summary>
        public DateTime LastAccessDate { get; set; }
        
        /// <summary>
        /// Number of times player has died in this instance
        /// </summary>
        public int DeathCount { get; set; }
        
        /// <summary>
        /// Number of zombies killed in this instance
        /// </summary>
        public int ZombieKills { get; set; }
        
        /// <summary>
        /// Number of players killed in this instance (PvP)
        /// </summary>
        public int PlayerKills { get; set; }
        
        /// <summary>
        /// Custom data storage for instance-specific information
        /// </summary>
        public Dictionary<string, object> CustomData { get; private set; }
        
        /// <summary>
        /// Instance-specific inventory data (optional feature)
        /// </summary>
        public string InventoryData { get; set; }
        
        /// <summary>
        /// Instance-specific skill progression (optional feature)
        /// </summary>
        public Dictionary<string, int> SkillLevels { get; private set; }
        
        #endregion
        
        #region Constructor
        
        /// <summary>
        /// Create new player instance data
        /// </summary>
        public PlayerInstanceData(string playerId, string instanceId)
        {
            PlayerId = playerId ?? throw new ArgumentNullException(nameof(playerId));
            InstanceId = instanceId ?? throw new ArgumentNullException(nameof(instanceId));
            
            Level = 1;
            Experience = 0;
            LastPosition = Vector3i.zero;
            PlayTimeMinutes = 0;
            FirstJoinDate = DateTime.UtcNow;
            LastAccessDate = DateTime.UtcNow;
            DeathCount = 0;
            ZombieKills = 0;
            PlayerKills = 0;
            
            CustomData = new Dictionary<string, object>();
            SkillLevels = new Dictionary<string, int>();
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Update player's last access time
        /// </summary>
        public void UpdateAccessTime()
        {
            LastAccessDate = DateTime.UtcNow;
        }
        
        /// <summary>
        /// Add experience points and handle level-up
        /// </summary>
        public void AddExperience(int amount)
        {
            if (amount <= 0)
                return;
                
            Experience += amount;
            
            // Simple level calculation (can be customized)
            int newLevel = CalculateLevel(Experience);
            if (newLevel > Level)
            {
                Level = newLevel;
                Log.Out($"[InstanceMod] Player {PlayerId} leveled up to {Level} in instance {InstanceId}");
            }
        }
        
        /// <summary>
        /// Record a death
        /// </summary>
        public void RecordDeath()
        {
            DeathCount++;
            Log.Out($"[InstanceMod] Player {PlayerId} died in instance {InstanceId} (Total: {DeathCount})");
        }
        
        /// <summary>
        /// Record a zombie kill
        /// </summary>
        public void RecordZombieKill()
        {
            ZombieKills++;
        }
        
        /// <summary>
        /// Record a player kill
        /// </summary>
        public void RecordPlayerKill()
        {
            PlayerKills++;
        }
        
        /// <summary>
        /// Update skill level
        /// </summary>
        public void UpdateSkill(string skillName, int level)
        {
            if (string.IsNullOrEmpty(skillName))
                return;
                
            SkillLevels[skillName] = level;
        }
        
        /// <summary>
        /// Get skill level
        /// </summary>
        public int GetSkillLevel(string skillName)
        {
            if (SkillLevels.TryGetValue(skillName, out int level))
                return level;
                
            return 0;
        }
        
        /// <summary>
        /// Set custom data value
        /// </summary>
        public void SetCustomData(string key, object value)
        {
            if (string.IsNullOrEmpty(key))
                return;
                
            CustomData[key] = value;
        }
        
        /// <summary>
        /// Get custom data value
        /// </summary>
        public T GetCustomData<T>(string key, T defaultValue = default)
        {
            if (CustomData.TryGetValue(key, out object value) && value is T typedValue)
                return typedValue;
                
            return defaultValue;
        }
        
        /// <summary>
        /// Get player statistics summary
        /// </summary>
        public string GetStats()
        {
            return $"Player Data for Instance {InstanceId}:\n" +
                   $"  Player ID: {PlayerId}\n" +
                   $"  Level: {Level} (XP: {Experience})\n" +
                   $"  Play Time: {PlayTimeMinutes} minutes\n" +
                   $"  Deaths: {DeathCount}\n" +
                   $"  Zombie Kills: {ZombieKills}\n" +
                   $"  Player Kills: {PlayerKills}\n" +
                   $"  First Join: {FirstJoinDate:yyyy-MM-dd HH:mm:ss} UTC\n" +
                   $"  Last Access: {LastAccessDate:yyyy-MM-dd HH:mm:ss} UTC";
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Calculate level based on experience (simple exponential curve)
        /// </summary>
        private int CalculateLevel(int exp)
        {
            // Simple formula: level = floor(sqrt(exp/100)) + 1
            // This can be customized based on desired progression curve
            if (exp <= 0)
                return 1;
                
            return (int)Math.Floor(Math.Sqrt(exp / 100.0)) + 1;
        }
        
        #endregion
    }
}
