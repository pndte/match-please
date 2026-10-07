using System.Collections.Generic;
using Bw.Entities;
using Bw.Entities.Network.LagCompensation;
using Bw.Entities.Network.Ticks;
using Bw.UseCases.Character;
using Bw.UseCases.Character.Extensions;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.Weapon.Network
{
    public sealed class RaycastShotResolver : IRaycastShots
    {
        private readonly List<RaycastShot> _shots = new();
        private readonly List<CharacterHit> _hits = new();
        private readonly ILagCompensator _lagCompensator;

        public RaycastShotResolver(Lifetime lifetime, INetworkTicks ticks, ILagCompensator lagCompensator)
        {
            _lagCompensator = lagCompensator;

            ticks.Ticked(TickPhase.Late).Advise(lifetime, Resolve);
        }

        public void Submit(RaycastShot shot) =>
            _shots.Add(shot);

        private void Resolve(int tick)
        {
            try
            {
                for (var index = 0; index < _shots.Count; index++)
                    Trace(tick, _shots[index]);

                for (var index = 0; index < _hits.Count; index++)
                    if (_hits[index].Character.State.Value == CharacterState.Alive)
                        _hits[index].Character.Hit(_hits[index].Damage);
            }
            finally
            {
                _shots.Clear();
                _hits.Clear();
            }
        }

        private void Trace(int tick, RaycastShot shot)
        {
            var ray = shot.Tracer.Aim(shot.Shot.Aim);
            var hit = Lifetime.Using(rewindLifetime =>
            {
                _lagCompensator.Rewind(rewindLifetime, tick - shot.Shot.ViewDelay);
                return shot.Tracer.Cast(ray).Hit;
            });

            if (hit && hit.collider.TryGetComponent<IHolder<ICharacter>>(out var characterHolder))
                _hits.Add(new CharacterHit(characterHolder.Value, shot.Damage));
        }

        private readonly struct CharacterHit
        {
            public readonly ICharacter Character;
            public readonly float Damage;

            public CharacterHit(ICharacter character, float damage)
            {
                Character = character;
                Damage = damage;
            }
        }
    }
}
