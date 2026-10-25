using System.Collections.Generic;
using Bw.UseCases.Combat.Abstractions.Weapons;
using JetBrains.Collections.Viewable;

namespace Bw.Tests.UseCases.Combat.Weapon.Doubles
{
    public sealed class FakeWeapon : IWeapon
    {
        public ISource<WeaponShot> Fired => _fired;
        public bool Stowable { get; set; } = true;
        public bool Rooting { get; set; }
        public IReadOnlyList<WeaponInput> Inputs => _inputs;

        private readonly Signal<WeaponShot> _fired = new();
        private readonly List<WeaponInput> _inputs = new();

        public void Step(WeaponInput input) =>
            _inputs.Add(input);

        public void Fire(float aim) =>
            _fired.Fire(new WeaponShot(aim));
    }
}
