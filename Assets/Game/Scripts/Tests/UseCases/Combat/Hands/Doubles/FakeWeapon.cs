using System.Collections.Generic;
using Bw.UseCases.Combat.Abstractions.Weapons;
using JetBrains.Collections.Viewable;

namespace Bw.Tests.UseCases.Combat.Hands.Doubles
{
    public sealed class FakeWeapon : IWeapon
    {
        public ISource<WeaponShot> Fired => _fired;
        public bool Stowable { get; set; } = true;
        public bool Rooting { get; set; }
        public IReadOnlyList<WeaponInput> Steps => _steps;

        private readonly Signal<WeaponShot> _fired = new();
        private readonly List<WeaponInput> _steps = new();

        public void Step(WeaponInput input) =>
            _steps.Add(input);
    }
}
