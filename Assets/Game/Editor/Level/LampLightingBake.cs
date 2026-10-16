using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bw.EditorTools.Level
{
    public sealed class LampLightingBake
    {
        private const string Title = "Bake Lamp Lighting";
        private const int WarmUpRenders = 3;

        private static readonly LampShot[] Shots = (LampShot[])Enum.GetValues(typeof(LampShot));

        private readonly Scene _scene;

        public LampLightingBake(Scene scene) =>
            _scene = scene;

        public bool TryRun()
        {
            var target = LampScene.Of(_scene);
            var preview = EditorSceneManager.OpenPreviewScene(_scene.path);
            try
            {
                var source = LampScene.Of(preview);
                var chunks = source.Chunks;
                if (chunks.Count == 0)
                    throw new InvalidOperationException($"'{_scene.name}' has no normal-mapped sprites: the lamp bake covers them and takes their relief.");

                if (source.Lamps.Count == 0)
                    throw new InvalidOperationException($"LampLight and LampHaze of '{_scene.name}' list no lamps: there is nothing to bake.");

                var bounds = chunks.Select(chunk => chunk.bounds).Aggregate((all, next) =>
                {
                    all.Encapsulate(next);
                    return all;
                });
                var tiles = BakeTiles.Covering(bounds);
                var ambient = source.Ambient.color * source.Ambient.intensity;
                var maps = new BakeMaps(tiles, ambient);
                using var probe = LampProbe.Create(preview);
                using var camera = LampBakeCamera.Create(preview, tiles);
                probe.Isolate(source);
                WarmUp(source, probe, camera, tiles);
                if (!TryShoot(source, probe, camera, tiles, maps))
                    return false;

                if (maps.LightPeak <= 0f)
                    throw new InvalidOperationException($"The lamps of '{_scene.name}' give no light: are their intensities zero?");

                EditorUtility.DisplayProgressBar(Title, "Writing the textures", 1f);
                var folders = new LampBakeFolders(target.Name);
                LampBakeTextures.Write(maps, tiles, folders);
                LampLightingPlacement.Rebuild(target, tiles, maps, folders);
                Debug.Log($"[{Title}] '{_scene.name}': {tiles.Count} tiles of {tiles.Size.x}×{tiles.Size.y} at {BakeTiles.PixelsPerUnit} px a unit, light peak {maps.LightPeak:0.###}, haze peak {maps.HazePeak:0.####}, ambient {ambient}");
                return true;
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(preview);
                EditorUtility.ClearProgressBar();
            }
        }

        private static void WarmUp(
            LampScene source,
            LampProbe probe,
            LampBakeCamera camera,
            BakeTiles tiles)
        {
            var center = tiles.HalfCenter(0, 0);
            probe.ShowFlat(tiles.HalfSizeInUnits, center);
            source.ShowLamps(true, true);
            for (var render = 0; render < WarmUpRenders; render++)
                camera.Render(source, center);
        }

        private static bool TryShoot(
            LampScene source,
            LampProbe probe,
            LampBakeCamera camera,
            BakeTiles tiles,
            BakeMaps maps)
        {
            var steps = tiles.Count * 2 * Shots.Length;
            var step = 0;
            for (var tile = 0; tile < tiles.Count; tile++)
                for (var half = 0; half < 2; half++)
                {
                    var center = tiles.HalfCenter(tile, half);
                    var shots = new Dictionary<LampShot, Color[]>();
                    foreach (var shot in Shots)
                    {
                        if (EditorUtility.DisplayCancelableProgressBar(Title, $"Tile {tile + 1}/{tiles.Count}, {shot}", step++ / (float)steps))
                            return false;

                        Prepare(shot, source, probe, tiles, center);
                        camera.Render(source, center);
                        shots[shot] = camera.FilteredPixels;
                    }

                    maps.Add(tile, half, shots);
                }

            return true;
        }

        private static void Prepare(
            LampShot shot,
            LampScene source,
            LampProbe probe,
            BakeTiles tiles,
            Vector3 center)
        {
            switch (shot)
            {
                case LampShot.FlatLit:
                case LampShot.FlatDark:
                case LampShot.FlatHazy:
                    probe.ShowFlat(tiles.HalfSizeInUnits, center);
                    break;
                case LampShot.ChunksLit:
                case LampShot.ChunksDark:
                    probe.ShowChunks();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(shot), shot, null);
            }

            source.ShowLamps(shot is not (LampShot.FlatDark or LampShot.ChunksDark), shot == LampShot.FlatHazy);
        }
    }
}
