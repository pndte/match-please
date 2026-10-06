using Bw.Entities.Simulation;
using Bw.UseCases.Audio.View.Playback.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Movement.View.Audio
{
    public sealed class MovementSoundsView : IStateView<MovementState>
    {
        private readonly ISoundPlayer _player;
        private readonly MovementConfig _movementConfig;
        private readonly MovementSoundsConfig _config;

        private bool _shown;
        private bool _grounded;
        private float _untilStep;

        public MovementSoundsView(ISoundPlayer player, MovementConfig movementConfig, MovementSoundsConfig config)
        {
            _player = player;
            _movementConfig = movementConfig;
            _config = config;
        }

        public void Show(MovementState from, MovementState to, float progress)
        {
            var position = Vector2.Lerp(from.Position, to.Position, progress);
            var velocity = Vector2.Lerp(from.Velocity, to.Velocity, progress);
            var grounded = progress < 0.5f ? from.Grounded : to.Grounded;

            if (_shown && grounded && !_grounded)
                Land(position);
            if (_shown && !grounded && _grounded && velocity.y > 0f)
                _player.Play(_config.Jump, position);
            _shown = true;
            _grounded = grounded;

            if (grounded && Mathf.Abs(velocity.x) / _movementConfig.Speed > _config.MinStepSpeed)
                Step(position);
            else
                _untilStep = 0f;
        }

        private void Land(Vector2 position)
        {
            _untilStep = _config.StepInterval;
            _player.Play(_config.Land, position);
        }

        private void Step(Vector2 position)
        {
            _untilStep -= Time.deltaTime;
            if (_untilStep > 0f)
                return;

            _untilStep = Mathf.Max(0f, _untilStep + _config.StepInterval);
            _player.Play(_config.Step, position); //TODO: шаги, прыжок и приземление всегда звучат как по траве — для других поверхностей звук нужно брать у поверхности под ногами
        }
    }
}
