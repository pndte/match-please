using JetBrains.Collections.Viewable;
using JetBrains.Core;

namespace Bw.UseCases.Mortality.View.Hit
{
    public interface IDamageViewModel
    {
        public ISource<Unit> Taken { get; }
    }
}
