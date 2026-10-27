using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Bots.View.Window
{
    public interface IBotsWindowViewModel
    {
        public IReadonlyProperty<bool> Available { get; }
    }
}
