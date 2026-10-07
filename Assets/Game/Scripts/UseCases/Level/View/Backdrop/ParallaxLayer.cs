using UnityEngine;

namespace Bw.UseCases.Level.View.Backdrop
{
    [DefaultExecutionOrder(1000)]
    public sealed class ParallaxLayer : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField, Range(0f, 1f)] private float _depth;

        private void LateUpdate()
        {
            var unit = transform.parent.lossyScale.x;
            var sprite = _renderer.sprite;
            var spriteWidth = sprite.rect.width / sprite.pixelsPerUnit;
            var period = spriteWidth * transform.localScale.x;
            var viewWidth = 2f * _camera.orthographicSize * _camera.aspect / unit;
            var size = new Vector2((Mathf.CeilToInt(viewWidth / period) + 1) * spriteWidth, _renderer.size.y);
            if (_renderer.size != size)
                _renderer.size = size;

            //TODO: horizontal parallax only: vertically the layers stay pinned to the camera, so on tall levels (ConstructionSite) the far fence and city ride along at the bottom of the screen while the camera climbs
            var travel = _camera.transform.position.x * (1f - _depth) / unit;
            var position = transform.localPosition;
            position.x = Mathf.Repeat(-travel, period) - period / 2f;
            transform.localPosition = position;
        }
    }
}
