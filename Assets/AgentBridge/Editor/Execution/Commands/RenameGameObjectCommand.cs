using AgentBridge.Editor.Execution;
using UnityEngine;
using UnityEditor;

namespace AgentBridge.Editor.Execution.Commands
{
    /// <summary>
    /// An explicit command targeted entirely at GameObjects within the Scene Hierarchy.
    /// This differs critically from RenameAssetCommand, utilizing Undo.RecordObject cleanly.
    /// </summary>
    public class RenameGameObjectCommand : IPreviewableCommand
    {
        public string CommandName => "RenameGameObject";
        public bool IsDangerous => false;

        private GameObject _target;
        private string _newName;

        public RenameGameObjectCommand(GameObject target, string newName)
        {
            _target = target;
            _newName = newName;
        }

        public string GetBeforeState() => _target != null ? _target.name : "Null Target";
        
        public string GetAfterState() => _newName;

        public bool Validate(out string errorMessage)
        {
            if (_target == null)
            {
                errorMessage = "Validation failure: Target GameObject is missing/null.";
                return false;
            }
            if (string.IsNullOrEmpty(_newName))
            {
                errorMessage = "Validation failure: The requested new name is empty.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        public void Execute()
        {
            // Forces standard hierarchy modification catching into the Unity Undo state
            Undo.RecordObject(_target, $"Rename GameObject to {_newName}");
            _target.name = _newName;
            Debug.Log($"[AgentBridge] Action Success: Renamed GameObject to '{_newName}'.");
        }
    }
}
