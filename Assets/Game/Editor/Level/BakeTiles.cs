using UnityEngine;

namespace Bw.EditorTools.Level
{
    public sealed class BakeTiles
    {
        public const int PixelsPerUnit = 32;
        public const int Supersampling = 2;
        private const int MaxWidth = 2048;

        public Vector2 Origin { get; }
        public int Count { get; }
        public Vector2Int Size { get; }

        private BakeTiles(Vector2 origin, int count, Vector2Int size)
        {
            Origin = origin;
            Count = count;
            Size = size;
        }

        public int HalfWidth => Size.x / 2;

        public Vector2 HalfSizeInUnits => new(HalfWidth / (float)PixelsPerUnit, Size.y / (float)PixelsPerUnit);

        public Vector2Int RenderSize => new(HalfWidth * Supersampling, Size.y * Supersampling);

        public static BakeTiles Covering(Bounds bounds)
        {
            var origin = new Vector2(Mathf.Floor(bounds.min.x * PixelsPerUnit), Mathf.Floor(bounds.min.y * PixelsPerUnit)) / PixelsPerUnit;
            var width = Mathf.CeilToInt((bounds.max.x - origin.x) * PixelsPerUnit);
            var count = Mathf.CeilToInt(width / (float)MaxWidth);
            var size = new Vector2Int(Ceil4(Mathf.CeilToInt(width / (float)count)), Ceil4(Mathf.CeilToInt((bounds.max.y - origin.y) * PixelsPerUnit)));
            return new BakeTiles(origin, count, size);
        }

        public Vector3 Corner(int tile) =>
            new(Origin.x + tile * Size.x / (float)PixelsPerUnit, Origin.y, 0f);

        public Vector3 HalfCenter(int tile, int half) =>
            Corner(tile) + new Vector3((half + 0.5f) * HalfSizeInUnits.x, HalfSizeInUnits.y / 2f, -10f);

        private static int Ceil4(int value) =>
            (value + 3) / 4 * 4;
    }
}
