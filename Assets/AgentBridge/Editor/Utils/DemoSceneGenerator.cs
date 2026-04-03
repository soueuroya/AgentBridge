using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AgentBridge.Editor.Utils
{
    /// <summary>
    /// Programmatically generates a highly tailored Unity Scene containing specifically 'broken' assets
    /// optimized for demonstrating AgentBridge AI resolution capabilities locally.
    /// This resolves the issue of being unable to distribute raw complex Unity YAML files dynamically via text.
    /// </summary>
    public static class DemoSceneGenerator
    {
        private const string TempFolder = "Assets/AgentBridge_DemoBuilder";

        [MenuItem("Window/AgentBridge/Tests/Generate Robust Demo Scene", false, 200)]
        public static void GenerateDemoEnvironment()
        {
            if (AssetDatabase.IsValidFolder(TempFolder))
            {
                if (!EditorUtility.DisplayDialog("Overwrite", "The 'AgentBridge_DemoBuilder' folder already exists in root Assets. Overwrite it?", "Yes", "No"))
                    return;
                
                AssetDatabase.DeleteAsset(TempFolder);
            }

            AssetDatabase.CreateFolder("Assets", "AgentBridge_DemoBuilder");

            // 1. Generate Contextual Testing Files
            CreateMockTexture();
            CreateMockAudio();

            // 2. Generate the robust architectural Demo Scene
            CreateDemoScene();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[AgentBridge] Successfully fabricated Demo environment natively in {TempFolder}. Open the scene, test the Context Resolvers, and manually drag the folder into 'Packages' or 'Samples~' when ready for Git Distribution!");
        }

        private static void CreateMockTexture()
        {
            Texture2D tex = new Texture2D(1024, 1024, TextureFormat.RGBA32, false);
            Color[] colors = new Color[1024 * 1024];
            for (int i = 0; i < colors.Length; i++) colors[i] = new Color(0.8f, 0.2f, 0.9f);
            tex.SetPixels(colors);
            tex.Apply();

            byte[] pngData = tex.EncodeToPNG();
            File.WriteAllBytes(Path.Combine(Application.dataPath, "AgentBridge_DemoBuilder/Overblown_Texture_HighRes.png"), pngData);
            
            AssetDatabase.Refresh();
        }

        private static void CreateMockAudio()
        {
            // Dummy byte writing for Context testing
            File.WriteAllText(Path.Combine(Application.dataPath, "AgentBridge_DemoBuilder/agent_demo_vo_audio.wav"), "dummy_audio_bytes_simulate_waveform");
            AssetDatabase.Refresh();
        }

        private static void CreateDemoScene()
        {
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Establish standard environment baseline
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            Camera cam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
            camObj.transform.position = new Vector3(0, 5, -10);
            camObj.transform.rotation = Quaternion.Euler(20, 0, 0);

            GameObject lightObj = new GameObject("Directional Light");
            Light dirLight = lightObj.AddComponent<Light>();
            dirLight.type = LightType.Directional;
            lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);

            // Build structural floor layout
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Environment_Ground_Plane";
            floor.transform.localScale = new Vector3(5, 1, 5);

            Material floorMat = new Material(Shader.Find("Standard"));
            floorMat.color = Color.gray;
            AssetDatabase.CreateAsset(floorMat, TempFolder + "/FloorMaterial.mat");
            floor.GetComponent<MeshRenderer>().sharedMaterial = floorMat;

            // Generate testable optimal character
            GameObject goodHero = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            goodHero.name = "Player_Active_Hero";
            goodHero.transform.position = new Vector3(-2, 1, 0);
            goodHero.AddComponent<Rigidbody>();
            
            Material heroMat = new Material(Shader.Find("Standard"));
            heroMat.color = Color.blue;
            AssetDatabase.CreateAsset(heroMat, TempFolder + "/HeroMaterial.mat");
            goodHero.GetComponent<MeshRenderer>().sharedMaterial = heroMat;

            // Generate poorly-optimized items for the AI to "Fix"
            GameObject badHero = new GameObject("player_character_finalV3_NO_COMPONENTS");
            badHero.transform.position = new Vector3(2, 0, 0);

            var childMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            childMesh.name = "mesh_target";
            childMesh.transform.position = new Vector3(2, 0.5f, 0);
            childMesh.transform.SetParent(badHero.transform);

            // Broken UI System
            GameObject brokenCanvas = new GameObject("Broken_UI_HUD");
            brokenCanvas.AddComponent<Canvas>();
            GameObject textNode = new GameObject("title_text");
            textNode.transform.SetParent(brokenCanvas.transform);

            // Export tracking
            string prefabPath = TempFolder + "/Player_BrokenInstance.prefab";
            PrefabUtility.SaveAsPrefabAsset(badHero, prefabPath);

            EditorSceneManager.SaveScene(newScene, TempFolder + "/AgentBridge_Demonstration.unity");
        }
    }
}
