using Bw.UseCases.Audio.View.Playback.Abstractions;
using Bw.UseCases.Props.View.Abstractions;
using Bw.UseCases.Props.View.Flight;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Props.View.Audio
{
    public sealed class PropSoundsView //TODO: придумать решение с кодогенерацией, околоуниверсальное
                                       // удобное и для гд и для разработчиков
    {
        private readonly ISoundPlayer _player;
        private readonly PropSoundsConfig _sounds;
        private readonly Transform _prop;
        private readonly SequentialLifetimes _flights;

        private bool _flying;

        public PropSoundsView(
            Lifetime lifetime,
            IPropThrowsViewModel throws,
            PropMotion motion,
            ISoundPlayer player,
            PropSoundsConfig sounds,
            Transform prop)
        {
            _player = player;
            _sounds = sounds;
            _prop = prop;
            _flights = new SequentialLifetimes(lifetime);

            throws.Thrown.Advise(lifetime, _ => _player.Play(_sounds.Throw, _prop.position));
            motion.Moved.Advise(lifetime, ShowFlight);
            motion.Impacted.Advise(lifetime, impact => _player.Play(_sounds.Land, impact.Point));
        }

        private void ShowFlight(PropMotionFrame frame)
        {
            var flying = frame.Fast && !frame.Held;
            if (flying && !_flying)
                _player.Play(_flights.Next(), _sounds.Flight, _prop);

            if (!flying && _flying)
                _flights.TerminateCurrent();

            _flying = flying;
        }
    }
}
