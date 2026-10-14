using System.Collections.Generic;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Players;
using Bw.UseCases.Mortality;
using UnityEngine;

namespace Bw.UseCases.Match.Network
{
    public sealed class ScoreKeeper
    {
        private static readonly Score Kill = new(1, 0, 0);
        private static readonly Score Death = new(0, 1, 0);

        private readonly INetworkTicks _ticks;
        private readonly IReadonlyMatch _match;
        private readonly IScoreboard _scoreboard;
        private readonly Dictionary<IReadonlyMortal, Blow> _lastBlows = new();
        private readonly Dictionary<IReadonlyMortal, int> _unclaimedDeaths = new();

        public ScoreKeeper(INetworkTicks ticks, IReadonlyMatch match, IScoreboard scoreboard)
        {
            _ticks = ticks;
            _match = match;
            _scoreboard = scoreboard;
        }

        public void CountHit(IPlayer attacker, IReadonlyMortal victim, float damage) //TODO: урон сверх остатка здоровья тоже засчитывается
        {
            if (!IsCounted(attacker))
                return;

            _scoreboard.Credit(attacker, new Score(0, 0, Mathf.RoundToInt(damage)));
            _lastBlows[victim] = new Blow(attacker, _ticks.Current);
            if (_unclaimedDeaths.Remove(victim, out var deathTick) && deathTick == _ticks.Current)
                _scoreboard.Credit(attacker, Kill);
        }

        public void CountDeath(IReadonlyMortal character, IPlayer victim)
        {
            if (!IsCounted(victim))
                return;

            _scoreboard.Credit(victim, Death);
            if (_lastBlows.TryGetValue(character, out var blow) && blow.Tick == _ticks.Current && IsCounted(blow.Attacker))
                _scoreboard.Credit(blow.Attacker, Kill);
            else
                _unclaimedDeaths[character] = _ticks.Current; //TODO: смерть без выстрела или удара в тот же тик (падение) не засчитывается никому
        }

        public void Forget(IReadonlyMortal character)
        {
            _lastBlows.Remove(character);
            _unclaimedDeaths.Remove(character);
        }

        private bool IsCounted(IPlayer player) =>
            _match.Period.Value.Phase == MatchPhase.Running && _scoreboard.Scores.ContainsKey(player);

        private readonly struct Blow
        {
            public readonly IPlayer Attacker;
            public readonly int Tick;

            public Blow(IPlayer attacker, int tick)
            {
                Attacker = attacker;
                Tick = tick;
            }
        }
    }
}
