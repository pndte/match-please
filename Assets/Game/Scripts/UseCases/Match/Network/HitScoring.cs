using Bw.Entities.Network.Prediction.Events;
using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Extensions;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Match.Network
{
    public sealed class HitScoring //TODO: попадания брошенных ящиков матч не слышит: их исход публикует сам ящик (ThrownPropHits), а здесь слушаются только выстрелы и удары сессии — урон и убийство броском не засчитываются
    {
        private readonly Combatants _combatants;
        private readonly ScoreKeeper _keeper;

        public HitScoring(
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
            if (!_combatants.TryFindShooter(outcome.Initiator, out var attacker)) //TODO: выстрел стрелка, убитого раньше в том же тике, уже без носителя — его урон и убийство никому не засчитываются
                return;

            foreach (var affected in outcome.Affected)
                if (_combatants.TryFindPlayer(affected.Target, out var victim) && victim != attacker)
                    _keeper.CountHit(attacker, outcome.Initiator, affected.Target, affected.Effect.Damage());
        }
    }
}
