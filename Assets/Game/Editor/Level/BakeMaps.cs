using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bw.EditorTools.Level
{
    public sealed class BakeMaps
    {
        public const float ReliefScale = 2f;

        private readonly BakeTiles _tiles;
        private readonly Color _ambient;

        public IReadOnlyList<Color[]> Light { get; }
        public IReadOnlyList<Color[]> Relief { get; }
        public IReadOnlyList<Color[]> Haze { get; }
        public float LightPeak { get; private set; }
        public float HazePeak { get; private set; }

        public BakeMaps(BakeTiles tiles, Color ambient)
        {
            _tiles = tiles;
            _ambient = ambient;
            Light = NewTiles(tiles);
            Relief = NewTiles(tiles);
            Haze = NewTiles(tiles);
        }

        public void Add(int tile, int half, IReadOnlyDictionary<LampShot, Color[]> shots)
        {
            var width = _tiles.HalfWidth;
            for (var y = 0; y < _tiles.Size.y; y++)
                for (var x = 0; x < width; x++)
                {
                    var i = y * width + x;
                    var o = y * _tiles.Size.x + half * width + x;
                    var light = Positive(shots[LampShot.FlatLit][i] - shots[LampShot.FlatDark][i]);
                    var haze = Positive(shots[LampShot.FlatHazy][i] - shots[LampShot.FlatLit][i]);
                    var chunks = Positive(shots[LampShot.ChunksLit][i] - shots[LampShot.ChunksDark][i]);
                    var covered = Mathf.Max(shots[LampShot.ChunksLit][i].a, shots[LampShot.ChunksDark][i].a) >= 1f;
                    Light[tile][o] = light;
                    Haze[tile][o] = haze;
                    Relief[tile][o] = covered ? Ratio(_ambient + chunks, _ambient + light) : Color.white;
                    LightPeak = Mathf.Max(LightPeak, light.maxColorComponent);
                    HazePeak = Mathf.Max(HazePeak, haze.maxColorComponent);
                }
        }

        private static IReadOnlyList<Color[]> NewTiles(BakeTiles tiles) =>
            Enumerable.Range(0, tiles.Count).Select(_ => new Color[tiles.Size.x * tiles.Size.y]).ToList();

        private static Color Positive(Color color) =>
            new(Mathf.Max(0f, color.r), Mathf.Max(0f, color.g), Mathf.Max(0f, color.b), 1f);

        private static Color Ratio(Color lit, Color flat) =>
            new(Part(lit.r, flat.r), Part(lit.g, flat.g), Part(lit.b, flat.b), 1f);

        private static float Part(float lit, float flat) =>
            flat > 0f ? lit / flat : 1f;
    }
}
