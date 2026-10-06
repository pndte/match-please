using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.View.Crosshair.Abstractions
{
    public interface IAimCursor
    {
        public void ShowReload(Lifetime reloadLifetime, IReloadTimer timer);
        public void ShowShot();
    }
}
