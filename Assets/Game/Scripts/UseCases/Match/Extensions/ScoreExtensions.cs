using System;
using UnityEngine;

namespace Bw.UseCases.Match.Extensions
{
    public static class ScoreExtensions
    {
        public static Score Plus(this Score score, Score gained) =>
            new(score.Kills + gained.Kills, score.Deaths + gained.Deaths, score.Damage + gained.Damage);

        public static float KillsPerDeath(this Score score) =>
            score.Kills / (float)Math.Max(score.Deaths, 1);

        public static bool TryGetDamagePerKill(this Score score, out int damagePerKill)
        {
            damagePerKill = score.Kills == 0 ? 0 : Mathf.RoundToInt(score.Damage / (float)score.Kills);
            return score.Kills > 0;
        }
    }
}
