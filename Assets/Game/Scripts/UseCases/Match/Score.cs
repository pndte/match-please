namespace Bw.UseCases.Match
{
    public readonly struct Score
    {
        public readonly int Kills;
        public readonly int Deaths;
        public readonly int Damage;

        public Score(int kills, int deaths, int damage)
        {
            Kills = kills;
            Deaths = deaths;
            Damage = damage;
        }
    }
}
