using System;
using Bw.Entities.Extensions;
using Bw.Entities.Network.Objects;
using Bw.UseCases.Match.Extensions;
using Bw.UseCases.Match.View.Hud;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Match.View.Adapters
{
    public sealed class MatchViewModel : IMatchViewModel
    {
        public IReadonlyProperty<bool> Shown { get; }
        public IReadonlyProperty<ScoreSheet> Sheet => _sheet;
        public int SecondsLeft => _countdown.SecondsLeft;

        private readonly MatchCountdown _countdown;
        private readonly ViewableProperty<ScoreSheet> _sheet = new(ScoreSheet.None);

        public MatchViewModel(
            Lifetime lifetime,
            INetworkLifetimedObject networkLifetimed,
            IReadonlyMatchBoard board,
            MatchCountdown countdown)
        {
            _countdown = countdown;
            Shown = networkLifetimed.SpawnedLifetime.Select(lifetime, spawnedLifetime => spawnedLifetime.IsAlive);

            board.Current.Advise(lifetime, standings => _sheet.Value = SheetOf(standings));
        }

        private static ScoreSheet SheetOf(MatchStandings standings)
        {
            var lines = new ScoreLine[standings.Lines.Count];
            for (var index = 0; index < lines.Length; index++)
                lines[index] = LineOf(standings.Lines[index]);

            return new ScoreSheet(StageOf(standings.Period.Phase), lines);
        }

        private static ScoreLine LineOf(Standing standing) =>
            new(standing.Nickname, standing.Figure, StatsOf(standing.Score), standing.IsMine);

        private static ScoreStats StatsOf(Score score)
        {
            var damagePerKill = score.TryGetDamagePerKill(out var perKill) ? perKill : 0;
            return new ScoreStats(score.Kills, score.Deaths, score.Damage, score.KillsPerDeath(), damagePerKill);
        }

        private static MatchStage StageOf(MatchPhase phase) =>
            phase switch
            {
                MatchPhase.Starting => MatchStage.Starting,
                MatchPhase.Running => MatchStage.Running,
                MatchPhase.Ended => MatchStage.Ended,
                _ => throw new ArgumentOutOfRangeException(nameof(phase), phase, null)
            };
    }
}
