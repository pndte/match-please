using System;
using UnityEditor;
using UnityEngine;

namespace Bw.EditorTools.Level
{
    public sealed class LampBakeFolders
    {
        private const string LevelArt = "Assets/Game/Art/Level";

        private readonly string _sceneName;

        public LampBakeFolders(string sceneName) =>
            _sceneName = sceneName;

        public string Light => $"{LevelArt}/{_sceneName}/Light";

        public string Haze => $"{LevelArt}/{_sceneName}/Haze";

        public string LightTile(int tile) =>
            $"{Light}/Light_{tile}.png";

        public string ReliefTile(int tile) =>
            $"{Light}/Relief_{tile}.png";

        public string HazeTile(int tile) =>
            $"{Haze}/Haze_{tile}.png";

        public string ReliefMaterial => $"{Light}/Relief.mat";

        public string HazeMaterial => $"{Haze}/Haze.mat";

        public static Sprite SpriteAt(string path) =>
            AssetDatabase.LoadAssetAtPath<Sprite>(path) ?? throw new InvalidOperationException($"No sprite at '{path}': the lamp bake writes it before it places it.");

        public static Material MaterialAt(string path) =>
            AssetDatabase.LoadAssetAtPath<Material>(path) ?? throw new InvalidOperationException($"No material at '{path}': the lamp bake writes it before it places it.");
    }
}
