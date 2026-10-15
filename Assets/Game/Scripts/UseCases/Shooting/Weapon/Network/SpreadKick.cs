using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon.Network
{
    public sealed class SpreadKick //TODO: бег, прыжок и полёт от отбрасывания ствол не сбивают
    {
        private readonly ISpread _spread;
        private readonly IAimDeflection _deflection;

        public SpreadKick(
            Lifetime lifetime,
            IControlledBy controlledBy,
            IReadonlyWeapon weapon,
            ISpread spread,
            IAimDeflection deflection)
        {
            _spread = spread;
            _deflection = deflection;

            controlledBy.Me.WhenTrue(lifetime, controlledLifetime =>
                weapon.Fired.Advise(controlledLifetime, Kick));
        }

        private void Kick(WeaponShot shot) => //TODO: ствол подбрасывает только клиент стрелка, сервер этого не проверяет: чит может стрелять без подброса
            _deflection.Kick(_spread.ThrowOf(shot.Aim, Random.Range(-1f, 1f)));
    }
}
