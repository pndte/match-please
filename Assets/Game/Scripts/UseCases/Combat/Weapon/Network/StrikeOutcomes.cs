using Bw.Entities.Network.Prediction.Events;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Weapon.Network.Prediction;
using Bw.UseCases.Mortality;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Weapon.Network
{
    public sealed class StrikeOutcomes : IOutcomes<IReadonlyWeapon, IReadonlyMortal, Impacts>
    {
        public ISource<ActionOutcome<IReadonlyWeapon, IReadonlyMortal, Impacts>> Reported => _reported;

        private readonly Signal<ActionOutcome<IReadonlyWeapon, IReadonlyMortal, Impacts>> _reported = new();
        private readonly IReadonlyWeapon _initiator;
        private readonly ICharacterRegistry _registry;

        public StrikeOutcomes(
            Lifetime lifetime,
            IReadonlyWeapon initiator,
            IStrikes strikes,
            ICharacterRegistry registry)
        {
            _initiator = initiator;
            _registry = registry;

            strikes.Struck.Advise(lifetime, Report);
        }

        private void Report(Strike strike) =>
            _reported.Fire(new ActionOutcome<IReadonlyWeapon, IReadonlyMortal, Impacts>(_initiator, _registry.PlayerByCharacter[strike.Actor], strike.Tick, strike.Affected));
    }
}
