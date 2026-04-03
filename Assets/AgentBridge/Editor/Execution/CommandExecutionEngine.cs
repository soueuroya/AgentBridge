using System;
using System.Collections.Generic;
using UnityEngine;

namespace AgentBridge.Editor.Execution
{
    /// <summary>
    /// Acts as the central traffic controller executing AI returns.
    /// Handles whitelisting, intercepting unvalidated instructions, and preventing editor crashes.
    /// </summary>
    [CreateAssetMenu(fileName = "CommandExecutionEngine", menuName = "AgentBridge/Execution Engine configuration")]
    public class CommandExecutionEngine : ScriptableObject
    {
        [Tooltip("When enabled, the AI cannot execute any capability unless it exists in the Whitelist array.")]
        public bool EnforceWhitelist = true;
        
        [Tooltip("Explicitly permitted command class names that the AI has permission to run.")]
        public List<string> Whitelist = new List<string>() 
        {
            "AddComponent",
            "RenameAsset",
            "ModifyImportSettings"
        };

        /// <summary>
        /// Attempt to safely pipe an IAgentCommand into the Unity backend.
        /// </summary>
        public bool TryExecute(IAgentCommand command, out string resultMessage)
        {
            if (command == null)
            {
                resultMessage = "Command was null or improperly deserialized.";
                return false;
            }

            if (EnforceWhitelist && !Whitelist.Contains(command.CommandName))
            {
                resultMessage = $"Command '{command.CommandName}' blocked by system whitelist constraints.";
                Debug.LogWarning($"[AgentBridge] Execution Blocked: {resultMessage}");
                return false;
            }

            if (!command.Validate(out string validationError))
            {
                resultMessage = $"Local Validation Failed: {validationError}";
                Debug.LogError($"[AgentBridge] Command Validation Error: {resultMessage}");
                return false;
            }

            try
            {
                // Action inherently registers its own Undo states (since requirements vary wildly per specific implementation) 
                command.Execute();
                resultMessage = "Success";
                return true;
            }
            catch (Exception ex)
            {
                resultMessage = $"Execution Exception Crash: {ex.Message}";
                Debug.LogException(ex);
                return false;
            }
        }
    }
}
