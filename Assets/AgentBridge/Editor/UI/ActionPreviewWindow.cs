using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using AgentBridge.Editor.Execution;

namespace AgentBridge.Editor.UI
{
    /// <summary>
    /// The primary Interceptor Window.
    /// Instead of silently executing AI logic, batch operators or complex tasks route here manually 
    /// so the Developer sees exact 'Before & After' diffs prior to Engine validations triggering.
    /// </summary>
    public class ActionPreviewWindow : EditorWindow
    {
        private static List<IAgentCommand> _pendingCommands = new List<IAgentCommand>();
        private CommandExecutionEngine _engine;
        private Vector2 _scroll;

        /// <summary>
        /// Exposes a global hook for scripts to cleanly intercept execution safely natively in Unity contexts.
        /// </summary>
        public static void QueueCommand(IAgentCommand command)
        {
            _pendingCommands.Add(command);
            var window = GetWindow<ActionPreviewWindow>("Action Preview");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        private void OnEnable()
        {
            string[] engineGuids = AssetDatabase.FindAssets("t:CommandExecutionEngine");
            if (engineGuids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(engineGuids[0]);
                _engine = AssetDatabase.LoadAssetAtPath<CommandExecutionEngine>(path);
            }
        }

        private void OnGUI()
        {
            GUILayout.Label("AI Interception & Diff Preview", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("The following actions are pending execution queued by the AI Provider. Please verify the differential states critically.", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space();

            if (_pendingCommands.Count == 0)
            {
                EditorGUILayout.HelpBox("No external commands are pending preview verification.", MessageType.Info);
                return;
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            for (int i = _pendingCommands.Count - 1; i >= 0; i--)
            {
                var cmd = _pendingCommands[i];
                DrawPreviewBlock(cmd, i);
            }

            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.Space();
            if (GUILayout.Button("Reject All Remaining"))
            {
                _pendingCommands.Clear();
            }
        }

        private void DrawPreviewBlock(IAgentCommand cmd, int index)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(cmd.CommandName, EditorStyles.boldLabel);
            
            // Highlighting catastrophic/IsDangerous actions explicitly to the user
            if (cmd.IsDangerous)
            {
                var warnStyle = new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = Color.red } };
                GUILayout.Label("[DANGEROUS AI OVERRIDE]", warnStyle);
            }
            EditorGUILayout.EndHorizontal();

            // Core Diff Logic
            if (cmd is IPreviewableCommand previewable)
            {
                EditorGUILayout.LabelField("Before (Current):", previewable.GetBeforeState());
                EditorGUILayout.LabelField("After (Projected):", previewable.GetAfterState());
            }
            else
            {
                EditorGUILayout.HelpBox("This explicit command acts statically and does not support dynamic visual previews.", MessageType.Warning);
            }

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Approve & Execute", GUILayout.Width(150)))
            {
                // Execution cleanly triggers Unity Undo sequences here under the hood.
                if (_engine != null && _engine.TryExecute(cmd, out string msg))
                {
                    Debug.Log($"[Preview Controller] Successfully executed approved AI Action: {cmd.CommandName}");
                }
                _pendingCommands.RemoveAt(index);
            }
            
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Reject", GUILayout.Width(100)))
            {
                _pendingCommands.RemoveAt(index);
            }
            
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }
    }
}
