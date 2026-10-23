using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.View.Crosshair.Abstractions
{
    public interface IAimCursor
    {
        public void ShowReload(Lifetime reloadLifetime, IReloadTimer timer);
        public void ShowShot();
        public void ShowRounds(Lifetime shownLifetime, IReadonlyProperty<int> rounds, int capacity);
        public void ShowFists(Lifetime drawnLifetime);
    }
}
