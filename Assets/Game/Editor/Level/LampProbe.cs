using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Bw.EditorTools.Level
{
    public sealed class LampProbe : IDisposable
    {
        private const string ShaderName = "Hidden/Bw/Light-Probe";

        private readonly Material _material;
        private readonly Sprite _white;
        private readonly SpriteRenderer _quad;

        private LampProbe(Material material, Sprite white, SpriteRenderer quad)
        {
            _material = material;
            _white = white;
            _quad = quad;
        }

        public static LampProbe Create(Scene preview)
        {
            var material = new Material(Shader.Find(ShaderName) ?? throw new InvalidOperationException($"No shader '{ShaderName}': the lamp bake reads the 2D light through it."));
            var texture = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            texture.SetPixels32(Enumerable.Repeat(new Color32(255, 255, 255, 255), 16).ToArray());
            texture.Apply();
            var white = Sprite.Create(texture, new Rect(0, 0, 4, 4), Vector2.zero, 4f);
            var quadObject = new GameObject("Light Probe");
            SceneManager.MoveGameObjectToScene(quadObject, preview);
            var quad = quadObject.AddComponent<SpriteRenderer>();
            quad.sprite = white;
            quad.sharedMaterial = material;
            quad.sortingOrder = short.MaxValue;
            return new LampProbe(material, white, quad);
        }

        public void Isolate(LampScene scene)
        {
            var chunks = scene.Chunks;
            foreach (var renderer in scene.Renderers)
                renderer.enabled = renderer is SpriteRenderer sprite && chunks.Contains(sprite);

            foreach (var chunk in chunks)
                chunk.sharedMaterial = _material;

            var lamps = scene.Lamps;
            foreach (var light in scene.Lights.Where(light => light.lightType != Light2D.LightType.Global && !lamps.Contains(light)))
                light.enabled = false;

            foreach (var caster in scene.MovingCasters)
                caster.enabled = false;

            scene.LightRoot.SetActive(false);
            scene.HazeRoot.SetActive(false);
        }

        public void ShowFlat(Vector2 halfSize, Vector3 center)
        {
            _quad.enabled = true;
            _quad.transform.position = new Vector3(center.x - halfSize.x / 2f - 1f, center.y - halfSize.y / 2f - 1f, 0f);
            _quad.transform.localScale = new Vector3(halfSize.x + 2f, halfSize.y + 2f, 1f);
        }

        public void ShowChunks() =>
            _quad.enabled = false;

        public void Dispose()
        {
            Object.DestroyImmediate(_white.texture);
            Object.DestroyImmediate(_white);
            Object.DestroyImmediate(_material);
        }
    }
}
