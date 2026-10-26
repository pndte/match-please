using Bw.UseCases.Mortality.Guarding;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Mortality.Network.Guarding
{
    public sealed class GuardCoverRegistrar
    {
        public GuardCoverRegistrar(
            Lifetime lifetime,
            IReadonlyMortal mortal,
            IGuardCover cover,
            IGuardCovers covers) =>
            covers.AddLifetimed(lifetime, mortal, cover);
    }
}
