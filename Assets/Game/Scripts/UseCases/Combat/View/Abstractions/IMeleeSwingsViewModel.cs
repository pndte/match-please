using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Combat.View.Abstractions
{
    public interface IMeleeSwingsViewModel
    {
        public ISource<MeleeSwing> Started { get; }
        public IReadonlyProperty<bool> Charging { get; }
        public IReadonlyProperty<bool> FullyCharged { get; }
    }
}
