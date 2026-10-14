using JetBrains.Collections.Viewable;
using JetBrains.Core;

namespace Bw.UseCases.Mortality.View.Death
{
    public interface IDeathViewModel
    {
        public IReadonlyProperty<bool> Dead { get; }
        public ISource<Unit> Killed { get; }
    }
}
