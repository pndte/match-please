using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;
using Bw.Tests.Integration.Injection;

namespace Bw.Tests.Integration.Assets
{
    public sealed class InstallerReferencesTests
    {
        private const string ProjectContextPrefab = "Assets/Resources/ProjectContext.prefab";

        private static IEnumerable<TestCaseData> Scenes() =>
            ValidationContainers.Scenes()
                .Select(path => new TestCaseData(path).SetName($"{{m}}({Path.GetFileNameWithoutExtension(path)})"));

        private static IEnumerable<TestCaseData> ContextPrefabs() =>
            AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => AssetDatabase.LoadAssetAtPath<GameObject>(path).TryGetComponent<GameObjectContext>(out _))
                .OrderBy(path => path, StringComparer.Ordinal)
                .Select(path => new TestCaseData(path).SetName($"{{m}}({Path.GetFileNameWithoutExtension(path)})"));

        [Test]
        public void ProjectContextListsNoNullInstaller()
        {
            var context = AssetDatabase.LoadAssetAtPath<GameObject>(ProjectContextPrefab).GetComponent<ProjectContext>();

            var listed = Listed(context);

            Assert.That(listed, Is.Not.Empty);
            Assert.That(listed.Count(installer => installer == null), Is.Zero);
        }

        [Test]
        public void ProjectContextReferencesNoMissingObject()
        {
            var context = AssetDatabase.LoadAssetAtPath<GameObject>(ProjectContextPrefab).GetComponent<ProjectContext>();

            var missing = MissingReferencesUnder(context.GetComponents<Component>());

            Assert.That(missing, Is.Empty, string.Join("\n", missing));
        }

        [Category("Slow")]
        [TestCaseSource(nameof(Scenes))]
        public void SceneContextListsNoNullInstallerAndReferencesNoMissingObject(string path)
        {
            var scene = EditorSceneManager.OpenPreviewScene(path);
            try
            {
                var contexts = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<SceneContext>(true)).ToList();
                var installers = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<MonoInstaller>(true)).Cast<Component>();

                var missing = MissingReferencesUnder(contexts.Cast<Component>().Concat(installers));

                Assert.That(contexts, Has.Count.EqualTo(1));
                Assert.That(Listed(contexts[0]).Count(installer => installer == null), Is.Zero);
                Assert.That(missing, Is.Empty, string.Join("\n", missing));
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }

        [TestCaseSource(nameof(ContextPrefabs))]
        public void GameObjectContextListsNoNullInstallerAndReferencesNoMissingObject(string path)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var context = prefab.GetComponent<GameObjectContext>();

            var missing = MissingReferencesUnder(prefab.GetComponentsInChildren<MonoInstaller>(true).Cast<Component>().Append(context));

            Assert.That(Listed(context).Count(installer => installer == null), Is.Zero);
            Assert.That(missing, Is.Empty, string.Join("\n", missing));
        }

        private static List<Object> Listed(Context context) =>
            context.ScriptableObjectInstallers.Cast<Object>().Concat(context.Installers).Concat(context.InstallerPrefabs).ToList();

        private static List<string> MissingReferencesUnder(IEnumerable<Component> components)
        {
            var missing = new List<string>();
            var visited = new HashSet<Object>();
            foreach (var component in components)
                CollectMissing(component, $"{component.gameObject.name}/{component.GetType().Name}", visited, missing);

            return missing;
        }

        private static void CollectMissing(
            Object target,
            string label,
            HashSet<Object> visited,
            List<string> missing)
        {
            if (!visited.Add(target))
                return;

            var property = new SerializedObject(target).GetIterator();
            while (property.NextVisible(true))
            {
                if (property.propertyType != SerializedPropertyType.ObjectReference)
                    continue;

                if (property.objectReferenceValue == null && property.objectReferenceInstanceIDValue != 0)
                    missing.Add($"{label}.{property.propertyPath}");

                if (property.objectReferenceValue is ScriptableObject config && config.GetType().Assembly.GetName().Name.StartsWith("Bw."))
                    CollectMissing(config, $"{label}.{property.propertyPath} ({config.name})", visited, missing);
            }
        }
    }
}
