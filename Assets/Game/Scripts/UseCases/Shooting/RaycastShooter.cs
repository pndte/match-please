using Bw.Entities;
using Bw.UseCases.Character;
using Bw.UseCases.Shooting.Weapon;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting
{
    public sealed class RaycastShooter
    {
        private readonly IShotTracer _tracer;
        private readonly ShootingWeaponConfig _config;

        public RaycastShooter(
            Lifetime lifetime,
            IReadonlyWeapon weapon,
            IShotTracer tracer,
            ShootingWeaponConfig config)
        {
            _tracer = tracer;
            _config = config;

            weapon.Fired.Advise(lifetime, Shoot);
        }

        private void Shoot(WeaponState state) //TODO: нет лаг-компенсации: стрелок видел цели в прошлом (InterpolationTick), а сервер бьёт по текущим позициям; ещё оружие может шагнуть в тике раньше своего персонажа — тогда дуло на тик позади
        {
            var hit = _tracer.Trace(state.Aim).Hit;
            if (hit && hit.collider.TryGetComponent<IHolder<ICharacter>>(out var characterHolder))
                characterHolder.Value.Health.Current.Value -= _config.Damage;
        }
    }
}
