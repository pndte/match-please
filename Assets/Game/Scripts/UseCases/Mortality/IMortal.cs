using Bw.Entities;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Mortality
{
    public interface IMortal : IReadonlyMortal, IAppliedState<Vitals>
    {
    }

    public interface IReadonlyMortal : IReadonlyAppliedState<Vitals>
    {
        public IReadonlyHealth Health { get; }
        public IReadonlyProperty<LifeState> State { get; }
    }
}
