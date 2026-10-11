namespace Bw.UseCases.Shooting.Weapon.Extensions
{
    public static class PelletsConfigExtensions
    {
        public static float AngleOf(this PelletsConfig pellets, int pellet) =>
            pellets.Count == 1 ? 0f : pellets.Cone * ((float)pellet / (pellets.Count - 1) - 0.5f);
    }
}
