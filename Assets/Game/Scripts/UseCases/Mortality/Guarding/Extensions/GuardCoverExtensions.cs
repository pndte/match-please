namespace Bw.UseCases.Mortality.Guarding.Extensions
{
    public static class GuardCoverExtensions
    {
        public static Impacts Uncovered(this IGuardCover cover, Impacts impacts)
        {
            var uncovered = 0;
            foreach (var impact in impacts)
                if (!cover.InCover(impact.Contact))
                    uncovered++;

            if (uncovered == impacts.Count)
                return impacts;

            var kept = new Impact[uncovered];
            var index = 0;
            foreach (var impact in impacts)
                if (!cover.InCover(impact.Contact))
                    kept[index++] = impact;

            return new Impacts(kept);
        }

        public static float CoveredDamage(this IGuardCover cover, Impacts impacts)
        {
            var damage = 0f;
            foreach (var impact in impacts)
                if (cover.InCover(impact.Contact))
                    damage += impact.Hit.Damage;

            return damage;
        }
    }
}
