using System;
using System.Collections.Generic;
using AgentBridge.Core.Providers;
using AgentBridge.Editor.Execution.Commands;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;

namespace AgentBridge.Editor.Execution
{
    /// <summary>
    /// Factory responsible for converting raw AI JSON responses into executable IAgentCommand instances.
    /// Manages reference resolution for Unity objects and parameter mapping.
    /// </summary>
    public static class CommandFactory
    {
        public static IAgentCommand ParseResponse(string jsonResponse)
        {
            if (string.IsNullOrEmpty(jsonResponse)) return null;

            try
            {
                AiExecutionResponse response = JsonConvert.DeserializeObject<AiExecutionResponse>(jsonResponse);
                if (response == null || string.IsNullOrEmpty(response.command)) return null;

                return MapToCommand(response);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AgentBridge] Failed to parse AI response: {ex.Message}");
                return null;
            }
        }

        private static IAgentCommand MapToCommand(AiExecutionResponse response)
        {
            GameObject target = ResolveTarget(response);

            switch (response.command)
            {
                case "AddComponent":
                    if (target != null && response.parameters.TryGetValue("component", out string componentType))
                    {
                        return new AddComponentCommand(target, componentType);
                    }
                    break;

                case "RenameGameObject":
                    if (target != null && response.parameters.TryGetValue("newName", out string newName))
                    {
                        return new RenameGameObjectCommand(target, newName);
                    }
                    break;

                case "RenameAsset":
                    if (response.parameters.TryGetValue("path", out string assetPath) && 
                        response.parameters.TryGetValue("newName", out string newAssetName))
                    {
                        return new RenameAssetCommand(assetPath, newAssetName);
                    }
                    break;

                case "ModifyImportSettings":
                    if (response.parameters.TryGetValue("path", out string importPath))
                    {
                        return new ModifyImportSettingsCommand(importPath);
                    }
                    break;

                // Add more mappings as new commands are implemented
                default:
                    Debug.LogWarning($"[AgentBridge] Unknown command received from AI: {response.command}");
                    break;
            }

            return null;
        }

        private static GameObject ResolveTarget(AiExecutionResponse response)
        {
            // Try resolving by InstanceID first (most reliable)
            if (response.targetInstanceId != 0)
            {
                UnityEngine.Object obj = EditorUtility.EntityIdToObject(response.targetInstanceId);
                if (obj is GameObject go) return go;
            }

            // Fallback to name-based lookup in scene
            if (!string.IsNullOrEmpty(response.targetName))
            {
                return GameObject.Find(response.targetName);
            }

            return null;
        }
    }
}
