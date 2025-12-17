using System;
using System.Reflection;
using HarmonyLib;

namespace InstanceMod
{
    /// <summary>
    /// Main ModAPI entry point for the Instance Mod
    /// </summary>
    public class API : IModApi
    {
        public static string ModName = "InstanceMod";
        public static string Version = "0.1.0";
        
        /// <summary>
        /// Initialize the mod when the server/game loads
        /// </summary>
        /// <param name="_modInstance">Mod instance provided by the game</param>
        public void InitMod(Mod _modInstance)
        {
            try
            {
                Log.Out($"[{ModName}] Version {Version} - Initializing...");
                
                // Initialize Harmony patching
                InitializeHarmony();
                
                // Register ModAPI event handlers
                RegisterEventHandlers();
                
                // Initialize core systems
                Core.InstanceManager.Initialize();
                
                Log.Out($"[{ModName}] Successfully initialized!");
            }
            catch (Exception ex)
            {
                Log.Error($"[{ModName}] Failed to initialize: {ex.Message}");
                Log.Error($"[{ModName}] Stack trace: {ex.StackTrace}");
            }
        }
        
        /// <summary>
        /// Initialize Harmony and apply all patches
        /// </summary>
        private void InitializeHarmony()
        {
            try
            {
                var harmony = new Harmony("com.jamesbridgeford.instancemod");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                Log.Out($"[{ModName}] Harmony patches applied successfully");
            }
            catch (Exception ex)
            {
                Log.Error($"[{ModName}] Harmony initialization failed: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Register all ModAPI event handlers
        /// </summary>
        private void RegisterEventHandlers()
        {
            // Game lifecycle events
            ModEvents.GameStartDone.RegisterHandler(OnGameStartDone);
            ModEvents.GameShutdown.RegisterHandler(OnGameShutdown);
            
            // Player events
            ModEvents.PlayerSpawnedInWorld.RegisterHandler(OnPlayerSpawned);
            ModEvents.PlayerDisconnected.RegisterHandler(OnPlayerDisconnected);
            ModEvents.SavePlayerData.RegisterHandler(OnSavePlayerData);
            ModEvents.ChatMessage.RegisterHandler(OnChatMessage);
            
            Log.Out($"[{ModName}] Event handlers registered");
        }
        
        #region Event Handlers
        
        /// <summary>
        /// Called when the game/server has fully started
        /// </summary>
        private void OnGameStartDone()
        {
            Log.Out($"[{ModName}] Game started - Instance system ready");
            Core.InstanceManager.OnGameStarted();
        }
        
        /// <summary>
        /// Called when the game/server is shutting down
        /// </summary>
        private void OnGameShutdown()
        {
            Log.Out($"[{ModName}] Game shutting down - Saving instance data");
            Core.InstanceManager.OnGameShutdown();
        }
        
        /// <summary>
        /// Called when a player spawns or respawns in the world
        /// </summary>
        private void OnPlayerSpawned(ClientInfo _cInfo, RespawnType _respawnReason, Vector3i _pos)
        {
            try
            {
                if (_cInfo == null)
                    return;
                    
                Log.Out($"[{ModName}] Player {_cInfo.playerName} spawned (Reason: {_respawnReason})");
                Core.InstanceManager.OnPlayerSpawned(_cInfo, _respawnReason, _pos);
            }
            catch (Exception ex)
            {
                Log.Error($"[{ModName}] Error in OnPlayerSpawned: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Called when a player disconnects from the server
        /// </summary>
        private void OnPlayerDisconnected(ClientInfo _cInfo, bool _bShutdown)
        {
            try
            {
                if (_cInfo == null)
                    return;
                    
                Log.Out($"[{ModName}] Player {_cInfo.playerName} disconnected");
                Core.InstanceManager.OnPlayerDisconnected(_cInfo, _bShutdown);
            }
            catch (Exception ex)
            {
                Log.Error($"[{ModName}] Error in OnPlayerDisconnected: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Called before player data is saved
        /// </summary>
        private void OnSavePlayerData(ClientInfo _cInfo, PlayerDataFile _playerDataFile)
        {
            try
            {
                if (_cInfo == null || _playerDataFile == null)
                    return;
                    
                Core.InstanceManager.SavePlayerInstanceData(_cInfo, _playerDataFile);
            }
            catch (Exception ex)
            {
                Log.Error($"[{ModName}] Error in OnSavePlayerData: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Called when a chat message is sent
        /// </summary>
        private bool OnChatMessage(ClientInfo _cInfo, EChatType _type, int _senderId, 
            string _msg, string _mainName, bool _localizeMain, System.Collections.Generic.List<int> _recipientEntityIds)
        {
            try
            {
                // Allow other mods to process chat messages
                // Return false to consume the message and prevent other handlers
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"[{ModName}] Error in OnChatMessage: {ex.Message}");
                return true;
            }
        }
        
        #endregion
    }
}
