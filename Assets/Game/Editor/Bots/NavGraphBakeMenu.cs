using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Bw.EditorTools.Shooting;
using Bw.Entities.Physics;
using Bw.Injection.Bots;
using Bw.Injection.Level;
using Bw.UseCases.Bots.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace Bw.EditorTools.Bots
{
    public static class NavGraphBakeMenu
    {
        private const string MenuPath = "Tools/Bots/Bake Navigation";
        private const string PhysicsConfigPath = "Assets/Resources/Physics/PhysicsConfig.asset";
        private const string GraphFolder = "Assets/Resources/Bots/Navigation";

        [MenuItem(MenuPath)]
        public static void Bake()
        {
            foreach (var scene in BotScenes())
                Bake(scene);

            AssetDatabase.SaveAssets();
        }

        [MenuItem(MenuPath, true)]
        public static bool IsBakeAvailable() =>
            !EditorApplication.isPlayingOrWillChangePlaymode && BotScenes().Any();

        private static void Bake(Scene scene)
        {
            var installer = InstallerIn(scene);
            var serialized = new SerializedObject(installer);
            var config = serialized.FindProperty(BotsInstaller.NavigationConfigField).objectReferenceValue as NavGraphConfig;
            if (config == null)
                throw new InvalidOperationException($"'{installer.name}' in '{scene.name}' has no navigation config: assign Resources/Bots/NavGraphConfig before baking.");

            var physics = AssetDatabase.LoadAssetAtPath<PhysicsConfig>(PhysicsConfigPath);
            if (physics == null)
                throw new InvalidOperationException($"No physics config at '{PhysicsConfigPath}': the bake builds its world with the game's physics settings.");

            var watch = Stopwatch.StartNew();
            var built = new NavGraphBuilder(config, physics, PreviewStep.FromNetwork()).Build(LevelBoxes.In(scene));
            var property = serialized.FindProperty(BotsInstaller.NavigationField);
            if (property.objectReferenceValue is NavGraphAsset existing)
            {
                EditorUtility.CopySerialized(built, existing);
                EditorUtility.SetDirty(existing);
                UnityEngine.Object.DestroyImmediate(built);
            }
            else
            {
                AssetDatabase.CreateAsset(built, $"{GraphFolder}/{scene.name}Navigation.asset");
                property.objectReferenceValue = built;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene))
                    throw new InvalidOperationException($"Saving '{scene.path}' after assigning its navigation graph failed.");
            }

            var graph = (NavGraphAsset)property.objectReferenceValue;
            Debug.Log($"[Bots] '{scene.name}' navigation baked in {watch.Elapsed.TotalSeconds:0.0} s: {graph.Nodes.Length} nodes, {graph.Edges.Length} edges.");
        }

        private static BotsInstaller InstallerIn(Scene scene) =>
            scene.GetRootGameObjects().Select(root => root.GetComponentInChildren<BotsInstaller>(true)).First(found => found != null);

        private static IEnumerable<Scene> BotScenes() =>
            Enumerable.Range(0, SceneManager.sceneCount)
                .Select(SceneManager.GetSceneAt)
                .Where(scene => scene.isLoaded && scene.GetRootGameObjects().Any(root => root.GetComponentInChildren<BotsInstaller>(true) != null))
                .ToList();
    }
}
