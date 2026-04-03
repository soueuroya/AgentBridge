using System;
using UnityEditor;
using UnityEngine;

namespace AgentBridge.Editor.Execution.Commands
{
    /// <summary>
    /// Connects an automated Component attaching action.
    /// Safely supports native Unity Undo mechanisms.
    /// </summary>
    public class AddComponentCommand : IAgentCommand
    {
        public string CommandName => "AddComponent";
        public bool IsDangerous => false;

        private GameObject _target;
        private Type _componentType;

        public AddComponentCommand(GameObject target, string componentTypeName)
        {
            _target = target;
            _componentType = GetTypeFromName(componentTypeName);
        }

        public bool Validate(out string errorMessage)
        {
            if (_target == null)
            {
                errorMessage = "Validation failure: Target GameObject was null.";
                return false;
            }
            if (_componentType == null)
            {
                errorMessage = "Validation failure: The requested component type could not be resolved in the Assembly.";
                return false;
            }
            
            errorMessage = string.Empty;
            return true;
        }

        public void Execute()
        {
            // Leverages Unity's internal Undo system explicitly so the user can just press CTRL+Z if the AI hallucinated.
            Undo.AddComponent(_target, _componentType);
            Debug.Log($"[AgentBridge] Successfully attached component '{_componentType.Name}' to '{_target.name}'.");
        }

        /// <summary>
        /// A robust lookup through the AppDomain to capture string-passed types.
        /// </summary>
        private Type GetTypeFromName(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.Name == name && typeof(Component).IsAssignableFrom(type))
                        return type;
                }
            }
            return null;
        }
    }
}
