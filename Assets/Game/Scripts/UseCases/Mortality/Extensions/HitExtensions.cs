namespace Bw.UseCases.Mortality.Extensions
{
    public static class HitExtensions
    {
        public static Hit Plus(this Hit hit, Hit other) =>
            new(hit.Damage + other.Damage, hit.Knockback + other.Knockback);
    }
}
