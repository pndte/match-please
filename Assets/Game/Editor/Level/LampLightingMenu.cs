using System;
using System.Collections.Generic;
using System.Linq;
using Bw.UseCases.Level.View.Lighting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Bw.EditorTools.Level
{
    public static class LampLightingMenu
    {
        private const string MenuPath = "Tools/Level/Bake Lamp Lighting";

        [MenuItem(MenuPath)]
        public static void Bake()
        {
            foreach (var scene in LampLitScenes())
            {
                if (scene.isDirty && !EditorSceneManager.SaveScene(scene))
                    throw new InvalidOperationException($"Saving '{scene.path}' failed: the lamp bake renders the saved scene.");

                if (!new LampLightingBake(scene).TryRun())
                    return;

                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene))
                    throw new InvalidOperationException($"Saving '{scene.path}' after the lamp bake failed.");
            }
        }

        [MenuItem(MenuPath, true)]
        public static bool IsBakeAvailable() =>
            !EditorApplication.isPlayingOrWillChangePlaymode && LampLitScenes().Any();

        private static IEnumerable<Scene> LampLitScenes() =>
            Enumerable.Range(0, SceneManager.sceneCount)
                .Select(SceneManager.GetSceneAt)
                .Where(scene => scene.isLoaded && scene.GetRootGameObjects().Any(root => root.GetComponentInChildren<LampLight>(true) != null))
                .ToList();
    }
}
