using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgentBridge.Editor.Execution;

namespace AgentBridge.Editor.UI
{
    /// <summary>
    /// AgentBridge's "Analyze and Improve" UI frontend.
    /// Pushes overarching holistic data to the AI and parses out multiple non-destructive suggestions,
    /// giving the Developer explicit manual override control on what physically executes.
    /// </summary>
    public class AnalyzeAndImproveWindow : EditorWindow
    {
        private CommandExecutionEngine _engine;
        private List<SuggestedAction> _suggestions = new List<SuggestedAction>();
        private bool _isAnalyzing = false;
        private Vector2 _scrollPos;

        [MenuItem("Window/AgentBridge/Analyze and Improve")]
        public static void ShowWindow()
        {
            var window = GetWindow<AnalyzeAndImproveWindow>("Analyze & Improve");
            window.minSize = new Vector2(400, 500);
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
            GUILayout.Label("AI Analysis & Optimization Engine", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Select a complex object and ask the AI for a comprehensive multithreaded review. It will yield independent action points for your approval.", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space();

            if (Selection.activeObject == null)
            {
                EditorGUILayout.HelpBox("Select a Hierarchy GameObject or raw Asset to begin Analysis.", MessageType.Info);
                return;
            }

            if (_isAnalyzing)
            {
                EditorGUILayout.HelpBox("Establishing Provider connection... Analyzing current context.", MessageType.Warning);
                return;
            }

            if (GUILayout.Button("Deep Analyze Current Selection", GUILayout.Height(40)))
            {
                RunAnalysisRoutineAsync();
            }

            EditorGUILayout.Space();
            DrawSuggestions();
        }

        /// <summary>
        /// Fires off the payload, waits, and parses out the suggestions array.
        /// (Mocking the AI Network/Compute delay here)
        /// </summary>
        private async void RunAnalysisRoutineAsync()
        {
            _isAnalyzing = true;
            _suggestions.Clear();
            
            // Simulating real network delay asynchronously
            await Task.Delay(1500);
            
            // Simulating parsing out the AI JSON payload
            _suggestions.Add(new SuggestedAction 
            { 
                CommandName = "AddComponent", 
                Reason = "The object lacks a Rigidbody but implies physical interactions based on Context parameters.",
                RawJsonParameters = "{ \"componentTypeName\": \"Rigidbody\" }"
            });

            _suggestions.Add(new SuggestedAction 
            { 
                CommandName = "RenameAsset", 
                Reason = "Local standard naming requires UpperCamelCase formatting. Attempting to rename from fallback defaults.",
                RawJsonParameters = "{ \"newName\": \"PlayerCharacter\" }"
            });

            _isAnalyzing = false;
            Repaint();
        }

        private void DrawSuggestions()
        {
            if (_suggestions.Count == 0 && !_isAnalyzing) return;

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Suggested Actions:", EditorStyles.boldLabel);
            
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, "box");

            for (int i = _suggestions.Count - 1; i >= 0; i--)
            {
                var suggestion = _suggestions[i];
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                GUILayout.Label($"Target Capability: {suggestion.CommandName}", EditorStyles.boldLabel);
                GUILayout.Label($"Reasoning: {suggestion.Reason}", EditorStyles.wordWrappedLabel);
                
                EditorGUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();
                
                // UX: Giving Explicit approval forces developer ownership of AI changes
                GUI.backgroundColor = Color.green;
                if (GUILayout.Button("Approve", GUILayout.Width(100)))
                {
                    ApproveAction(suggestion);
                    _suggestions.RemoveAt(i);
                }
                
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("Reject", GUILayout.Width(100)))
                {
                    _suggestions.RemoveAt(i);
                }
                GUI.backgroundColor = Color.white;
                
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2);
            }

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// Attempts to convert a raw JSON AI suggestion into an IAgentCommand mapped explicitly through the Execution Engine.
        /// </summary>
        private void ApproveAction(SuggestedAction suggestion)
        {
            if (_engine == null)
            {
                Debug.LogError("[AgentBridge] CRITICAL: CommandExecutionEngine is unmapped!");
                return;
            }

            IAgentCommand generatedCommand = null;

            // In production, reflection parses `RawJsonParameters` to construct the relevant implementation class seamlessly.
            // Using a structural factory mock here since NewtonSoft dependencies aren't loaded in local package bounds yet.
            if (suggestion.CommandName == "AddComponent" && Selection.activeGameObject != null)
                generatedCommand = new Execution.Commands.AddComponentCommand(Selection.activeGameObject, "Rigidbody");
            else if (suggestion.CommandName == "RenameAsset" && Selection.activeObject != null)
                generatedCommand = new Execution.Commands.RenameAssetCommand(AssetDatabase.GetAssetPath(Selection.activeObject), "PlayerCharacter");

            if (generatedCommand != null)
            {
                if (_engine.TryExecute(generatedCommand, out string resultMessage))
                {
                    Debug.Log($"[Analyze] Successfully applied Action: {suggestion.CommandName}. Result: {resultMessage}");
                }
            }
            else
            {
                Debug.LogError($"[Analyze] Command Factory Failed to serialize the Raw Json into a Unity Command for '{suggestion.CommandName}'.");
            }
        }
    }
}
