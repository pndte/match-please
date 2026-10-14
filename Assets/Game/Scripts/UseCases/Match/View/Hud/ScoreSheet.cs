using System;
using System.Collections.Generic;

namespace Bw.UseCases.Match.View.Hud
{
    public sealed class ScoreSheet
    {
        public static readonly ScoreSheet None = new(MatchStage.Starting, Array.Empty<ScoreLine>());

        public readonly MatchStage Stage;
        public readonly IReadOnlyList<ScoreLine> Lines;

        public ScoreSheet(MatchStage stage, IReadOnlyList<ScoreLine> lines)
        {
            Stage = stage;
            Lines = lines;
        }
    }
}
