using Bw.Entities.Network.Prediction.Events;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Extensions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Match.Network
{
    public sealed class HitScorer //TODO: попадания ящиков матч не слышит: их исход публикует сам ящик (StrikeOutcomes в его контейнере), а здесь слушаются только выстрелы и удары сессии — урон и убийство ящиком не засчитываются
    {
        private readonly Combatants _combatants;
        private readonly ScoreKeeper _keeper;

        public HitScorer(
            Lifetime lifetime,
            IOutcomes<IReadonlyWeapon, IReadonlyMortal, Impacts> hits,
            Combatants combatants,
            ScoreKeeper keeper)
        {
            _combatants = combatants;
            _keeper = keeper;

            hits.Reported.Advise(lifetime, Count);
        }

        private void Count(ActionOutcome<IReadonlyWeapon, IReadonlyMortal, Impacts> outcome)
        {
            foreach (var affected in outcome.Affected)
                if (_combatants.TryFindVictim(affected.Target, out var victim) && victim != outcome.Actor)
                    _keeper.CountHit(outcome.Actor, outcome.Initiator, affected.Target, affected.Effect.Damage());
        }
    }
}
