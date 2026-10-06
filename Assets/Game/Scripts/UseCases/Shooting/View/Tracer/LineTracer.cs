using DG.Tweening;
using UnityEngine;

namespace Bw.UseCases.Shooting.View.Tracer
{
    public sealed class LineTracer : MonoBehaviour
    {
        [SerializeField] private LineRenderer _line;
        [SerializeField, Range(0f, 1f)] private float _length = 0.1f;

        public void Play(Vector2 from, Vector2 to, float duration)
        {
            Draw(from, to, 0f);
            _line.enabled = true;
            DOVirtual.Float(0f, 1f, duration, progress => Draw(from, to, progress))
                .SetEase(Ease.Linear)
                .SetLink(gameObject, LinkBehaviour.KillOnDisable);
        }

        private void Draw(Vector2 from, Vector2 to, float progress)
        {
            _line.SetPosition(1, Vector2.Lerp(from, to, progress));
            _line.SetPosition(0, Vector2.Lerp(from, to, Mathf.Max(0f, progress - _length)));
        }
    }
}
