using System;
using System.Collections.Generic;
using Bw.Entities.Players;
using Bw.UseCases.Match.Extensions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Match
{
    public interface IReadonlyScoreboard
    {
        public IReadOnlyDictionary<IPlayer, Score> Scores { get; }
        public ISource<IPlayer> ScoreChanged { get; }
    }

    public interface IScoreboard : IReadonlyScoreboard
    {
        public void Credit(IPlayer player, Score gained);
        public void Reset();
    }

    public sealed class Scoreboard : IScoreboard
    {
        public IReadOnlyDictionary<IPlayer, Score> Scores => _scores;
        public ISource<IPlayer> ScoreChanged => _scoreChanged;

        private readonly Dictionary<IPlayer, Score> _scores = new();
        private readonly List<IPlayer> _resetting = new();
        private readonly Signal<IPlayer> _scoreChanged = new();

        public Scoreboard(Lifetime lifetime, IPlayerCollection players)
        {
            players.View(lifetime, (playerLifetime, player) => Enter(playerLifetime, player));
        }

        public void Credit(IPlayer player, Score gained)
        {
            if (!_scores.TryGetValue(player, out var score))
                throw new InvalidOperationException($"Player {player.Nickname} is not on the scoreboard: only players in the game score.");

            _scores[player] = score.Plus(gained);
            _scoreChanged.Fire(player);
        }

        public void Reset()
        {
            _resetting.Clear();
            _resetting.AddRange(_scores.Keys);
            foreach (var player in _resetting)
            {
                _scores[player] = default;
                _scoreChanged.Fire(player);
            }
        }

        private void Enter(Lifetime playerLifetime, IPlayer player)
        {
            _scores.Add(player, default);
            _scoreChanged.Fire(player);
            playerLifetime.OnTermination(() => Leave(player));
        }

        private void Leave(IPlayer player)
        {
            _scores.Remove(player);
            _scoreChanged.Fire(player);
        }
    }
}
