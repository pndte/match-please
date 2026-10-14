using Bw.Entities.Pool;
using Bw.UseCases.Shooting.View.Abstractions;
using DG.Tweening;
using JetBrains.Collections.Viewable;
using JetBrains.Core;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Shooting.View.Tracer
{
    public sealed class BulletTracer : MonoBehaviour, IResource<IBulletTracer>, IBulletTracer
    {
        public ISource<Unit> Finished => _finished;

        [SerializeField] private LineRenderer _core;
        [SerializeField] private LineRenderer _glow;
        [SerializeField] private SpriteRenderer _head;
        [SerializeField, Min(0.001f)] private float _trailTime = 0.018f;
        private readonly Signal<Unit> _finished = new();

        public IBulletTracer Facade(Lifetime lifetime)
        {
            lifetime.OnTermination(Hide);
            return this;
        }

        public void Fly(Vector2 from, Vector2 to, float speed)
        {
            var distance = Vector2.Distance(from, to);
            var direction = (to - from) / Mathf.Max(distance, 1e-5f);
            var length = speed * _trailTime;
            _head.transform.rotation = Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.right, direction));
            Show(true);
            Draw(from, direction, distance, length, 0f);
            DOVirtual.Float(0f, distance + length, (distance + length) / speed, travelled => Draw(from, direction, distance, length, travelled))
                .SetEase(Ease.Linear)
                .SetLink(gameObject, LinkBehaviour.KillOnDisable)
                .OnComplete(() => _finished.Fire(Unit.Instance));
        }

        private void Draw(
            Vector2 from,
            Vector2 direction,
            float distance,
            float length,
            float travelled)
        {
            var head = from + direction * Mathf.Min(travelled, distance);
            var tail = from + direction * Mathf.Clamp(travelled - length, 0f, distance);
            Stretch(_core, tail, head);
            Stretch(_glow, tail, head);
            _head.transform.position = head;
            _head.enabled = travelled < distance;
        }

        private static void Stretch(LineRenderer line, Vector2 tail, Vector2 head)
        {
            line.SetPosition(0, tail);
            line.SetPosition(1, head);
        }

        private void Hide() =>
            Show(false);

        private void Show(bool shown)
        {
            _core.enabled = shown;
            _glow.enabled = shown;
            _head.enabled = shown;
        }
    }
}
