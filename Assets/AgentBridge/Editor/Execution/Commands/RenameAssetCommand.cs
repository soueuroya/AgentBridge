using UnityEditor;
using UnityEngine;

namespace AgentBridge.Editor.Execution.Commands
{
    /// <summary>
    /// Instructs the engine to rename an asset inside the project structure.
    /// </summary>
    public class RenameAssetCommand : IAgentCommand
    {
        public string CommandName => "RenameAsset";
        public bool IsDangerous => false;

        private string _assetPath;
        private string _newName;

        public RenameAssetCommand(string assetPath, string newName)
        {
            _assetPath = assetPath;
            _newName = newName;
        }

        public bool Validate(out string errorMessage)
        {
            if (string.IsNullOrEmpty(_assetPath) || string.IsNullOrEmpty(_newName))
            {
                errorMessage = "Validation failure: missing asset path or new name string.";
                return false;
            }
            
            UnityEngine.Object internalAsset = AssetDatabase.LoadMainAssetAtPath(_assetPath);
            if (internalAsset == null)
            {
                errorMessage = $"Target asset was not found at standard path: {_assetPath}";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        public void Execute()
        {
            // Unity provides built-in Undo management for specific Asset actions via AssetDatabase renaming, 
            // but tracking its metadata file must remain synchronized.
            string standardResult = AssetDatabase.RenameAsset(_assetPath, _newName);
            
            if (string.IsNullOrEmpty(standardResult))
            {
                Debug.Log($"[AgentBridge] Action Success: Renamed standard asset '{_assetPath}' -> '{_newName}'.");
            }
            else
            {
                Debug.LogError($"[AgentBridge] Action Error on rename. Native Reason: {standardResult}");
            }
        }
    }
}
