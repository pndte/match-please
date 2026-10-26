using JetBrains.Lifetimes;

namespace Bw.UseCases.Mortality.Guarding
{
    public interface IReadonlyGuardCovers
    {
        public IGuardCover CoverOf(IReadonlyMortal mortal);
    }

    public interface IGuardCovers : IReadonlyGuardCovers
    {
        public void AddLifetimed(Lifetime lifetime, IReadonlyMortal mortal, IGuardCover cover);
    }
}
