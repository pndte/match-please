using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Bw.EditorTools.Level
{
    public static class LampBakeTextures
    {
        private const string ReliefShader = "Bw/Level-Relief";
        private const string HazeShader = "Bw/Level-Haze";

        public static void Write(BakeMaps maps, BakeTiles tiles, LampBakeFolders folders)
        {
            CreateFolder(folders.Light);
            CreateFolder(folders.Haze);
            var hazePeak = Mathf.Max(maps.HazePeak, float.Epsilon);
            for (var tile = 0; tile < tiles.Count; tile++)
            {
                Save(folders.LightTile(tile), tiles, maps.Light[tile].Select(c => Gamma(c / maps.LightPeak)).ToArray(), true);
                Save(folders.ReliefTile(tile), tiles, maps.Relief[tile].Select(c => c / BakeMaps.ReliefScale).ToArray(), false);
                Save(folders.HazeTile(tile), tiles, maps.Haze[tile].Select(c => Gamma(c / hazePeak)).ToArray(), true);
            }

            for (var tile = tiles.Count; AssetDatabase.LoadAssetAtPath<Texture2D>(folders.LightTile(tile)) != null; tile++)
            {
                AssetDatabase.DeleteAsset(folders.LightTile(tile));
                AssetDatabase.DeleteAsset(folders.ReliefTile(tile));
                AssetDatabase.DeleteAsset(folders.HazeTile(tile));
            }

            MaterialWithShader(folders.ReliefMaterial, ReliefShader).SetFloat("_Scale", BakeMaps.ReliefScale);
            MaterialWithShader(folders.HazeMaterial, HazeShader).SetFloat("_Gain", maps.HazePeak);
            AssetDatabase.SaveAssets();
        }

        private static void Save(
            string path,
            BakeTiles tiles,
            Color[] pixels,
            bool srgb)
        {
            var image = new Texture2D(tiles.Size.x, tiles.Size.y, TextureFormat.RGB24, false, !srgb);
            image.SetPixels32(pixels.Select(pixel => (Color32)pixel).ToArray());
            image.Apply();
            File.WriteAllBytes(path, image.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(image);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = BakeTiles.PixelsPerUnit;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.BottomLeft;
            settings.spritePivot = Vector2.zero;
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteGenerateFallbackPhysicsShape = false;
            importer.SetTextureSettings(settings);
            importer.sRGBTexture = srgb;
            importer.alphaSource = TextureImporterAlphaSource.None;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.maxTextureSize = Mathf.NextPowerOfTwo(Mathf.Max(tiles.Size.x, tiles.Size.y));
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
        }

        private static Material MaterialWithShader(string path, string shaderName)
        {
            var shader = Shader.Find(shaderName) ?? throw new InvalidOperationException($"No shader '{shaderName}' for '{path}'.");
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.shader = shader;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void CreateFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            var slash = path.LastIndexOf('/');
            CreateFolder(path.Substring(0, slash));
            AssetDatabase.CreateFolder(path.Substring(0, slash), path.Substring(slash + 1));
        }

        private static Color Gamma(Color linear) =>
            new(Mathf.LinearToGammaSpace(linear.r), Mathf.LinearToGammaSpace(linear.g), Mathf.LinearToGammaSpace(linear.b), 1f);
    }
}
