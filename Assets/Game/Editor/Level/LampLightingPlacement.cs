using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Bw.EditorTools.Level
{
    public static class LampLightingPlacement
    {
        private const int HazeOrder = 9;

        public static void Rebuild(
            LampScene scene,
            BakeTiles tiles,
            BakeMaps maps,
            LampBakeFolders folders)
        {
            var reliefOrder = ReliefOrder(scene);
            var layers = new SerializedObject(scene.Lamps[0]).FindProperty("m_ApplyToSortingLayers");
            Clear(scene.LightRoot);
            Clear(scene.HazeRoot);
            for (var tile = 0; tile < tiles.Count; tile++)
            {
                var corner = tiles.Corner(tile);
                var light = CreateChild(scene.LightRoot, "Light " + tile, corner).AddComponent<Light2D>();
                light.lightType = Light2D.LightType.Sprite;
                light.lightCookieSprite = LampBakeFolders.SpriteAt(folders.LightTile(tile));
                light.color = Color.white;
                light.intensity = maps.LightPeak;
                light.shadowsEnabled = false;
                light.volumetricEnabled = false;
                light.blendStyleIndex = 0;
                CopyLayers(layers, light);
                AddSprite(CreateChild(scene.LightRoot, "Relief " + tile, corner), folders.ReliefTile(tile), folders.ReliefMaterial, reliefOrder);
                AddSprite(CreateChild(scene.HazeRoot, "Haze " + tile, corner), folders.HazeTile(tile), folders.HazeMaterial, HazeOrder);
            }
        }

        private static int ReliefOrder(LampScene scene)
        {
            var chunks = scene.Chunks;
            var order = chunks.Max(chunk => chunk.sortingOrder) + 1;
            var crowded = scene.Renderers
                .Where(renderer => renderer.sortingOrder > chunks.Min(chunk => chunk.sortingOrder) && renderer.sortingOrder <= order)
                .Where(renderer => !(renderer is SpriteRenderer sprite && chunks.Contains(sprite)) && !renderer.transform.IsChildOf(scene.LightRoot.transform))
                .Select(renderer => renderer.name)
                .ToList();
            if (crowded.Count > 0)
                throw new InvalidOperationException(
                    $"'{string.Join("', '", crowded)}' of '{scene.Name}' sort between the normal-mapped chunks and their relief (order {order}): move them above it.");

            return order;
        }

        private static void Clear(GameObject root)
        {
            foreach (var child in root.transform.Cast<Transform>().ToList())
                UnityEngine.Object.DestroyImmediate(child.gameObject);
        }

        private static GameObject CreateChild(GameObject root, string name, Vector3 position)
        {
            var child = new GameObject(name);
            child.transform.SetParent(root.transform, false);
            child.transform.position = position;
            return child;
        }

        private static void AddSprite(
            GameObject target,
            string sprite,
            string material,
            int order)
        {
            var renderer = target.AddComponent<SpriteRenderer>();
            renderer.sprite = LampBakeFolders.SpriteAt(sprite);
            renderer.sharedMaterial = LampBakeFolders.MaterialAt(material);
            renderer.sortingOrder = order;
        }

        private static void CopyLayers(SerializedProperty layers, Light2D light)
        {
            var data = new SerializedObject(light);
            var targets = data.FindProperty("m_ApplyToSortingLayers");
            targets.arraySize = layers.arraySize;
            for (var i = 0; i < layers.arraySize; i++)
                targets.GetArrayElementAtIndex(i).intValue = layers.GetArrayElementAtIndex(i).intValue;

            data.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
