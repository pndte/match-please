namespace Bw.UseCases.Match.View.Hud
{
    public readonly struct ScoreStats
    {
        public readonly int Kills;
        public readonly int Deaths;
        public readonly int Damage;
        public readonly float KillsPerDeath;
        public readonly int DamagePerKill;

        public ScoreStats(
            int kills,
            int deaths,
            int damage,
            float killsPerDeath,
            int damagePerKill)
        {
            Kills = kills;
            Deaths = deaths;
            Damage = damage;
            KillsPerDeath = killsPerDeath;
            DamagePerKill = damagePerKill;
        }
    }
}
