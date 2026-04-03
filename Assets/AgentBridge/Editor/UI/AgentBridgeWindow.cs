using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AgentBridge.Core.Registry;
using AgentBridge.Editor.Execution;
using AgentBridge.Core.Context;
using AgentBridge.Core.MCP;
using AgentBridge.Core.Providers;
using UnityEditor;
using UnityEngine;

namespace AgentBridge.Editor.UI
{
    /// <summary>
    /// The primary native frontend for AgentBridge.
    /// Radically deviates from ChatBot UIs by prioritizing structured button interactions and discrete logs.
    /// </summary>
    public class AgentBridgeWindow : EditorWindow
    {
        private CapabilityRegistry _registry;
        private CommandExecutionEngine _engine;
        private ProviderConfiguration _providerConfig;

        private Vector2 _scrollPos;
        private Vector2 _inspectorScrollPos;
        private List<string> _executionLogs = new List<string>();
        private bool _isBridgeConnected;
        private double _nextStatusCheck;

        [MenuItem("Window/AgentBridge")]
        public static void ShowWindow()
        {
            AgentBridgeWindow window = GetWindow<AgentBridgeWindow>("AgentBridge");
            window.minSize = new Vector2(300, 400);
            window.titleContent = new GUIContent("AgentBridge");
            window.Show();
        }

        private void OnEnable()
        {
            // Subscribe to whenever the user clicks different objects in the Editor Hierarchy/Project.
            Selection.selectionChanged += OnSelectionChanged;
            LoadDependencies();
            titleContent = new GUIContent("AgentBridge");
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= OnSelectionChanged;
        }

        private void OnSelectionChanged()
        {
            // Force the window to re-draw whenever selection changes so Context/Capabilities adjust instantly.
            Repaint();
        }

        private void LoadDependencies()
        {
            // Lazily track down the core configuration assets initially if they are missing
            if (_registry == null)
            {
                string[] registryGuids = AssetDatabase.FindAssets("t:CapabilityRegistry");
                if (registryGuids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(registryGuids[0]);
                    _registry = AssetDatabase.LoadAssetAtPath<CapabilityRegistry>(path);
                }
            }

            if (_engine == null)
            {
                string[] engineGuids = AssetDatabase.FindAssets("t:CommandExecutionEngine");
                if (engineGuids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(engineGuids[0]);
                    _engine = AssetDatabase.LoadAssetAtPath<CommandExecutionEngine>(path);
                }
            }

            if (_providerConfig == null)
            {
                string[] configGuids = AssetDatabase.FindAssets("t:ProviderConfiguration");
                if (configGuids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(configGuids[0]);
                    _providerConfig = AssetDatabase.LoadAssetAtPath<ProviderConfiguration>(path);
                }
            }
        }

        private int _selectedTab = 0;
        private string[] _tabNames = { "Context Inspector", "System Configuration" };

        private void OnGUI()
        {
            EditorGUILayout.Space();
            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabNames, GUILayout.Height(30));
            EditorGUILayout.Space();
            
            if (_selectedTab == 0)
            {
                _inspectorScrollPos = EditorGUILayout.BeginScrollView(_inspectorScrollPos);
                DrawContextSection();
                DrawActionsSection();
                EditorGUILayout.EndScrollView();
            }
            else
            {
                DrawStatusIndicator();
                DrawHeader();
                DrawLogsSection();
            }
        }

        private void DrawStatusIndicator()
        {
            if (EditorApplication.timeSinceStartup > _nextStatusCheck)
            {
                CheckBridgeStatus();
                _nextStatusCheck = EditorApplication.timeSinceStartup + 3.0;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            Rect rect = EditorGUILayout.GetControlRect(false, 30);
            EditorGUI.DrawRect(rect, new Color(0.12f, 0.12f, 0.13f));
            
            Rect statusRect = new Rect(rect.x + 10, rect.y + 10, 10, 10);
            EditorGUI.DrawRect(statusRect, _isBridgeConnected ? new Color(0.2f, 0.8f, 0.2f) : new Color(0.8f, 0.2f, 0.2f));
            
            GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel);
            labelStyle.normal.textColor = _isBridgeConnected ? Color.white : new Color(0.8f, 0.5f, 0.5f);
            
            string statusText = _isBridgeConnected 
                ? "Antigravity Real-Time Link: CONNECTED" 
                : "Antigravity Real-Time Link: DISCONNECTED";
            
            EditorGUI.LabelField(new Rect(rect.x + 25, rect.y + 5, rect.width - 150, 20), statusText, labelStyle);

            if (!_isBridgeConnected)
            {
                if (GUI.Button(new Rect(rect.x + rect.width - 140, rect.y + 5, 130, 20), "Start Link Service"))
                {
                    StartBridgeService();
                }
            }
            else
            {
                if (GUI.Button(new Rect(rect.x + rect.width - 140, rect.y + 5, 130, 20), "Restart Service"))
                {
                    StartBridgeService();
                }
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void StartBridgeService()
        {
            try
            {
                string scriptPath = System.IO.Path.Combine(Application.dataPath, "../BridgeLinkNode.ps1");
                string absolutePath = System.IO.Path.GetFullPath(scriptPath);

                if (!System.IO.File.Exists(absolutePath))
                {
                    Debug.LogError($"[AgentBridge] Link script not found at: {absolutePath}");
                    return;
                }

                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoExit -ExecutionPolicy Bypass -File \"{absolutePath}\"",
                    UseShellExecute = true,
                    CreateNoWindow = false,
                    WorkingDirectory = System.IO.Path.GetDirectoryName(absolutePath)
                };

                System.Diagnostics.Process.Start(startInfo);
                _executionLogs.Add("[System] Initializing Antigravity Real-Time Link...");
                _nextStatusCheck = EditorApplication.timeSinceStartup + 1.0;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[AgentBridge] Failed to start Link service: {e.Message}");
            }
        }

        private async void CheckBridgeStatus()
        {
            using (var www = UnityEngine.Networking.UnityWebRequest.Get("http://localhost:11500/ping"))
            {
                www.timeout = 1;
                var operation = www.SendWebRequest();
                while (!operation.isDone) await Task.Yield();
                
                _isBridgeConnected = www.result == UnityEngine.Networking.UnityWebRequest.Result.Success 
                                     && www.downloadHandler.text == "pong";
                Repaint();
            }
        }

        private void DrawHeader()
        {
            GUILayout.Label("AgentBridge System Bindings", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Allow developers to physically swap or drag/drop distinct Engine or Registry configs.
            _registry = (CapabilityRegistry)EditorGUILayout.ObjectField("Capability Registry", _registry, typeof(CapabilityRegistry), false);
            _engine = (CommandExecutionEngine)EditorGUILayout.ObjectField("Execution Engine", _engine, typeof(CommandExecutionEngine), false);
            _providerConfig = (ProviderConfiguration)EditorGUILayout.ObjectField("AI Provider Config", _providerConfig, typeof(ProviderConfiguration), false);
            
            if (_registry == null || _engine == null || _providerConfig == null)
            {
                EditorGUILayout.HelpBox("AgentBridge requires one or more configuration assets to be assigned. Click below to generate a default suite.", MessageType.Warning);
                
                // Fallback feature to generate them automatically for users who haven't built custom ones yet
                if (GUILayout.Button("Create All Default Configurations", GUILayout.Height(30)))
                {
                    CreateDefaultConfigurations();
                }
            }
            else
            {
                // If they are assigned but empty (e.g. from an older version), provide a repair button
                if (_registry.RegisteredActions.Count == 0 || _engine.Whitelist.Count <= 3)
                {
                    EditorGUILayout.HelpBox("Your currently assigned Engine or Registry appears mostly empty. You can auto-generate the massive list of default capabilities below.", MessageType.Info);
                    if (GUILayout.Button("Populate Missing Default Actions", GUILayout.Height(30)))
                    {
                        PopulateAssignedConfigurations();
                    }
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        private void PopulateAssignedConfigurations()
        {
            if (!AssetDatabase.IsValidFolder("Assets/AgentBridge/Settings/Actions"))
            {
                AssetDatabase.CreateFolder("Assets/AgentBridge/Settings", "Actions");
            }

            if (_registry != null)
            {
                var newActions = new List<AgentAction>
                {
                    GenerateAndSaveAction("RefactorScript", "Deep scans the MonoScript for architectural flaws and restructures code to idiomatic C# best practices.", "MonoScript"),
                    GenerateAndSaveAction("GenerateDocumentation", "Injects clean, standardized XML summary comments above all public methods natively.", "MonoScript"),
                    GenerateAndSaveAction("GenerateUnitTests", "Produces a companion Editor test script validating the target's public API constraints.", "MonoScript"),
                    GenerateAndSaveAction("FindSecurityFlaws", "Performs secure semantic analysis regarding GC spikes and memory allocations.", "MonoScript"),

                    GenerateAndSaveAction("RenameGameObject", "Intelligently parses the GameObject's role and assigns it a standardized camelCase or PascalCase node tracking name.", "GameObject"),
                    GenerateAndSaveAction("AddMissingComponents", "Scans the GameObject's Context and appends implicit rigidbodies, colliders, or UI canvases that the AI determines form its conceptual model.", "GameObject"),
                    GenerateAndSaveAction("AuditHierarchyOptimizations", "Iterates downward through children looking for empty game objects, redundant canvases, or deep-nested structural warnings.", "GameObject"),

                    GenerateAndSaveAction("OptimizeTexture", "Reviews import settings and forces proper Mobile/PC crunch compression blocks, trilinear filtering, and non-power-of-two constraints.", "Texture2D"),
                    GenerateAndSaveAction("GenerateNormalMap", "Uses AI heuristics to rip simulated depth from standard diffuse layouts, producing a secondary texture asset.", "Texture2D"),

                    GenerateAndSaveAction("TrimAudioSilence", "Strips away dead air bytes at the beginning/end of clips, truncating memory footprints.", "AudioClip"),
                    GenerateAndSaveAction("NormalizeVoiceOver", "Equalizes dynamic audio frequencies natively to a standardized -3db mixing envelope.", "AudioClip"),

                    GenerateAndSaveAction("BakeLightProbes", "Automatically distributes logical lighting probe spheres throughout a parsed structural scene volume.", "SceneAsset"),
                    GenerateAndSaveAction("AuditDrawCalls", "Evaluates static batching overlaps and provides a generated breakdown report artifact inside the project.", "SceneAsset"),
                    
                    GenerateAndSaveAction("AnalyzeAndImprove", "Holistic overarching system scan capable of projecting diff recommendations across any generic target.", "*")
                };

                // Add to existing registry rather than overwriting custom user additions
                foreach (var act in newActions)
                {
                    if (!_registry.RegisteredActions.Any(a => a != null && a.ActionName == act.ActionName))
                    {
                        _registry.RegisteredActions.Add(act);
                    }
                }
                EditorUtility.SetDirty(_registry);
            }

            if (_engine != null)
            {
                var newWhitelist = new List<string> {
                    "AddComponent", "RenameAsset", "ModifyImportSettings", "RenameGameObject", 
                    "RefactorScript", "GenerateDocumentation", "GenerateUnitTests", "FindSecurityFlaws",
                    "AddMissingComponents", "AuditHierarchyOptimizations", "OptimizeTexture", 
                    "GenerateNormalMap", "TrimAudioSilence", "NormalizeVoiceOver", 
                    "BakeLightProbes", "AuditDrawCalls", "AnalyzeAndImprove"
                };

                foreach (var rule in newWhitelist)
                {
                    if (!_engine.Whitelist.Contains(rule))
                    {
                        _engine.Whitelist.Add(rule);
                    }
                }
                EditorUtility.SetDirty(_engine);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[AgentBridge] Successfully populated the assigned configurations with robust default tools.");
        }

        private void CreateDefaultConfigurations()
        {
            // Guarantee parent constraints exist
            if (!AssetDatabase.IsValidFolder("Assets/AgentBridge"))
            {
                AssetDatabase.CreateFolder("Assets", "AgentBridge");
            }
            if (!AssetDatabase.IsValidFolder("Assets/AgentBridge/Settings"))
            {
                AssetDatabase.CreateFolder("Assets/AgentBridge", "Settings");
            }

            // Provide default action repository
            if (!AssetDatabase.IsValidFolder("Assets/AgentBridge/Settings/Actions"))
            {
                AssetDatabase.CreateFolder("Assets/AgentBridge/Settings", "Actions");
            }

            // Map and Save Registry
            if (_registry == null)
            {
                CapabilityRegistry reg = ScriptableObject.CreateInstance<CapabilityRegistry>();
                AssetDatabase.CreateAsset(reg, "Assets/AgentBridge/Settings/DefaultCapabilityRegistry.asset");
                _registry = reg;
            }

            // Map and Save Engine
            if (_engine == null)
            {
                CommandExecutionEngine eng = ScriptableObject.CreateInstance<CommandExecutionEngine>();
                AssetDatabase.CreateAsset(eng, "Assets/AgentBridge/Settings/DefaultExecutionEngine.asset");
                _engine = eng;
            }

            // Map and Save Provider
            if (_providerConfig == null)
            {
                ProviderConfiguration config = ScriptableObject.CreateInstance<ProviderConfiguration>();
                config.ModelName = "Antigravity Real-Time Link";
                config.UseFileStream = false; 
                config.UseRealTimeLink = true;
                config.TimeoutSeconds = 600; // 10 minute headroom for complex agent actions
                AssetDatabase.CreateAsset(config, "Assets/AgentBridge/Settings/DefaultAntigravityConfig.asset");
                _providerConfig = config;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            PopulateAssignedConfigurations();
        }

        private AgentAction GenerateAndSaveAction(string name, string desc, string ctx)
        {
            AgentAction action = ScriptableObject.CreateInstance<AgentAction>();
            action.ActionName = name;
            action.Description = desc;
            action.SupportedContextTypes = new List<string> { ctx };
            action.Parameters = new List<ActionParameter>();
            
            AssetDatabase.CreateAsset(action, $"Assets/AgentBridge/Settings/Actions/AIAction_{name}.asset");
            return action;
        }

        private void DrawContextSection()
        {
            if (_registry == null || _engine == null)
            {
                EditorGUILayout.HelpBox("AgentBridge is unconfigured. Please switch to the System Configuration tab to bind core engines first.", MessageType.Warning);
                return;
            }

            if (Selection.activeObject != null)
            {
                EditorGUILayout.LabelField(
                    string.Format("Current Selection Context: {0} - {1}", Selection.activeObject.name, Selection.activeObject.GetType().Name),
                    EditorStyles.boldLabel
                );
                EditorGUI.indentLevel++;
                
                // Extra UI polish based on file path integrations
                if (AssetDatabase.Contains(Selection.activeObject))
                {
                    EditorGUILayout.LabelField("Path", AssetDatabase.GetAssetPath(Selection.activeObject));
                }
            }
            else
            {
                EditorGUILayout.LabelField("No internal Object Selected", EditorStyles.helpBox);
                EditorGUI.indentLevel++;
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        private void DrawActionsSection()
        {
            if (_registry == null || _engine == null) return;

            EditorGUILayout.LabelField("Supported AI Actions", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            if (Selection.activeObject == null)
            {
                EditorGUILayout.LabelField("Select any object in Unity to view mapped capabilities.", EditorStyles.wordWrappedLabel);
                EditorGUI.indentLevel--;
                return;
            }

            // Dynamically rip context arrays including structural sub-modifiers
            HashSet<string> contextTypes = new HashSet<string>();
            contextTypes.Add(Selection.activeObject.GetType().Name);

            if (Selection.activeObject is GameObject go)
            {
                contextTypes.Add("GameObject");
                
                // Harvest every attached component natively
                var components = go.GetComponents<Component>();
                foreach(var comp in components)
                {
                    if (comp != null)
                    {
                        contextTypes.Add(comp.GetType().Name);
                    }
                }
            }

            // Aggregate all matching capabilities gracefully
            HashSet<AgentAction> availableActions = new HashSet<AgentAction>();
            foreach(var contextBlock in contextTypes)
            {
                var mapped = _registry.GetActionsForContext(contextBlock);
                foreach(var map in mapped) 
                {
                    availableActions.Add(map);
                }
            }

            if (availableActions.Count == 0)
            {
                EditorGUILayout.HelpBox($"No AI Capabilities are currently mapped to the context type: {string.Join(", ", contextTypes)}.", MessageType.Info);
            }
            else
            {
                foreach (var action in availableActions)
                {
                    DrawActionBox(action);
                }
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
        }

        private void DrawActionBox(AgentAction action)
        {
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label(action.ActionName, EditorStyles.boldLabel);
            GUILayout.Label(action.Description, EditorStyles.wordWrappedLabel);

            if (GUILayout.Button($"Execute {action.ActionName}..."))
            {
                ExecuteAction(action);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }

        private async void ExecuteAction(AgentAction action)
        {
            if (Selection.activeObject == null) return;
            
            string targetName = Selection.activeObject.name;
            Log($"[System] Starting interaction: '{action.ActionName}' for object '{targetName}'...");
            
            try 
            {
                // 1. Build the Unified Context for the Object
                var unityContext = new UnityObjectContext(Selection.activeObject);
                
                // 2. Wrap into an MCP-compliant Request
                var mcpRequest = McpRequestBuilder.BuildRequest(action, unityContext);
                string jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(mcpRequest);
                
                Log($"[MCP] Request built ({jsonPayload.Length} chars). Targeting Tool: {mcpRequest.tool}");

                // 3. Select Provider
                IAiProvider provider;
                if (_providerConfig != null && _providerConfig.UseRealTimeLink)
                {
                    provider = new AntigravityLinkProvider(_providerConfig);
                }
                else if (_providerConfig != null && _providerConfig.UseFileStream)
                {
                    provider = new AntigravityFileProvider(_providerConfig);
                }
                else if (_providerConfig != null && !string.IsNullOrEmpty(_providerConfig.EndpointUrl))
                {
                    provider = new GenericHttpProvider(_providerConfig);
                }
                else
                {
                    Log("[Warning] No external HTTP AI Provider configured. Utilizing the built-in Mock Testing Provider for simulation.");
                    provider = new MockProvider();
                }

                // 4. Dispatch and Wait
                Log($"[AI] Dispatching request to: {provider.Name}...");
                
                // Cancellation token logic can be added here if needed to avoid hanging threads
                string response = await provider.SendRequestAsync(jsonPayload);
                
                Log($"[AI] Response Received: {response}");

                // 5. Parse and Execute Command
                IAgentCommand command = CommandFactory.ParseResponse(response);
                if (command != null)
                {
                    if (_engine.TryExecute(command, out string resultMessage))
                    {
                        Log($"[System] Execution Success: {resultMessage}");
                    }
                    else
                    {
                        Log($"[Error] Execution Failed: {resultMessage}");
                    }
                }
                else
                {
                    Log("[Info] AI response did not contain a valid executable command or target was not found.");
                }

                Log($"[Success] '{action.ActionName}' successfully finished communication cycle.");
            }
            catch (System.Exception ex) 
            {
                Log($"[Error] Action Execution Failed: {ex.Message}");
                Debug.LogException(ex);
            }
        }

        private void DrawLogsSection()
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("System Output", EditorStyles.boldLabel);
            if (GUILayout.Button("Copy All", GUILayout.Width(70)))
            {
                EditorGUIUtility.systemCopyBuffer = string.Join("\n", _executionLogs);
                Debug.Log("[AgentBridge] Successfully copied all logs to system clipboard.");
            }
            EditorGUILayout.EndHorizontal();
            
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, "box", GUILayout.ExpandHeight(true));
            
            foreach (var log in _executionLogs)
            {
                // Calculate height dynamically to support word wrapping in a selectable field
                float height = EditorStyles.wordWrappedLabel.CalcHeight(new GUIContent(log), position.width - 30);
                EditorGUILayout.SelectableLabel(log, EditorStyles.wordWrappedLabel, GUILayout.Height(height));
            }
            
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Clear Logs"))
            {
                _executionLogs.Clear();
            }
        }

        private void Log(string message)
        {
            _executionLogs.Insert(0, $"[{System.DateTime.Now:HH:mm:ss}] {message}");
            // Frame limit ceiling protections
            if (_executionLogs.Count > 50)
            {
                _executionLogs.RemoveAt(_executionLogs.Count - 1);
            }
        }
    }
}
