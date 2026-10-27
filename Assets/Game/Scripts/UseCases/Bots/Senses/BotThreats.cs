using Bw.Entities.Simulation;
using Bw.UseCases.Combat.Abstractions.Characters;
using UnityEngine;

namespace Bw.UseCases.Bots.Senses
{
    public sealed class BotThreats
    {
        private readonly BotHearing _hearing;
        private readonly BotWeapons _weapons;
        private readonly IReadonlyCharacter _self;
        private readonly BotDifficultyConfig _difficulty;
        private readonly BotsConfig _bots;
        private readonly ISimulationStep _step;

        public BotThreats(
            BotHearing hearing,
            BotWeapons weapons,
            IReadonlyCharacter self,
            BotDifficultyConfig difficulty,
            BotsConfig bots,
            ISimulationStep step)
        {
            _hearing = hearing;
            _weapons = weapons;
            _self = self;
            _difficulty = difficulty;
            _bots = bots;
            _step = step;
        }

        public Threat ThreatAfter(Threat previous)
        {
            var memory = Ticks(_bots.ThreatMemory);
            if (_hearing.Heard != previous.Heard)
                return Attacked(previous, _hearing.Shooter, memory);

            if (_weapons.TryFindCharger(_self, _bots.ChargeThreatRange, out var charger))
                return Attacked(previous, charger, memory);

            var quiet = Mathf.Min(previous.QuietTicks + 1, memory + 1);
            return new Threat(previous.Attacker, _hearing.Heard, previous.SinceTicks + 1, quiet, previous.Noticed && quiet <= memory);
        }

        private Threat Attacked(Threat previous, IReadonlyCharacter attacker, int memory)
        {
            var going = previous.Attacker == attacker && previous.QuietTicks <= memory;
            var since = going ? previous.SinceTicks + 1 : 1;
            return new Threat(attacker, _hearing.Heard, since, 0, since >= Ticks(_difficulty.ReactionTime));
        }

        private int Ticks(float seconds) =>
            Mathf.CeilToInt(seconds / _step.Duration);
    }
}
