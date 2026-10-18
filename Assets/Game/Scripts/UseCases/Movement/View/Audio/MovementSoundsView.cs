using Bw.Entities.Simulation;
using Bw.UseCases.Audio.View.Playback.Abstractions;
using Bw.UseCases.Movement.View.Animation.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Movement.View.Audio
{
    public sealed class MovementSoundsView : IStateView<MovementMotion>
    {
        private readonly ISoundPlayer _player;
        private readonly MovementSoundsConfig _config;

        private bool _shown;
        private bool _grounded;
        private Vector2 _position;

        public MovementSoundsView(
            Lifetime lifetime,
            ISoundPlayer player,
            MovementSoundsConfig config,
            IFootsteps footsteps)
        {
            _player = player;
            _config = config;

            footsteps.Stepped.Advise(lifetime, _ => Step());
        }

        public void Show(MovementMotion from, MovementMotion to, float progress)
        {
            _position = Vector2.Lerp(from.Position, to.Position, progress);
            var rise = Mathf.Lerp(from.Rise, to.Rise, progress);
            var grounded = progress < 0.5f ? from.Grounded : to.Grounded;

            if (_shown && grounded && !_grounded)
                _player.Play(_config.Land, _position);

            if (_shown && !grounded && _grounded && rise > 0f)
                _player.Play(_config.Jump, _position);

            _shown = true;
            _grounded = grounded;
        }

        private void Step() =>
            _player.Play(_config.Step, _position); //TODO: шаги, прыжок и приземление всегда звучат как по траве — для других поверхностей звук нужно брать у поверхности под ногами
    }
}
