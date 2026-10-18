using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Props.View.Flight
{
    public sealed class PropFlightView
    {
        private readonly PropFlightFx _fx;
        private readonly SpriteRenderer _visual;
        private readonly PropVfxConfig _config;

        private float _untilGhost;

        public PropFlightView(
            Lifetime lifetime,
            PropMotion motion,
            PropFlightFx fx,
            SpriteRenderer visual,
            PropVfxConfig config)
        {
            _fx = fx;
            _visual = visual;
            _config = config;

            motion.Moved.Advise(lifetime, Show);
        }

        private void Show(PropMotionFrame frame)
        {
            _fx.Trail.emitting = frame.Fast;
            var lines = _fx.Lines.emission;
            lines.enabled = frame.Fast;
            if (frame.Fast)
            {
                var main = _fx.Lines.main;
                main.startRotation = -Mathf.Atan2(frame.Velocity.y, frame.Velocity.x);
            }

            _untilGhost -= Time.deltaTime;
            if (!frame.Fast || frame.Held || _untilGhost > 0f)
                return;

            _untilGhost = _config.GhostInterval;
            EmitGhost();
        }

        private void EmitGhost()
        {
            var sprite = _visual.sprite;
            var size = Vector3.Scale(sprite.bounds.size, _visual.transform.lossyScale);
            _fx.Ghosts.textureSheetAnimation.SetSprite(0, sprite);
            _fx.Ghosts.Emit(new ParticleSystem.EmitParams
            {
                position = _visual.bounds.center,
                rotation = -_visual.transform.eulerAngles.z,
                startSize3D = new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), 1f),
                applyShapeToPosition = false
            }, 1);
        }
    }
}
