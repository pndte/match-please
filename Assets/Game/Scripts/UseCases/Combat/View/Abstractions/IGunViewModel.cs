using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Combat.View.Abstractions
{
    public interface IGunViewModel
    {
        public int Capacity { get; }
        public IReadonlyProperty<int> Rounds { get; }
        public IReadonlyProperty<GunReload> Reload { get; }
    }
}
