using UnityEngine;

namespace Bw.UseCases.Level.View.Platforms
{
    [ExecuteAlways]
    public sealed class PlatformSkin : MonoBehaviour
    {
        [SerializeField] private BoxCollider2D _body;
        [SerializeField] private SpriteRenderer _skin;
        [SerializeField] private PlatformSkinConfig _config;

        public void Fit()
        {
            var scale = (Vector2)transform.lossyScale;
            var solid = Vector2.Scale(_body.size, new Vector2(Mathf.Abs(scale.x), Mathf.Abs(scale.y)));
            var size = solid + new Vector2(
                _config.OverhangLeft + _config.OverhangRight,
                _config.OverhangBottom + _config.OverhangTop);
            var shift = new Vector2(
                _config.OverhangRight - _config.OverhangLeft,
                _config.OverhangTop - _config.OverhangBottom) / 2f;
            var localScale = new Vector3(1f / scale.x, 1f / scale.y, 1f);
            Vector3 localPosition = _body.offset + new Vector2(shift.x / scale.x, shift.y / scale.y);

            var skin = _skin.transform;
            if (_skin.size != size)
                _skin.size = size;
            if (skin.localScale != localScale)
                skin.localScale = localScale;
            if (skin.localPosition != localPosition)
                skin.localPosition = localPosition;
        }

        private void LateUpdate() =>
            Fit();
    }
}
