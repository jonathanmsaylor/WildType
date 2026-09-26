using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace WildType.Editor
{
    public static class PainterlyBuilder
    {
        public const string ScenePath="Assets/WildType/Scenes/CreatureStage_PainterlyPreview.unity";
        public const string MaterialPath="Assets/WildType/Art/Painterly/Painted meadow.mat";
        // Batch-only creation from a committed baseline copy, never the user's dirty scene.
        public static void Create()
        {
            if(File.Exists(ScenePath))throw new System.InvalidOperationException("Preview already exists; refusing to overwrite it.");
            string baseline=System.Environment.GetEnvironmentVariable("WILDTYPE_BASELINE_SCENE");
            if(string.IsNullOrEmpty(baseline)||!File.Exists(baseline))throw new System.InvalidOperationException("Explicit committed baseline required.");
            File.Copy(baseline,ScenePath);AssetDatabase.ImportAsset(ScenePath);
            var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
            var shader=Shader.Find("WildType/Painted Meadow");if(!shader)throw new System.Exception("Painterly shader missing");
            var material=new Material(shader){name="Painted meadow"};AssetDatabase.CreateAsset(material,MaterialPath);
            var preview=new GameObject("Painterly comparison (presentation only)").AddComponent<PainterlyPreview>();preview.paintedMaterial=material;
            EditorSceneManager.SaveScene(scene,ScenePath);AssetDatabase.SaveAssets();
            Debug.Log("PAINTERLY_PREVIEW_CREATED: committed baseline; original saved scene untouched.");
        }
        [MenuItem("WildType/Open painterly comparison")]
        public static void Open()
        {
            if(EditorApplication.isPlaying){Debug.LogWarning("Stop Play Mode before opening the comparison scene.");return;}
            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
        }
        public static void ConfigureComparison()
        {
            var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
            foreach(var root in scene.GetRootGameObjects())foreach(var filter in root.GetComponentsInChildren<MeshFilter>())
                GameObjectUtility.SetStaticEditorFlags(filter.gameObject,GameObjectUtility.GetStaticEditorFlags(filter.gameObject)&~StaticEditorFlags.BatchingStatic);
            EditorSceneManager.SaveScene(scene,ScenePath);
            var scenes=new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if(!scenes.Exists(s=>s.path==ScenePath))scenes.Add(new EditorBuildSettingsScene(ScenePath,true));EditorBuildSettings.scenes=scenes.ToArray();
        }
    }
}
