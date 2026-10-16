using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Bw.EditorTools.Level
{
    public sealed class LampBakeCamera : IDisposable
    {
        private static readonly MethodInfo LightLateUpdate = typeof(Light2D).GetMethod("LateUpdate", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Light2D has no LateUpdate: a preview cannot build the lights' meshes without it.");

        private readonly Camera _camera;
        private readonly RenderTexture _target;
        private readonly Texture2D _readback;

        private LampBakeCamera(Camera camera, RenderTexture target, Texture2D readback)
        {
            _camera = camera;
            _target = target;
            _readback = readback;
        }

        public Color[] FilteredPixels
        {
            get
            {
                var previous = RenderTexture.active;
                RenderTexture.active = _target;
                _readback.ReadPixels(new Rect(0, 0, _target.width, _target.height), 0, 0);
                _readback.Apply();
                RenderTexture.active = previous;
                return Filtered(_readback.GetPixels(), _target.width, _target.height);
            }
        }

        public static LampBakeCamera Create(Scene preview, BakeTiles tiles)
        {
            var size = tiles.RenderSize;
            var target = new RenderTexture(size.x, size.y, 24, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
            var cameraObject = new GameObject("Lamp Bake Camera");
            SceneManager.MoveGameObjectToScene(cameraObject, preview);
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = tiles.HalfSizeInUnits.y / 2f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.allowMSAA = false;
            camera.allowHDR = true;
            camera.scene = preview;
            camera.targetTexture = target;
            cameraObject.AddComponent<UniversalAdditionalCameraData>().renderPostProcessing = false;
            return new LampBakeCamera(camera, target, new Texture2D(size.x, size.y, TextureFormat.RGBAHalf, false, true));
        }

        public void Render(LampScene scene, Vector3 center)
        {
            var lights = scene.Lights.Where(light => light.isActiveAndEnabled).ToList();
            var casters = scene.Casters.Where(caster => caster.isActiveAndEnabled).ToList();
            _camera.transform.position = center;
            for (var frame = 0; frame < 2; frame++)
            {
                foreach (var light in lights)
                    LightLateUpdate.Invoke(light, null);

                foreach (var caster in casters)
                    caster.Update();

                _camera.Render();
            }
        }

        public void Dispose()
        {
            _target.Release();
            Object.DestroyImmediate(_target);
            Object.DestroyImmediate(_readback);
        }

        private static Color[] Filtered(Color[] pixels, int width, int height)
        {
            const int super = BakeTiles.Supersampling;
            var filteredWidth = width / super;
            var filtered = new Color[filteredWidth * (height / super)];
            for (var y = 0; y < height / super; y++)
                for (var x = 0; x < filteredWidth; x++)
                {
                    var sum = Color.clear;
                    var covered = 0;
                    for (var sy = 0; sy < super; sy++)
                        for (var sx = 0; sx < super; sx++)
                        {
                            var pixel = pixels[(y * super + sy) * width + x * super + sx];
                            sum += pixel;
                            if (pixel.maxColorComponent > 0f)
                                covered++;
                        }

                    filtered[y * filteredWidth + x] = new Color(sum.r, sum.g, sum.b, covered) / (super * super);
                }

            return filtered;
        }
    }
}
