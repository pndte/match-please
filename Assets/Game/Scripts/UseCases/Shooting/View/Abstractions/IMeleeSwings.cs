using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Shooting.View.Abstractions
{
    public interface IMeleeSwings
    {
        public ISource<MeleeSwing> Started { get; }
        public IReadonlyProperty<bool> Charging { get; }
        public IReadonlyProperty<bool> FullyCharged { get; }
    }
}
