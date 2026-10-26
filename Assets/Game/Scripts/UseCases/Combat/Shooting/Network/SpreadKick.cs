using Bw.Entities;
using Bw.Entities.Simulation;
using Bw.UseCases.Combat.Shooting.Abstractions;
using Bw.UseCases.Combat.Weapon.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Combat.Shooting.Network
{
    public sealed class SpreadKick : IStateView<GunState> //TODO: бег, прыжок и полёт от отбрасывания ствол не сбивают
    {
        private readonly IReadonlyControlledBy _controlledBy;
        private readonly ISpread _spread;
        private readonly IAimDeflection _deflection;

        private bool _synchronized;
        private int _shots;

        public SpreadKick(IReadonlyControlledBy controlledBy, ISpread spread, IAimDeflection deflection)
        {
            _controlledBy = controlledBy;
            _spread = spread;
            _deflection = deflection;
        }

        public void Show(GunState from, GunState to, float progress)
        {
            var fired = _synchronized && to.Shots > _shots;
            _shots = to.Shots;
            _synchronized = true;

            if (fired && _controlledBy.Me.Value)
                Kick(to.Aim);
        }

        private void Kick(float aim) =>
            _deflection.Kick(_spread.ThrowOf(aim, Random.Range(-1f, 1f)));
    }
}
