using UnityEditor;
using UnityEngine;
using AgentBridge.Core.Registry;
using System.Linq;

namespace AgentBridge.Editor.UI
{
    /// <summary>
    /// Injects dynamic AgentBridge capabilities directly into the standard Unity right-click Context Menus.
    /// Supports both the Hierarchy (GameObjects) and Project window (Assets).
    /// </summary>
    public static class AgentBridgeContextMenu
    {
        // Bind to Hierarchy Right-Click
        [MenuItem("GameObject/AgentBridge AI Actions...", false, -10)]
        public static void ShowGameObjectMenu()
        {
            SpawnDynamicMenu();
        }

        // Bind to Project Window Right-Click (Assets)
        [MenuItem("Assets/AgentBridge AI Actions...", false, -10)]
        public static void ShowAssetMenu()
        {
            SpawnDynamicMenu();
        }

        /// <summary>
        /// Validates if we have anything selected to operate on.
        /// </summary>
        [MenuItem("GameObject/AgentBridge AI Actions...", true)]
        [MenuItem("Assets/AgentBridge AI Actions...", true)]
        public static bool ValidateMenu()
        {
            return Selection.activeObject != null;
        }

        /// <summary>
        /// Renders a dynamic Unity GenericMenu at the cursor, building buttons explicitly 
        /// out of the CapabilityRegistry matching the currently selected context type.
        /// </summary>
        private static void SpawnDynamicMenu()
        {
            if (Selection.activeObject == null) return;

            // Simple type parsing. Will pipe to IContextResolvers later.
            string activeType = Selection.activeObject.GetType().Name;
            if (Selection.activeObject is GameObject) activeType = "GameObject";

            // Find global capabilities mapping
            string[] registryGuids = AssetDatabase.FindAssets("t:CapabilityRegistry");
            if (registryGuids.Length == 0)
            {
                Debug.LogWarning("[AgentBridge] Context menu failed. No 'CapabilityRegistry' ScriptableObject found in the project.");
                return;
            }

            string path = AssetDatabase.GUIDToAssetPath(registryGuids[0]);
            CapabilityRegistry registry = AssetDatabase.LoadAssetAtPath<CapabilityRegistry>(path);
            
            if (registry == null) return;

            var validActions = registry.GetActionsForContext(activeType).ToList();

            GenericMenu menu = new GenericMenu();

            if (validActions.Count == 0)
            {
                menu.AddDisabledItem(new GUIContent($"No AI Actions mapped for type: {activeType}"));
            }
            else
            {
                foreach (var action in validActions)
                {
                    // Add items natively into the generated dropdown
                    menu.AddItem(new GUIContent(action.ActionName), false, () => 
                    {
                        ExecuteActionFromMenu(action);
                    });
                }
            }

            // Display directly at the user's current mouse position
            menu.ShowAsContext();
        }

        private static void ExecuteActionFromMenu(AgentAction action)
        {
            string targetName = Selection.activeObject != null ? Selection.activeObject.name : "Unknown Target";
            Debug.Log($"[AgentBridge Context] Triggered '{action.ActionName}' execution directly onto '{targetName}'.");
            
            // In a live environment, this would package an MCPRequest manually and forward to the IAiProvider,
            // identical to pushing the button in the Hub window!
        }
    }
}
