using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace Bw.Tests.Integration.Assets
{
    public sealed class NetworkPrefabTests
    {
        private const string PrefabList = "Assets/Game/DefaultNetworkPrefabs.asset";
        private const string NetworkManagerPrefab = "Assets/Game/Prefabs/Network/Network.prefab";
        private const string NetcodeSettings = "ProjectSettings/NetcodeForGameObjects.asset";

        private static readonly Regex StoredHash = new(@"\n  GlobalObjectIdHash: (\d+)");
        private static readonly Regex OverriddenHash = new(@"propertyPath: GlobalObjectIdHash\s+value: (\d+)");
        private static readonly Regex PrefabsPath = new(@"^\s*NetworkPrefabsPath: (.+?)\s*$", RegexOptions.Multiline);

        private static IEnumerable<string> NetworkPrefabPaths() =>
            AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => AssetDatabase.LoadAssetAtPath<GameObject>(path).TryGetComponent<NetworkObject>(out _))
                .OrderBy(path => path, StringComparer.Ordinal);

        private static IEnumerable<TestCaseData> NetworkPrefabs() =>
            NetworkPrefabPaths().Select(path => new TestCaseData(path).SetName($"{{m}}({Path.GetFileNameWithoutExtension(path)})"));

        [Test]
        public void ProjectHasNetworkPrefabs()
        {
            Assert.That(NetworkPrefabPaths().Count(), Is.GreaterThan(0));
        }

        [TestCaseSource(nameof(NetworkPrefabs))]
        public void PrefabHasNoMissingScript(string path)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            var missing = prefab.GetComponentsInChildren<Transform>(true)
                .Where(child => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject) > 0)
                .Select(child => child.name)
                .ToList();

            Assert.That(missing, Is.Empty);
        }

        [TestCaseSource(nameof(NetworkPrefabs))]
        public void FileStoresTheHashOfThePrefabsGlobalObjectId(string path)
        {
            var network = AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponent<NetworkObject>();
            var expected = Hash32(GlobalObjectId.GetGlobalObjectIdSlow(network).ToString());

            var stored = StoredHashIn(File.ReadAllText(Path.GetFullPath(path)));

            Assert.That(stored, Is.EqualTo(expected));
        }

        [TestCaseSource(nameof(NetworkPrefabs))]
        public void PrefabIsInTheDefaultNetworkPrefabsList(string path)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            var listed = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>(PrefabList).PrefabList.Select(entry => entry.Prefab);

            Assert.That(listed, Has.Member(prefab));
        }

        [Test]
        public void DefaultNetworkPrefabsListNamesOnlyExistingNetworkPrefabs()
        {
            var list = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>(PrefabList);

            var broken = list.PrefabList.Where(entry => entry.Prefab == null || !entry.Prefab.TryGetComponent<NetworkObject>(out _)).ToList();

            Assert.That(broken, Is.Empty);
        }

        [Test]
        public void NetworkManagerUsesTheListNetcodeFillsByItself()
        {
            var list = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>(PrefabList);
            var manager = AssetDatabase.LoadAssetAtPath<GameObject>(NetworkManagerPrefab).GetComponent<NetworkManager>();

            var filled = PrefabsPath.Match(File.ReadAllText(Path.GetFullPath(NetcodeSettings))).Groups[1].Value;

            Assert.That(filled, Is.EqualTo(PrefabList));
            Assert.That(manager.NetworkConfig.Prefabs.NetworkPrefabsLists, Has.Member(list));
        }

        [TestCase("", 0x02CC5D05u)]
        [TestCase("a", 0x550D7456u)]
        [TestCase("abc", 0x32D153FFu)]
        [TestCase("Nobody inspects the spammish repetition", 0xE2293B2Fu)]
        public void HashIsXxHash32WithSeedZero(string text, uint expected)
        {
            var hash = Hash32(text);

            Assert.That(hash, Is.EqualTo(expected));
        }

        private static uint StoredHashIn(string file)
        {
            var match = StoredHash.Match(file);
            if (!match.Success)
                match = OverriddenHash.Match(file);

            return match.Success ? uint.Parse(match.Groups[1].Value) : 0u;
        }

        private static uint Hash32(string text)
        {
            const uint prime1 = 2654435761u;
            const uint prime2 = 2246822519u;
            const uint prime3 = 3266489917u;
            const uint prime4 = 668265263u;
            const uint prime5 = 374761393u;

            var input = Encoding.UTF8.GetBytes(text);
            var length = input.Length;
            var position = 0;
            var hash = prime5;
            unchecked
            {
                if (length >= 16)
                {
                    var lanes = new[] { prime1 + prime2, prime2, 0u, 0u - prime1 };
                    for (; position + 16 <= length; position += 16)
                        for (var lane = 0; lane < 4; lane++)
                            lanes[lane] = RotateLeft(lanes[lane] + BitConverter.ToUInt32(input, position + lane * 4) * prime2, 13) * prime1;

                    hash = RotateLeft(lanes[0], 1) + RotateLeft(lanes[1], 7) + RotateLeft(lanes[2], 12) + RotateLeft(lanes[3], 18);
                }

                hash += (uint)length;
                for (; position + 4 <= length; position += 4)
                    hash = RotateLeft(hash + BitConverter.ToUInt32(input, position) * prime3, 17) * prime4;

                for (; position < length; position++)
                    hash = RotateLeft(hash + input[position] * prime5, 11) * prime1;

                hash ^= hash >> 15;
                hash *= prime2;
                hash ^= hash >> 13;
                hash *= prime3;
                hash ^= hash >> 16;
            }

            return hash;
        }

        private static uint RotateLeft(uint value, int bits) =>
            (value << bits) | (value >> (32 - bits));
    }
}
