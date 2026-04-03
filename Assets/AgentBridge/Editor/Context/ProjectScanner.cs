using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using AgentBridge.Core.Interfaces;

namespace AgentBridge.Editor.Context
{
    /// <summary>
    /// Serves as the overarching JSON layout that an AI provider will ingest to understand the raw architecture.
    /// Clean arrays prevent token limits from instantly overloading when querying entire repositories.
    /// </summary>
    [System.Serializable]
    public class ProjectScanSummary
    {
        public int TotalGrossAssetsEvaluated;
        public AssetCategorySummary Textures = new AssetCategorySummary();
        public AssetCategorySummary Audio = new AssetCategorySummary();
        public AssetCategorySummary Scripts = new AssetCategorySummary();
        public AssetCategorySummary Scenes = new AssetCategorySummary();
    }

    [System.Serializable]
    public class AssetCategorySummary
    {
        public int TotalCount;
        
        // Pushing all 25,000 project texture paths inside an LLM's context window will instantly crash almost any provider's token limit.
        // This ensures the AI understands structurally *where* things are generally populated without brute forcing it.
        public List<string> RepresentativeSamplePaths = new List<string>();
    }

    /// <summary>
    /// Translates raw ProjectScan summaries into explicitly unified Context Models that the McpRequestBuilder understands natively.
    /// </summary>
    public class ProjectScannerContext : IActionContext
    {
        public string ContextType => "HolisticProjectScan";
        private ProjectScanSummary _summary;

        public ProjectScannerContext(ProjectScanSummary summary)
        {
            _summary = summary;
        }

        public object GetRawData()
        {
            return _summary; // Returns raw cleanly for standard JSON generation.
        }
    }

    /// <summary>
    /// Rapidly indexes bulk amounts of data without destroying Editor Memory or freezing threads.
    /// Used when the AI requires a holistic understanding of the entire repository structure.
    /// </summary>
    public static class ProjectScanner
    {
        [MenuItem("Window/AgentBridge/Tests/Print Project Scan Payload", false, 100)]
        public static void DebugPrintScan()
        {
            var summary = ScanProject();
            string json = JsonUtility.ToJson(summary, true);
            Debug.Log("[AgentBridge ProjectScanner] Successfully dumped optimized structural snapshot:\n" + json);
        }

        public static ProjectScanSummary ScanProject()
        {
            var summary = new ProjectScanSummary();

            // FindAssets uses Unity's highly optimized internal SQL-like SQLite Registry rather than physical disk reads.
            // Under NO circumstance should AssetDatabase.LoadAssetAtPath be called within this loop to preserve performance optimizations!
            
            summary.Textures.TotalCount = IndexCategory("t:Texture2D", summary.Textures.RepresentativeSamplePaths);
            summary.Audio.TotalCount = IndexCategory("t:AudioClip", summary.Audio.RepresentativeSamplePaths);
            summary.Scripts.TotalCount = IndexCategory("t:MonoScript", summary.Scripts.RepresentativeSamplePaths);
            summary.Scenes.TotalCount = IndexCategory("t:SceneAsset", summary.Scenes.RepresentativeSamplePaths);

            summary.TotalGrossAssetsEvaluated = summary.Textures.TotalCount + summary.Audio.TotalCount + summary.Scripts.TotalCount + summary.Scenes.TotalCount;

            return summary;
        }

        /// <summary>
        /// Extrapolates arrays, extracts counts, and intelligent string formatting for sample representations.
        /// </summary>
        private static int IndexCategory(string searchFilter, List<string> sampleTarget, int maxTokenSamples = 10)
        {
            string[] guids = AssetDatabase.FindAssets(searchFilter);
            
            for (int i = 0; i < guids.Length && i < maxTokenSamples; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (!string.IsNullOrEmpty(path))
                {
                    sampleTarget.Add(path);
                }
            }

            return guids.Length;
        }
    }
}
