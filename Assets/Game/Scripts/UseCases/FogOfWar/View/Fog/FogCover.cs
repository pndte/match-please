using Bw.Entities.Extensions;
using JetBrains.Lifetimes;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Bw.UseCases.FogOfWar.View.Fog
{
    public sealed class FogCover
    {
        private const float NearGap = 0.01f;

        private static readonly int SightMaskTexture = Shader.PropertyToID("_SightMask");
        private static readonly int SightArea = Shader.PropertyToID("_SightArea");
        private static readonly int FogColor = Shader.PropertyToID("_FogColor");
        private static readonly int Softness = Shader.PropertyToID("_Softness");
        private static readonly int Grain = Shader.PropertyToID("_Grain");
        private static readonly int HatchColor = Shader.PropertyToID("_HatchColor");
        private static readonly int HatchAcross = Shader.PropertyToID("_HatchAcross");
        private static readonly int HatchSpacing = Shader.PropertyToID("_HatchSpacing");
        private static readonly int HatchWidth = Shader.PropertyToID("_HatchWidth");
        private static readonly int HatchLength = Shader.PropertyToID("_HatchLength");

        private readonly UnityEngine.Camera _camera;
        private readonly Transform _root;
        private readonly MeshRenderer _hiding;
        private readonly MeshRenderer _fog;
        private readonly Material _hidingMaterial;
        private readonly Material _fogMaterial;
        private readonly FogOfWarConfig _config;

        private FogCover(
            Lifetime lifetime,
            UnityEngine.Camera camera,
            Transform root,
            MeshRenderer hiding,
            MeshRenderer fog,
            Material hidingMaterial,
            Material fogMaterial,
            FogOfWarConfig config)
        {
            _camera = camera;
            _root = root;
            _hiding = hiding;
            _fog = fog;
            _hidingMaterial = hidingMaterial;
            _fogMaterial = fogMaterial;
            _config = config;

            RenderPipelineManager.beginCameraRendering += Reveal;
            RenderPipelineManager.endCameraRendering += Conceal;
            lifetime.OnTermination(() =>
            {
                RenderPipelineManager.beginCameraRendering -= Reveal;
                RenderPipelineManager.endCameraRendering -= Conceal;
            });
        }

        public static FogCover Create(
            Lifetime lifetime,
            UnityEngine.Camera camera,
            SightMask mask,
            FogOfWarConfig config)
        {
            var root = new GameObject("FogOfWar");
            root.transform.SetParent(camera.transform, false);
            root.transform.localPosition = new Vector3(0f, 0f, camera.nearClipPlane + NearGap);
            var coverLifetime = lifetime.Intersect(root.Lifetime());
            var quad = CreateQuad();
            var hidingMaterial = new Material(config.HidingShader) { name = "FogOfWarHiding" }; //TODO: прятанье держится на буфере глубины 2D-рендерера (Renderer2D → Depth/Stencil Buffer): если его выключить, всё за стенами станет видно, а проверки на это нет
            var fogMaterial = new Material(config.FogShader) { name = "FogOfWar" };
            hidingMaterial.SetTexture(SightMaskTexture, mask.Texture);
            fogMaterial.SetTexture(SightMaskTexture, mask.Texture);
            coverLifetime.OnTermination(() =>
            {
                Object.Destroy(root);
                Object.Destroy(quad);
                Object.Destroy(hidingMaterial);
                Object.Destroy(fogMaterial);
            });

            var hiding = CreateLayer(root.transform, "Hiding", quad, hidingMaterial, config.HidingOrder); //TODO: прячет только то, что сортируется не ниже HidingOrder и проверяет глубину (ZTest LEqual, как у спрайтов): пятна крови (порядок -9) и следы пуль на стенах (-7) бледно видны под туманом, объект с порядком ниже или с ZTest Always будет виден сквозь стены
            var fog = CreateLayer(root.transform, "Fog", quad, fogMaterial, config.FogOrder);
            return new FogCover(coverLifetime, camera, root.transform, hiding, fog, hidingMaterial, fogMaterial, config);
        }

        public void Cover(Rect area)
        {
            var bounds = new Vector4(area.xMin, area.yMin, 1f / area.width, 1f / area.height);
            var angle = _config.HatchAngle * Mathf.Deg2Rad;
            _root.localScale = new Vector3(area.width, area.height, 1f);
            _hidingMaterial.SetVector(SightArea, bounds);
            _fogMaterial.SetVector(SightArea, bounds);
            _fogMaterial.SetColor(FogColor, _config.FogColor);
            _fogMaterial.SetFloat(Softness, _config.Softness);
            _fogMaterial.SetFloat(Grain, _config.Grain);
            _fogMaterial.SetColor(HatchColor, _config.HatchColor);
            _fogMaterial.SetVector(HatchAcross, new Vector4(-Mathf.Sin(angle), Mathf.Cos(angle), 0f, 0f));
            _fogMaterial.SetFloat(HatchSpacing, _config.HatchSpacing);
            _fogMaterial.SetFloat(HatchWidth, _config.HatchWidth);
            _fogMaterial.SetFloat(HatchLength, _config.HatchLength);
        }

        private void Reveal(ScriptableRenderContext context, UnityEngine.Camera camera)
        {
            _hiding.enabled = camera == _camera;
            _fog.enabled = camera == _camera;
        }

        private void Conceal(ScriptableRenderContext context, UnityEngine.Camera camera)
        {
            _hiding.enabled = false;
            _fog.enabled = false;
        }

        private static Mesh CreateQuad()
        {
            var quad = new Mesh { name = "FogOfWar" };
            quad.SetVertices(new[]
            {
                new Vector3(-0.5f, -0.5f, 0f),
                new Vector3(0.5f, -0.5f, 0f),
                new Vector3(0.5f, 0.5f, 0f),
                new Vector3(-0.5f, 0.5f, 0f)
            });
            quad.SetTriangles(new[] { 0, 1, 2, 0, 2, 3 }, 0);
            return quad;
        }

        private static MeshRenderer CreateLayer(
            Transform root,
            string name,
            Mesh quad,
            Material material,
            int order)
        {
            var layer = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            layer.transform.SetParent(root, false);
            layer.GetComponent<MeshFilter>().sharedMesh = quad;
            var renderer = layer.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.sortingOrder = order;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            renderer.enabled = false;
            return renderer;
        }
    }
}
