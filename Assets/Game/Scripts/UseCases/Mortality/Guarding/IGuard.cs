using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Mortality.Guarding
{
    public interface IReadonlyGuard
    {
        public float Max { get; }
        public IReadonlyProperty<GuardState> State { get; }
    }

    public interface IGuard : IReadonlyGuard
    {
        public void Apply(GuardState state);
    }

    public interface IGuardCover
    {
        public bool InCover(Contact contact);
    }

    public interface IGuardBlows
    {
        public void Absorb(Impacts impacts);
    }
}
