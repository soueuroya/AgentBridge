using UnityEditor;
using UnityEngine;

namespace AgentBridge.Editor.Execution.Commands
{
    /// <summary>
    /// A command representing strict asset-level metadata configuration changes.
    /// This demonstrates our safe 'IsDangerous' flag usage for destructive actions.
    /// </summary>
    public class ModifyImportSettingsCommand : IAgentCommand
    {
        public string CommandName => "ModifyImportSettings";
        
        // This is explicitly marked true since modifying asset importers causes re-compression/data mutation.
        public bool IsDangerous => true;

        private string _assetPath;
        // In a strictly typed system, we would wrap exactly what setting we want here:
        private bool _forceReadWrite;

        public ModifyImportSettingsCommand(string assetPath, bool forceReadWrite = true)
        {
            _assetPath = assetPath;
            _forceReadWrite = forceReadWrite;
        }

        public bool Validate(out string errorMessage)
        {
            if (string.IsNullOrEmpty(_assetPath))
            {
                errorMessage = "Validation failure: Asset path string is missing.";
                return false;
            }
            
            AssetImporter importer = AssetImporter.GetAtPath(_assetPath);
            if (importer == null)
            {
                errorMessage = "Could not locate standard AssetImporter. Make sure the path includes the extension.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        public void Execute()
        {
            AssetImporter standardImporter = AssetImporter.GetAtPath(_assetPath);
            
            // To prove the command pattern, we execute a simple test implementation here. 
            // In a production codebase, this would branch to TextureImporter vs ModelImporter etc.
            if (standardImporter is TextureImporter textureImporter)
            {
                textureImporter.isReadable = _forceReadWrite;
                
                // AssetImporter commits cannot natively be cleanly reversed by Unity's Undo stack since
                // it manipulates external bytes/metafiles, meaning the system MUST rely on its Validation pipeline!
                textureImporter.SaveAndReimport();
                Debug.Log($"[AgentBridge] Set Texture Read/Write configuration to '{_forceReadWrite}' on '{_assetPath}'");
            }
            else
            {
                Debug.LogWarning("[AgentBridge] ModifyImportSettings fallback: Asset was not a texture so no custom action was applied.");
            }
        }
    }
}
