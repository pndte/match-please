using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bw.Injection.Bots;
using Bw.Injection.Level;
using Bw.UseCases.Bots.Navigation;
using NUnit.Framework;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Bw.Tests.Integration.Injection;

namespace Bw.Tests.Integration.Assets
{
    [Category("Slow")]
    public sealed class NavigationGraphTests
    {
        private const string NetworkManagerPrefab = "Assets/Game/Prefabs/Network/Network.prefab";

        private static IEnumerable<TestCaseData> LevelsWithBots()
        {
            var script = AssetDatabase.FindAssets("t:MonoScript BotsInstaller")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Single(path => AssetDatabase.LoadAssetAtPath<MonoScript>(path).GetClass() == typeof(BotsInstaller));
            var reference = "guid: " + AssetDatabase.AssetPathToGUID(script);
            return ValidationContainers.Levels()
                .Where(path => File.ReadAllText(Path.GetFullPath(path)).Contains(reference))
                .Select(path => new TestCaseData(path).SetName($"{{m}}({Path.GetFileNameWithoutExtension(path)})"));
        }

        [Test]
        public void SomeLevelHasBots()
        {
            Assert.That(LevelsWithBots().Count(), Is.GreaterThan(0));
        }

        [TestCaseSource(nameof(LevelsWithBots))]
        public void BakedGraphMatchesItsLevelAndMovement(string level)
        {
            var step = 1f / AssetDatabase.LoadAssetAtPath<GameObject>(NetworkManagerPrefab).GetComponent<NetworkManager>().NetworkConfig.TickRate;
            var scene = EditorSceneManager.OpenPreviewScene(level);
            try
            {
                var installer = new SerializedObject(scene.GetRootGameObjects().Select(root => root.GetComponentInChildren<BotsInstaller>(true)).First(found => found != null));
                var graph = (NavGraphAsset)installer.FindProperty(BotsInstaller.NavigationField).objectReferenceValue;
                var config = (NavGraphConfig)installer.FindProperty(BotsInstaller.NavigationConfigField).objectReferenceValue;

                var fingerprint = NavFingerprint.Of(LevelBoxes.In(scene), config, step);

                Assert.That(graph.Fingerprint, Is.EqualTo(fingerprint));
                Assert.That(graph.Nodes, Is.Not.Empty);
                Assert.That(graph.Edges, Is.Not.Empty);
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }

        [TestCaseSource(nameof(BakedGraphs))]
        public void EveryEdgeJoinsTwoNodesOfItsGraph(string path)
        {
            var asset = AssetDatabase.LoadAssetAtPath<NavGraphAsset>(path);

            var broken = asset.Edges.Where(edge => edge.From < 0 || edge.From >= asset.Nodes.Length || edge.To < 0 || edge.To >= asset.Nodes.Length || edge.Ticks <= 0).ToList();

            Assert.That(broken, Is.Empty);
            Assert.DoesNotThrow(() => NavGraph.Of(asset));
        }

        private static IEnumerable<TestCaseData> BakedGraphs() =>
            AssetDatabase.FindAssets("t:NavGraphAsset")
                .Select(AssetDatabase.GUIDToAssetPath)
                .OrderBy(path => path, StringComparer.Ordinal)
                .Select(path => new TestCaseData(path).SetName($"{{m}}({Path.GetFileNameWithoutExtension(path)})"));
    }
}
