using System.Collections.Generic;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Players;
using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Mortality;
using JetBrains.Collections.Viewable;
using UnityEngine;

namespace Bw.UseCases.Match.Network
{
    public sealed class ScoreKeeper : IKills
    {
        private static readonly Score KillScore = new(1, 0, 0);
        private static readonly Score DeathScore = new(0, 1, 0);

        public ISource<Kill> Scored => _scored;

        private readonly INetworkTicks _ticks;
        private readonly IReadonlyMatch _match;
        private readonly IScoreboard _scoreboard;
        private readonly Combatants _combatants;
        private readonly Signal<Kill> _scored = new();
        private readonly Dictionary<IReadonlyMortal, Blow> _lastBlows = new();
        private readonly Dictionary<IReadonlyMortal, Death> _unclaimedDeaths = new();

        public ScoreKeeper(
            INetworkTicks ticks,
            IReadonlyMatch match,
            IScoreboard scoreboard,
            Combatants combatants)
        {
            _ticks = ticks;
            _match = match;
            _scoreboard = scoreboard;
            _combatants = combatants;
        }

        public void CountHit(
            IPlayer attacker,
            IReadonlyWeapon weapon,
            IReadonlyMortal victim,
            float damage) //TODO: урон сверх остатка здоровья тоже засчитывается
        {
            if (!IsCounted(attacker))
                return;

            _scoreboard.Credit(attacker, new Score(0, 0, Mathf.RoundToInt(damage)));
            _lastBlows[victim] = new Blow(attacker, weapon, _ticks.Current);
            if (_unclaimedDeaths.Remove(victim, out var death) && death.Tick == _ticks.Current)
                CreditKill(attacker, death.Victim, weapon);
        }

        public void CountDeath(IReadonlyCharacter character)
        {
            if (!_combatants.TryFindPlayer(character, out var victim) || !IsCounted(victim))
                return;

            _scoreboard.Credit(victim, DeathScore);
            if (_lastBlows.TryGetValue(character.Mortal, out var blow) && blow.Tick == _ticks.Current && IsCounted(blow.Attacker))
                CreditKill(blow.Attacker, victim, blow.Weapon);
            else
                _unclaimedDeaths[character.Mortal] = new Death(victim, _ticks.Current); //TODO: смерть без выстрела или удара в тот же тик (падение) не засчитывается никому
        }

        public void Forget(IReadonlyCharacter character)
        {
            _lastBlows.Remove(character.Mortal);
            _unclaimedDeaths.Remove(character.Mortal);
        }

        private void CreditKill(IPlayer killer, IPlayer victim, IReadonlyWeapon weapon)
        {
            _scoreboard.Credit(killer, KillScore);
            _scored.Fire(new Kill(killer, victim, weapon));
        }

        private bool IsCounted(IPlayer player) =>
            _match.Period.Value.Phase == MatchPhase.Running && _scoreboard.Scores.ContainsKey(player);

        private readonly struct Blow
        {
            public readonly IPlayer Attacker;
            public readonly IReadonlyWeapon Weapon;
            public readonly int Tick;

            public Blow(IPlayer attacker, IReadonlyWeapon weapon, int tick)
            {
                Attacker = attacker;
                Weapon = weapon;
                Tick = tick;
            }
        }

        private readonly struct Death
        {
            public readonly IPlayer Victim;
            public readonly int Tick;

            public Death(IPlayer victim, int tick)
            {
                Victim = victim;
                Tick = tick;
            }
        }
    }
}
