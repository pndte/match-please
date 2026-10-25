using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Weapon.Abstractions;
using JetBrains.Collections.Viewable;

namespace Bw.Tests.UseCases.Combat.Weapon.Doubles
{
    public sealed class FakeFeed : IWeaponFeed
    {
        public ISource<WeaponInput> Fed => _fed;

        private readonly Signal<WeaponInput> _fed = new();

        public void Feed(WeaponInput input = default) =>
            _fed.Fire(input);
    }
}
