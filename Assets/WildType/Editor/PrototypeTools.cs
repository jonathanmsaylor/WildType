using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
namespace WildType.Editor
{
    public static class PrototypeTools
    {
        [MenuItem("WildType/Open prototype scene")]
        public static void Open()
        {
            if (EditorApplication.isPlaying) return;
            EditorBuildSettings.RemoveConfigObject("com.unity.input.settings.actions");
            EditorSceneManager.OpenScene("Assets/WildType/Scenes/CreatureStage_Prototype.unity");
            var stage = Object.FindAnyObjectByType<StageSession>();
            stage.hudFont = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>("Assets/ThirdParty/UnityTextMeshPro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            EditorUtility.SetDirty(stage);
            EditorSceneManager.MarkSceneDirty(stage.gameObject.scene);
            Selection.activeGameObject = stage.gameObject;
            EditorSceneManager.SaveOpenScenes(); AssetDatabase.SaveAssets();
            Debug.Log($"WILDTYPE_CONFIGURATION | Unity {Application.unityVersion} | {EditorUserBuildSettings.activeBuildTarget} | {PlayerSettings.colorSpace} | {EditorSettings.serializationMode} | {UnityEditor.VersionControlSettings.mode} | fixed {Time.fixedDeltaTime} | pipeline {GraphicsSettings.defaultRenderPipeline.name} | physical gamepads {Gamepad.all.Count}");
            SceneView.RepaintAll();
        }
    }
}
