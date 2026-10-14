using System;
using System.Runtime.InteropServices;
using Bw.UseCases.FogOfWar.View.Sight;
using JetBrains.Lifetimes;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Bw.UseCases.FogOfWar.View.Fog
{
    public sealed class SightMask
    {
        private const float FaceTolerance = 0.01f;
        private const MeshUpdateFlags Quietly = MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontResetBoneBounds | MeshUpdateFlags.DontNotifyMeshUsers | MeshUpdateFlags.DontRecalculateBounds;

        private static readonly Color32 SeenArea = new(255, 255, 0, 0);
        private static readonly Color32 ObjectsShown = new(255, 0, 0, 0);
        private static readonly Color32 FogCleared = new(0, 255, 0, 0);
        private static readonly VertexAttributeDescriptor[] Layout =
        {
            new(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            new(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4)
        };

        public Texture Texture => _texture;

        private readonly RenderTexture _texture;
        private readonly Mesh _mesh;
        private readonly Material _material;
        private readonly CommandBuffer _commands;
        private readonly FogOfWarConfig _config;

        private Vertex[] _vertices = Array.Empty<Vertex>();
        private int[] _indices = Array.Empty<int>();
        private int _vertexCount;
        private int _indexCount;

        private SightMask(
            RenderTexture texture,
            Mesh mesh,
            Material material,
            CommandBuffer commands,
            FogOfWarConfig config)
        {
            _texture = texture;
            _mesh = mesh;
            _material = material;
            _commands = commands;
            _config = config;
        }

        public static SightMask Create(Lifetime lifetime, Vector2Int size, FogOfWarConfig config)
        {
            var texture = new RenderTexture(size.x, size.y, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
            {
                name = "SightMask",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            texture.Create();
            var mesh = new Mesh { name = "Sight" };
            mesh.MarkDynamic();
            var material = new Material(config.MaskShader) { name = "SightMask" };
            var commands = new CommandBuffer { name = "SightMask" };
            lifetime.OnTermination(() =>
            {
                commands.Release();
                Object.Destroy(material);
                Object.Destroy(mesh);
                Object.Destroy(texture);
            });

            var mask = new SightMask(texture, mesh, material, commands, config);
            mask.Blind();
            return mask;
        }

        public void Draw(LineOfSight sight, Vector2 eye, Rect area)
        {
            Build(sight, eye);
            Upload();
            _commands.Clear();
            _commands.SetRenderTarget(_texture);
            _commands.ClearRenderTarget(false, true, Color.clear);
            _commands.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.Ortho(area.xMin, area.xMax, area.yMin, area.yMax, -1f, 1f));
            _commands.DrawMesh(_mesh, Matrix4x4.identity, _material, 0, 0);
            Graphics.ExecuteCommandBuffer(_commands);
        }

        private void Blind()
        {
            _commands.Clear();
            _commands.SetRenderTarget(_texture);
            _commands.ClearRenderTarget(false, true, Color.clear);
            Graphics.ExecuteCommandBuffer(_commands);
        }

        private void Build(LineOfSight sight, Vector2 eye)
        {
            var count = sight.Count;
            Reserve(1 + 9 * count, 15 * count);
            _vertexCount = 0;
            _indexCount = 0;
            AddVertex(eye, SeenArea);
            for (var index = 0; index < count; index++)
                AddVertex(sight.Ray(index).End, SeenArea);

            for (var index = 0; index < count; index++)
            {
                var next = (index + 1) % count;
                AddTriangle(0, 1 + index, 1 + next);
                var from = sight.Ray(index);
                var to = sight.Ray(next);
                if (!OnOneFace(from, to))
                    continue;

                AddBand(from, to, _config.HidingDepth, ObjectsShown);
                AddBand(from, to, _config.WallDepth, FogCleared);
            }
        }

        private void AddBand(
            SightRay from,
            SightRay to,
            float depth,
            Color32 color)
        {
            if (depth <= 0f)
                return;

            var start = _vertexCount;
            AddVertex(from.End, color);
            AddVertex(to.End, color);
            AddVertex(to.End - to.Normal * depth, color);
            AddVertex(from.End - from.Normal * depth, color);
            AddTriangle(start, start + 1, start + 2);
            AddTriangle(start, start + 2, start + 3);
        }

        private void AddVertex(Vector2 position, Color32 color) =>
            _vertices[_vertexCount++] = new Vertex(position, color);

        private void AddTriangle(int first, int second, int third)
        {
            _indices[_indexCount++] = first;
            _indices[_indexCount++] = second;
            _indices[_indexCount++] = third;
        }

        private void Reserve(int vertices, int indices)
        {
            if (vertices > _vertices.Length)
            {
                _vertices = new Vertex[Mathf.NextPowerOfTwo(vertices)];
                _mesh.SetVertexBufferParams(_vertices.Length, Layout);
            }

            if (indices > _indices.Length)
            {
                _indices = new int[Mathf.NextPowerOfTwo(indices)];
                _mesh.SetIndexBufferParams(_indices.Length, IndexFormat.UInt32);
            }
        }

        private void Upload()
        {
            _mesh.SetVertexBufferData(_vertices, 0, 0, _vertexCount, 0, Quietly);
            _mesh.SetIndexBufferData(_indices, 0, 0, _indexCount, Quietly);
            _mesh.SetSubMesh(0, new SubMeshDescriptor(0, _indexCount) { vertexCount = _vertexCount }, Quietly);
        }

        private static bool OnOneFace(SightRay from, SightRay to) =>
            from.IsHit && to.IsHit && Vector2.Dot(from.Normal, to.Normal) > 0.999f && Mathf.Abs(Vector2.Dot(to.End - from.End, from.Normal)) < FaceTolerance;

        [StructLayout(LayoutKind.Sequential)]
        private readonly struct Vertex
        {
            public readonly Vector3 Position;
            public readonly Color32 Color;

            public Vertex(Vector2 position, Color32 color)
            {
                Position = position;
                Color = color;
            }
        }
    }
}
