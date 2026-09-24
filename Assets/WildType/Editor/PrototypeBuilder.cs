using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;
namespace WildType.Editor
{
    public static class PrototypeBuilder
    {
        const string Root = "Assets/WildType";
        static Material body, bark, leaf, stone, grass, fruit, gold, particle;
        [MenuItem("WildType/Build prototype scene")]
        public static void Build()
        {
            foreach (var path in new[] { "Art/Prototype", "Materials", "Prefabs/Creatures", "Prefabs/Environment", "Prefabs/Food", "Scenes", "ScriptableObjects", "Tests" })
                Directory.CreateDirectory(Root + "/" + path);
            Directory.CreateDirectory("Assets/ThirdParty");
            AssetDatabase.Refresh();
            EditorSettings.serializationMode = SerializationMode.ForceText;
            UnityEditor.VersionControlSettings.mode = "Visible Meta Files";
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.companyName = "WildType"; PlayerSettings.productName = "WILDTYPE";
            PlayerSettings.defaultScreenWidth = 1920; PlayerSettings.defaultScreenHeight = 1080;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.runInBackground = true;
            var settings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            var input = settings.FindProperty("activeInputHandler");
            if (input == null) throw new Exception("Cannot locate active input handling setting");
            input.intValue = 2; settings.ApplyModifiedPropertiesWithoutUndo();
            Time.fixedDeltaTime = .02f;
            EditorUtility.SetDirty(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TimeManager.asset")[0]);
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
            var tags = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layers = tags.FindProperty("layers");
            layers.GetArrayElementAtIndex(8).stringValue = "Ground"; layers.GetArrayElementAtIndex(9).stringValue = "Obstacles";
            layers.GetArrayElementAtIndex(10).stringValue = "Creatures"; layers.GetArrayElementAtIndex(11).stringValue = "Food";
            tags.ApplyModifiedPropertiesWithoutUndo();
            // Root controller owns collisions; decorative meshes never affect movement.
            Physics.IgnoreLayerCollision(10, 10, true);
            ImportFontResources();
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/ThirdParty/UnityTextMeshPro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            if (!font) throw new Exception("Bundled TMP font missing");
            body = Material("Prototype", new Color(.25f, .65f, .56f));
            bark = Material("Warm bark", new Color(.25f, .16f, .1f));
            leaf = Material("Fern canopy", new Color(.18f, .35f, .2f));
            stone = Material("Weathered rock", new Color(.42f, .43f, .36f));
            grass = Material("Meadow blades", new Color(.39f, .55f, .22f)); grass.SetFloat("_Cull", 0);
            fruit = Material("Brightfruit coral", new Color(.97f, .33f, .13f));
            gold = Material("Interaction gold", new Color(1, .8f, .22f));
            particle = Material("Feedback", Color.white, "Universal Render Pipeline/Particles/Unlit");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(.53f, .66f, .72f);
            RenderSettings.ambientEquatorColor = new Color(.37f, .44f, .32f);
            RenderSettings.ambientGroundColor = new Color(.18f, .21f, .15f);
            RenderSettings.fog = true; RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(.65f, .76f, .75f); RenderSettings.fogDensity = .0052f;
            var sky = Material("Local sky", Color.white, "Skybox/Procedural");
            sky.SetFloat("_SunSize", .025f); sky.SetColor("_SkyTint", new Color(.5f, .62f, .7f)); sky.SetFloat("_AtmosphereThickness", .9f); RenderSettings.skybox = sky;
            var sun = new GameObject("Afternoon sun").AddComponent<Light>(); sun.type = LightType.Directional;
            sun.color = new Color(1, .93f, .77f); sun.intensity = 1.7f; sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(48, -32, 0); RenderSettings.sun = sun;
            var sessionObject = new GameObject("Creature Stage", typeof(Ecosystem), typeof(PlayerInputBridge), typeof(StageHud), typeof(StageSession));
            var session = sessionObject.GetComponent<StageSession>();
            session.prototypeMaterial = body; session.particleMaterial = particle; session.hudFont = font;
            session.presets = BuildPresets(); session.creaturePrefab = BuildCreature(); session.foodPrefab = BuildFood();
            var camera = new GameObject("Third-person camera", typeof(Camera), typeof(AudioListener), typeof(OrbitCamera));
            camera.tag = "MainCamera"; var cam = camera.GetComponent<Camera>(); cam.fieldOfView = 61; cam.nearClipPlane = .1f; cam.farClipPlane = 500;
            cam.transform.position = new Vector3(0, 7, -11); cam.transform.rotation = Quaternion.Euler(22, 0, 0);
            cam.allowHDR = true; cam.GetUniversalAdditionalCameraData().antialiasing = AntialiasingMode.FastApproximateAntialiasing;
            session.orbit = camera.GetComponent<OrbitCamera>();
            BuildTerrain(); BuildScenery();
            var pipeline = GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;
            if (!pipeline) throw new Exception("URP template pipeline missing");
            pipeline.msaaSampleCount = 4; pipeline.shadowDistance = 75; pipeline.renderScale = 1;
            EditorUtility.SetDirty(pipeline);
            EditorBuildSettings.RemoveConfigObject("com.unity.input.settings.actions");
            foreach (string path in new[] { "Assets/TutorialInfo", "Assets/Readme.asset", "Assets/InputSystem_Actions.inputactions", "Assets/Scenes/SampleScene.unity" })
                if (File.Exists(path) || Directory.Exists(path)) AssetDatabase.DeleteAsset(path);
            string scenePath = Root + "/Scenes/CreatureStage_Prototype.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(scenePath, true) };
            Selection.activeGameObject = sessionObject;
            AssetDatabase.SaveAssets();
            Debug.Log("WILDTYPE_SETUP_COMPLETE | URP | Windows64 | Linear | Input Both | fixed .02");
        }
        static void ImportFontResources()
        {
            if (AssetDatabase.FindAssets("LiberationSans SDF t:TMP_FontAsset").Length > 0) return;
            string[] packages = Directory.GetFiles("Library/PackageCache", "TMP Essential Resources.unitypackage", SearchOption.AllDirectories);
            if (packages.Length == 0) throw new Exception("TMP bundled essential resource package unavailable");
            AssetDatabase.ImportPackage(packages[0], false);
            AssetDatabase.Refresh();
            if (AssetDatabase.IsValidFolder("Assets/TextMesh Pro"))
            {
                string error = AssetDatabase.MoveAsset("Assets/TextMesh Pro", "Assets/ThirdParty/UnityTextMeshPro");
                if (!string.IsNullOrEmpty(error)) throw new Exception(error);
            }
        }
        static Material Material(string name, Color color, string shader = "Universal Render Pipeline/Lit")
        {
            string path = Root + "/Materials/" + name + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!mat) { mat = new Material(Shader.Find(shader)); AssetDatabase.CreateAsset(mat, path); }
            mat.color = color; if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", .22f);
            EditorUtility.SetDirty(mat); return mat;
        }
        static GenomePreset[] BuildPresets()
        {
            var values = new[] {
                new Genome { bodySize = 1, legLength = 1, movementSpeed = 6.4f, metabolism = 1, vision = 27, fertility = 1.05f, reproductionThreshold = .72f, offspringTendency = 1.05f, lifespan = 600, camouflage = new Color(.17f, .65f, .59f) },
                new Genome { bodySize = 1.5f, legLength = .8f, movementSpeed = 4.8f, turnAgility = 5.5f, metabolism = .7f, energyEfficiency = 1.35f, maximumEnergy = 130, stamina = 120, vision = 20, fertility = .72f, reproductionThreshold = .8f, offspringTendency = .72f, lifespan = 780, camouflage = new Color(.59f, .39f, .2f) },
                new Genome { bodySize = .77f, legLength = 1.5f, movementSpeed = 8.6f, turnAgility = 9, metabolism = 1.35f, energyEfficiency = .8f, maximumEnergy = 80, stamina = 85, vision = 38, fertility = 1.28f, reproductionThreshold = .62f, offspringTendency = 1.3f, lifespan = 420, camouflage = new Color(.38f, .4f, .77f) }
            };
            string[] names = { "Meadow Grazer", "Amber Bulwark", "Violet Strider" };
            var presets = new GenomePreset[3];
            for (int i = 0; i < 3; i++)
            {
                string path = Root + "/ScriptableObjects/" + names[i] + ".asset";
                var p = AssetDatabase.LoadAssetAtPath<GenomePreset>(path);
                if (!p) { p = ScriptableObject.CreateInstance<GenomePreset>(); AssetDatabase.CreateAsset(p, path); }
                p.genome = values[i]; p.description = names[i]; EditorUtility.SetDirty(p); presets[i] = p;
            }
            return presets;
        }
        static CreatureAgent BuildCreature()
        {
            var root = new GameObject("CreatureRoot", typeof(CharacterController), typeof(CreatureMotor), typeof(CreatureVitals), typeof(CreatureInteraction), typeof(CreatureAgent)); root.layer = 10;
            var visual = new GameObject("VisualRoot", typeof(CreatureVisual)); visual.transform.SetParent(root.transform, false);
            var point = new GameObject("InteractionPoint").transform; point.SetParent(root.transform, false); point.localPosition = new Vector3(0, 1, 1.1f);
            root.GetComponent<CreatureInteraction>().point = point;
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, Root + "/Prefabs/Creatures/CreatureRoot.prefab").GetComponent<CreatureAgent>();
            UnityEngine.Object.DestroyImmediate(root); return prefab;
        }
        static FoodPlant BuildFood()
        {
            var root = new GameObject("Brightfruit", typeof(FoodPlant)); root.layer = 11;
            var plant = root.GetComponent<FoodPlant>();
            Primitive("Stem", PrimitiveType.Cylinder, root.transform, new Vector3(0, .65f, 0), new Vector3(.12f, .65f, .12f), leaf);
            for (int i = 0; i < 5; i++)
            {
                float a = i * Mathf.PI * 2 / 5;
                var p = Primitive("Leaf", PrimitiveType.Sphere, root.transform, new Vector3(Mathf.Cos(a) * .3f, .66f, Mathf.Sin(a) * .3f), new Vector3(.58f, .11f, .27f), grass);
                p.transform.localRotation = Quaternion.Euler(0, -a * Mathf.Rad2Deg, 14);
            }
            plant.fruit = new GameObject("Berries").transform; plant.fruit.SetParent(root.transform, false);
            for (int i = 0; i < 3; i++)
            {
                float a = i * Mathf.PI * 2 / 3;
                Primitive("Fruit", PrimitiveType.Sphere, plant.fruit, new Vector3(Mathf.Cos(a) * .23f, 1.35f + i % 2 * .16f, Mathf.Sin(a) * .23f), Vector3.one * .42f, fruit);
            }
            var ring = Primitive("Selection glow", PrimitiveType.Cylinder, root.transform, new Vector3(0, .04f, 0), new Vector3(1.3f, .018f, 1.3f), gold);
            plant.marker = ring.GetComponent<Renderer>(); plant.marker.enabled = false;
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, Root + "/Prefabs/Food/Brightfruit.prefab").GetComponent<FoodPlant>(); UnityEngine.Object.DestroyImmediate(root); return prefab;
        }
        static GameObject Primitive(string name, PrimitiveType type, Transform parent, Vector3 position, Vector3 scale, Material material, bool collision = false)
        {
            var obj = GameObject.CreatePrimitive(type); obj.name = name; obj.transform.SetParent(parent, false); obj.transform.localPosition = position; obj.transform.localScale = scale;
            obj.GetComponent<Renderer>().sharedMaterial = material;
            if (!collision) UnityEngine.Object.DestroyImmediate(obj.GetComponent<Collider>());
            else obj.layer = 9;
            return obj;
        }
        static void BuildTerrain()
        {
            const int n = 128; const float extent = 140;
            var vertices = new Vector3[(n + 1) * (n + 1)];
            var triangles = new List<int>[9]; for (int i = 0; i < 9; i++) triangles[i] = new List<int>();
            for (int z = 0; z <= n; z++) for (int x = 0; x <= n; x++)
            {
                float px = -extent + x * extent * 2 / n, pz = -extent + z * extent * 2 / n;
                vertices[z * (n + 1) + x] = new Vector3(px, Ecosystem.Height(px, pz), pz);
            }
            for (int z = 0; z < n; z++) for (int x = 0; x < n; x++)
            {
                int a = z * (n + 1) + x, b = a + 1, c = a + n + 1, d = c + 1;
                Vector3 p = vertices[a]; int zone = p.x < -35 ? 0 : p.x > 35 ? 2 : 1;
                float patch = Mathf.Sin(p.x * .11f) * Mathf.Cos(p.z * .13f);
                int shade = patch > .35f ? 2 : patch < -.35f ? 0 : 1;
                triangles[zone * 3 + shade].AddRange(new[] { a, c, b, b, c, d });
            }
            var mesh = new Mesh { name = "Rolling ecosystem terrain", vertices = vertices, subMeshCount = 9 };
            for (int i = 0; i < 9; i++) mesh.SetTriangles(triangles[i], i);
            mesh.RecalculateNormals(); mesh.RecalculateBounds(); mesh = SaveMesh(mesh, "Terrain");
            var root = new GameObject("250m ecosystem terrain", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider)); root.layer = 8;
            root.GetComponent<MeshFilter>().sharedMesh = mesh; root.GetComponent<MeshCollider>().sharedMesh = mesh;
            var materials = new Material[9]; Color[] palette = { new Color(.22f, .34f, .18f), new Color(.4f, .52f, .24f), new Color(.63f, .5f, .29f) };
            for (int i = 0; i < 9; i++) materials[i] = Material("Ground " + i, palette[i / 3] * (.91f + i % 3 * .09f));
            root.GetComponent<MeshRenderer>().sharedMaterials = materials; root.isStatic = true;
        }
        static void BuildScenery()
        {
            var root = new GameObject("Procedural scenery"); var random = new System.Random(44129);
            for (int i = 0; i < 130; i++)
            {
                var p = Ecosystem.RandomGround(random, 13, 112);
                if (p.x > 30 && i % 5 != 0) p.x = -Mathf.Abs(p.x);
                p.y = Ecosystem.Height(p.x, p.z);
                float s = 1.5f + (float)random.NextDouble() * 1.6f;
                var tree = new GameObject("Tree").transform; tree.SetParent(root.transform); tree.position = p;
                Primitive("Trunk", PrimitiveType.Cylinder, tree, Vector3.up * s, new Vector3(.34f * s, s, .34f * s), bark, true);
                for (int j = 0; j < 3; j++)
                {
                    float angle = j * Mathf.PI * 2 / 3;
                    Primitive("Crown", PrimitiveType.Sphere, tree, new Vector3(Mathf.Sin(angle) * s * .38f, s * (2.1f + j % 2 * .25f), Mathf.Cos(angle) * s * .38f), new Vector3(1.5f, 1.7f, 1.5f) * s, leaf);
                }
                if (i == 0) PrefabUtility.SaveAsPrefabAsset(tree.gameObject, Root + "/Prefabs/Environment/PrototypeTree.prefab");
            }
            for (int i = 0; i < 180; i++)
            {
                var p = Ecosystem.RandomGround(random, 12, 117); float s = .8f + (float)random.NextDouble() * 2.4f;
                var rock = Primitive("Rock", PrimitiveType.Sphere, root.transform, p + Vector3.up * s * .18f, new Vector3(s * 1.3f, s * .7f, s), stone);
                rock.layer = 9; rock.AddComponent<MeshCollider>().sharedMesh = rock.GetComponent<MeshFilter>().sharedMesh;
                if (i == 0) PrefabUtility.SaveAsPrefabAsset(rock, Root + "/Prefabs/Environment/PrototypeRock.prefab");
            }
            for (int i = 0; i < 48; i++)
            {
                float a = i * Mathf.PI * 2 / 48, x = Mathf.Sin(a) * 124, z = Mathf.Cos(a) * 124;
                Primitive("Boundary crag", PrimitiveType.Sphere, root.transform, new Vector3(x, Ecosystem.Height(x,z), z), new Vector3(10, 8 + i % 4, 9), stone);
            }
            // Combine decorative geometry by shared material; only collision objects remain separate.
            var groups = new Dictionary<Material, List<CombineInstance>>();
            var filters = root.GetComponentsInChildren<MeshFilter>();
            foreach (var f in filters)
            {
                var renderer = f.GetComponent<MeshRenderer>(); var material = renderer.sharedMaterial;
                if (!groups.ContainsKey(material)) groups[material] = new List<CombineInstance>();
                groups[material].Add(new CombineInstance { mesh = f.sharedMesh, transform = f.transform.localToWorldMatrix });
            }
            foreach (var group in groups)
            {
                var mesh = new Mesh { name = group.Key.name + " combined", indexFormat = IndexFormat.UInt32 };
                mesh.CombineMeshes(group.Value.ToArray()); mesh = SaveMesh(mesh, "Scenery " + group.Key.name);
                var obj = new GameObject(mesh.name, typeof(MeshFilter), typeof(MeshRenderer)); obj.transform.SetParent(root.transform);
                obj.GetComponent<MeshFilter>().sharedMesh = mesh; obj.GetComponent<MeshRenderer>().sharedMaterial = group.Key; obj.isStatic = true;
            }
            foreach (var f in filters)
            {
                UnityEngine.Object.DestroyImmediate(f.GetComponent<MeshRenderer>());
                if (f.GetComponent<Collider>()) UnityEngine.Object.DestroyImmediate(f);
                else UnityEngine.Object.DestroyImmediate(f.gameObject);
            }
            var v = new List<Vector3>(); var t = new List<int>();
            for (int i = 0; i < 3600; i++)
            {
                var p = Ecosystem.RandomGround(random, 5, 112);
                if (p.x > 35 && i % 3 != 0) continue;
                for (int b = 0; b < 3; b++)
                {
                    float a = b * Mathf.PI / 3, h = .3f + (float)random.NextDouble() * .6f;
                    Vector3 side = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * .16f;
                    int first = v.Count; v.Add(p - side); v.Add(p + side); v.Add(p + Vector3.up * h + Vector3.right * .1f); t.AddRange(new[] {first, first + 1, first + 2});
                }
            }
            var grassMesh = new Mesh { name = "Batched meadow grass" }; grassMesh.SetVertices(v); grassMesh.SetTriangles(t, 0); grassMesh.RecalculateNormals(); grassMesh = SaveMesh(grassMesh, "Grass");
            var tuft = new GameObject("Grass batch", typeof(MeshFilter), typeof(MeshRenderer)); tuft.transform.SetParent(root.transform);
            tuft.GetComponent<MeshFilter>().sharedMesh = grassMesh; tuft.GetComponent<MeshRenderer>().sharedMaterial = grass;
            tuft.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off; tuft.isStatic = true;
        }
        static Mesh SaveMesh(Mesh mesh, string name)
        {
            string path = Root + "/Art/Prototype/" + name + ".asset";
            var old = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (old) { EditorUtility.CopySerialized(mesh, old); UnityEngine.Object.DestroyImmediate(mesh); EditorUtility.SetDirty(old); return old; }
            AssetDatabase.CreateAsset(mesh, path); return mesh;
        }
    }
}
