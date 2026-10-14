using DG.Tweening;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Shooting.View.Highlight
{
    public sealed class WeaponHighlightView
    {
        private static readonly int HighlightId = Shader.PropertyToID("_Highlight");
        private static readonly int SpriteUvRectId = Shader.PropertyToID("_SpriteUvRect");
        private static readonly int UvPerUnitId = Shader.PropertyToID("_UvPerUnit");
        private static readonly int SpriteCenterId = Shader.PropertyToID("_SpriteCenter");

        private readonly WeaponHighlightConfig _config;
        private readonly Transform _visual;
        private readonly SpriteRenderer[] _parts;
        private readonly Material[] _materials;
        private readonly MaterialPropertyBlock _block = new();

        private bool _worn;
        private float _amount;

        public WeaponHighlightView(
            Lifetime lifetime,
            IHighlightViewModel highlight,
            WeaponHighlightConfig config,
            Transform visual,
            SpriteRenderer[] parts)
        {
            _config = config;
            _visual = visual;
            _parts = parts;
            _materials = new Material[parts.Length];

            highlight.Highlighted.WhenTrue(lifetime, Show);
        }

        private void Show(Lifetime highlightLifetime)
        {
            Wear();
            DOTween.Kill(this);
            Fade(1f, _config.FadeInTime);
            highlightLifetime.OnTermination(() =>
            {
                DOTween.Kill(this);
                Fade(0f, _config.FadeOutTime).OnComplete(TakeOff);
            });
        }

        private Tweener Fade(float amount, float seconds) =>
            DOTween.To(() => _amount, Apply, amount, seconds)
                .SetEase(Ease.OutQuad)
                .SetTarget(this)
                .SetLink(_visual.gameObject);

        private void Wear()
        {
            if (_worn)
                return;

            _worn = true;
            for (var index = 0; index < _parts.Length; index++)
            {
                var part = _parts[index];
                _materials[index] = part.sharedMaterial;
                part.sharedMaterial = _config.Material;
                part.GetPropertyBlock(_block);
                if (part.sprite != null)
                    Describe(part.sprite);

                _block.SetFloat(HighlightId, _amount);
                part.SetPropertyBlock(_block);
            }
        }

        private void TakeOff()
        {
            _worn = false;
            for (var index = 0; index < _parts.Length; index++)
            {
                _block.Clear();
                _parts[index].SetPropertyBlock(_block);
                _parts[index].sharedMaterial = _materials[index];
            }
        }

        private void Describe(Sprite sprite)
        {
            var texture = sprite.texture;
            var rect = sprite.textureRect;
            _block.SetVector(SpriteUvRectId, new Vector4(rect.xMin / texture.width, rect.yMin / texture.height, rect.xMax / texture.width, rect.yMax / texture.height));
            _block.SetVector(UvPerUnitId, new Vector4(sprite.pixelsPerUnit / texture.width, sprite.pixelsPerUnit / texture.height, 0f, 0f));
            _block.SetVector(SpriteCenterId, sprite.bounds.center);
        }

        private void Apply(float amount)
        {
            _amount = amount;
            for (var index = 0; index < _parts.Length; index++)
            {
                _parts[index].GetPropertyBlock(_block);
                _block.SetFloat(HighlightId, amount);
                _parts[index].SetPropertyBlock(_block);
            }
        }
    }
}
