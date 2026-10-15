using UnityEngine;

namespace Bw.UseCases.Match.View.Hud
{
    public readonly struct ScoreLine
    {
        public readonly string Nickname;
        public readonly Sprite Portrait;
        public readonly ScoreStats Stats;
        public readonly bool Mine;

        public ScoreLine(
            string nickname,
            Sprite portrait,
            ScoreStats stats,
            bool mine)
        {
            Nickname = nickname;
            Portrait = portrait;
            Stats = stats;
            Mine = mine;
        }
    }
}
