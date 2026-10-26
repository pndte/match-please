using Bw.UseCases.Mortality.Guarding;

namespace Bw.Tests.UseCases.Mortality.Doubles
{
    public static class GuardStateOf
    {
        public static GuardState Shown(float health, bool raised, bool broken) =>
            new(health, raised, broken, 0f, 0, 0);

        public static (float Health, bool Raised, bool Broken) ShownPart(GuardState state) =>
            (state.Health, state.Raised, state.Broken);
    }
}
