using System;
using System.Collections.Generic;
using System.Text;
using InstanceMod.Core;

namespace InstanceMod.Commands
{
    /// <summary>
    /// Console command for instance management
    /// Usage: instance <subcommand> [parameters]
    /// </summary>
    public class InstanceCommand : ConsoleCmdAbstract
    {
        public override string GetDescription()
        {
            return "Manage game instances - Create, join, leave, and configure isolated game environments";
        }

        public override string GetHelp()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Instance Management Commands:");
            sb.AppendLine("  instance create <name> [description] - Create a new instance");
            sb.AppendLine("  instance list - List all instances");
            sb.AppendLine("  instance info [name] - Show instance information");
            sb.AppendLine("  instance join <name> - Join an instance");
            sb.AppendLine("  instance leave - Leave current instance");
            sb.AppendLine("  instance delete <name> - Delete an instance (admin only)");
            sb.AppendLine("  instance reset <name> - Reset instance data (admin only)");
            sb.AppendLine("  instance stats [name] - Show instance statistics");
            sb.AppendLine("\nExamples:");
            sb.AppendLine("  instance create MyInstance \"A private game instance\"");
            sb.AppendLine("  instance join MyInstance");
            sb.AppendLine("  instance list");
            return sb.ToString();
        }

        public override string[] GetCommands()
        {
            return new string[] { "instance", "inst" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                // Require at least one parameter (subcommand)
                if (_params.Count == 0)
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(GetHelp());
                    return;
                }

                string subCommand = _params[0].ToLower();

                switch (subCommand)
                {
                    case "create":
                        ExecuteCreate(_params, _senderInfo);
                        break;
                    case "list":
                        ExecuteList(_params, _senderInfo);
                        break;
                    case "info":
                        ExecuteInfo(_params, _senderInfo);
                        break;
                    case "join":
                        ExecuteJoin(_params, _senderInfo);
                        break;
                    case "leave":
                        ExecuteLeave(_params, _senderInfo);
                        break;
                    case "delete":
                    case "remove":
                        ExecuteDelete(_params, _senderInfo);
                        break;
                    case "reset":
                        ExecuteReset(_params, _senderInfo);
                        break;
                    case "stats":
                        ExecuteStats(_params, _senderInfo);
                        break;
                    default:
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Unknown subcommand: {subCommand}");
                        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(GetHelp());
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[InstanceMod] Error executing instance command: {ex.Message}");
                SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Error: {ex.Message}");
            }
        }

        #region Subcommand Handlers

        /// <summary>
        /// Create a new instance
        /// Usage: instance create <name> [description]
        /// </summary>
        private void ExecuteCreate(List<string> _params, CommandSenderInfo _senderInfo)
        {
            if (_params.Count < 2)
            {
                SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Usage: instance create <name> [description]");
                return;
            }

            string name = _params[1];
            string description = _params.Count > 2 ? string.Join(" ", _params.GetRange(2, _params.Count - 2)) : "";

            // Get player ID (use "console" for server console)
            string ownerId = GetPlayerId(_senderInfo);

            var instance = InstanceManager.CreateInstance(name, ownerId, description);
            if (instance != null)
            {
                SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Created instance: {name}");
                SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Instance ID: {instance.Id}");
            }
            else
            {
                SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Failed to create instance: {name} (may already exist)");
            }
        }

        /// <summary>
        /// List all instances
        /// Usage: instance list
        /// </summary>
        private void ExecuteList(List<string> _params, CommandSenderInfo _senderInfo)
        {
            var instances = InstanceManager.GetAllInstances();

            if (instances.Count == 0)
            {
                SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No instances found");
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Total Instances: {instances.Count}");
            sb.AppendLine("─────────────────────────────────────────");

            foreach (var instance in instances)
            {
                string status = instance.IsActive ? "Active" : "Inactive";
                string players = instance.MaxPlayers > 0
                    ? $"{instance.ActivePlayers.Count}/{instance.MaxPlayers}"
                    : $"{instance.ActivePlayers.Count}";

                sb.AppendLine($"• {instance.Name} [{status}]");
                sb.AppendLine($"  ID: {instance.Id}");
                sb.AppendLine($"  Players: {players}");
                if (!string.IsNullOrEmpty(instance.Description))
                    sb.AppendLine($"  Description: {instance.Description}");
                sb.AppendLine();
            }

            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(sb.ToString());
        }

        /// <summary>
        /// Show instance information
        /// Usage: instance info [name]
        /// </summary>
        private void ExecuteInfo(List<string> _params, CommandSenderInfo _senderInfo)
        {
            Instance instance;

            if (_params.Count < 2)
            {
                // Show current player's instance
                string playerId = GetPlayerId(_senderInfo);
                instance = InstanceManager.GetPlayerInstance(playerId);
            }
            else
            {
                string name = _params[1];
                instance = InstanceManager.GetInstanceByName(name);
            }

            if (instance == null)
            {
                SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Instance not found");
                return;
            }

            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(instance.GetStats());
        }

        /// <summary>
        /// Join an instance
        /// Usage: instance join <name>
        /// </summary>
        private void ExecuteJoin(List<string> _params, CommandSenderInfo _senderInfo)
        {
            if (_params.Count < 2)
            {
                SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Usage: instance join <name>");
                return;
            }

            string name = _params[1];
            string playerId = GetPlayerId(_senderInfo);

            var instance = InstanceManager.GetInstanceByName(name);
            if (instance == null)
            {
                SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Instance not found: {name}");
                return;
            }

            if (InstanceManager.AssignPlayerToInstance(playerId, instance.Id))
            {
                SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Joined instance: {name}");
            }
            else
            {
                SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Failed to join instance: {name} (may be full)");
            }
        }

        /// <summary>
        /// Leave current instance
        /// Usage: instance leave
        /// </summary>
        private void ExecuteLeave(List<string> _params, CommandSenderInfo _senderInfo)
        {
            string playerId = GetPlayerId(_senderInfo);

            var currentInstance = InstanceManager.GetPlayerInstance(playerId);
            if (currentInstance == null || currentInstance.Id == InstanceManager.DefaultInstanceId)
            {
                SingletonMonoBehaviour<SdtdConsole>.Instance.Output("You are not in any instance");
                return;
            }

            // Move to default instance
            if (InstanceManager.AssignPlayerToInstance(playerId, InstanceManager.DefaultInstanceId))
            {
                SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Left instance: {currentInstance.Name}");
            }
            else
            {
                SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Failed to leave instance");
            }
        }

        /// <summary>
        /// Delete an instance
        /// Usage: instance delete <name>
        /// </summary>
        private void ExecuteDelete(List<string> _params, CommandSenderInfo _senderInfo)
        {
            // Check admin permission
            if (!IsAdmin(_senderInfo))
            {
                SingletonMonoBehaviour<SdtdConsole>.Instance.Output("This command requires admin privileges");
                return;
            }

            if (_params.Count < 2)
            {
                SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Usage: instance delete <name>");
                return;
            }

            string name = _params[1];
            var instance = InstanceManager.GetInstanceByName(name);

            if (instance == null)
            {
                SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Instance not found: {name}");
                return;
            }

            if (InstanceManager.DeleteInstance(instance.Id))
            {
                SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Deleted instance: {name}");
            }
            else
            {
                SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Failed to delete instance: {name}");
            }
        }

        /// <summary>
        /// Reset instance data
        /// Usage: instance reset <name>
        /// </summary>
        private void ExecuteReset(List<string> _params, CommandSenderInfo _senderInfo)
        {
            // Check admin permission
            if (!IsAdmin(_senderInfo))
            {
                SingletonMonoBehaviour<SdtdConsole>.Instance.Output("This command requires admin privileges");
                return;
            }

            if (_params.Count < 2)
            {
                SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Usage: instance reset <name>");
                return;
            }

            string name = _params[1];
            var instance = InstanceManager.GetInstanceByName(name);

            if (instance == null)
            {
                SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Instance not found: {name}");
                return;
            }

            instance.Reset();
            SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Reset instance: {name}");
        }

        /// <summary>
        /// Show instance statistics
        /// Usage: instance stats [name]
        /// </summary>
        private void ExecuteStats(List<string> _params, CommandSenderInfo _senderInfo)
        {
            if (_params.Count < 2)
            {
                // Show overall statistics
                var sb = new StringBuilder();
                sb.AppendLine("Instance System Statistics:");
                sb.AppendLine($"  Total Instances: {InstanceManager.InstanceCount}");
                sb.AppendLine($"  Default Instance: {InstanceManager.DefaultInstanceId}");
                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(sb.ToString());
            }
            else
            {
                // Show specific instance stats
                ExecuteInfo(_params, _senderInfo);
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Get player ID from sender info
        /// </summary>
        private string GetPlayerId(CommandSenderInfo _senderInfo)
        {
            if (_senderInfo.RemoteClientInfo != null)
            {
                return _senderInfo.RemoteClientInfo.CrossplatformId.CombinedString;
            }
            return "console";
        }

        /// <summary>
        /// Check if sender has admin privileges
        /// </summary>
        private bool IsAdmin(CommandSenderInfo _senderInfo)
        {
            // Console always has admin
            if (_senderInfo.RemoteClientInfo == null)
                return true;

            // Check game master level
            return GameManager.Instance.adminTools.IsAdmin(_senderInfo.RemoteClientInfo.InternalId.CombinedString);
        }

        #endregion
    }
}
