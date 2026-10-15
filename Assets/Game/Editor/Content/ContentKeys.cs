using System;
using System.Collections.Generic;
using System.Linq;
using Bw.Entities.Content;
using Unity.Multiplayer.PlayMode;
using UnityEditor;
using UnityEngine;

namespace Bw.EditorTools.Content
{
    public sealed class ContentKeys : AssetPostprocessor
    {
        public static ContentAsset[] KeyedAssets() =>
            TypeCache.GetTypesDerivedFrom<ContentAsset>()
                .Where(type => !type.IsAbstract)
                .SelectMany(type => AssetDatabase.FindAssets($"t:{type.Name}"))
                .Select(AssetDatabase.GUIDToAssetPath)
                .Distinct()
                .OrderBy(path => path, StringComparer.Ordinal)
                .Select(AssetDatabase.LoadAssetAtPath<ContentAsset>)
                .Where(asset => asset != null)
                .ToArray();

        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths,
            bool didDomainReload)
        {
            if (!CurrentPlayer.IsMainEditor)
                return;

            var imported = importedAssets.Where(IsContent).ToArray();
            if (imported.Length > 0 || didDomainReload)
                EditorApplication.delayCall += () => Assign(imported);
        }

        private static void Assign(IReadOnlyCollection<string> imported)
        {
            var counter = ContentKeyCounter.Saved();
            var assets = KeyedAssets();
            foreach (var asset in assets)
                counter.RaiseAbove(asset.Key);

            var taken = new HashSet<int>();
            var issued = false;
            foreach (var asset in assets.OrderBy(asset => imported.Contains(AssetDatabase.GetAssetPath(asset))))
            {
                if (asset.Key != 0 && taken.Add(asset.Key))
                    continue;

                var key = counter.Next;
                counter.Advance();
                Write(asset, key);
                taken.Add(key);
                issued = true;
            }

            if (issued)
                counter.Save();
        }

        private static void Write(ContentAsset asset, int key)
        {
            var serialized = new SerializedObject(asset);
            serialized.FindProperty(ContentAsset.KeyField).intValue = key;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssetIfDirty(asset);
            Debug.Log($"Content key {key.ToString()} goes to {AssetDatabase.GetAssetPath(asset)}");
        }

        private static bool IsContent(string path) =>
            typeof(ContentAsset).IsAssignableFrom(AssetDatabase.GetMainAssetTypeAtPath(path));
    }
}
