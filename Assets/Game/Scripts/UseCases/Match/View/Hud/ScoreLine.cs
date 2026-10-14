namespace Bw.UseCases.Match.View.Hud
{
    public readonly struct ScoreLine
    {
        public readonly string Nickname;
        public readonly string Figure;
        public readonly ScoreStats Stats;
        public readonly bool Mine;

        public ScoreLine(
            string nickname,
            string figure,
            ScoreStats stats,
            bool mine)
        {
            Nickname = nickname;
            Figure = figure;
            Stats = stats;
            Mine = mine;
        }
    }
}
