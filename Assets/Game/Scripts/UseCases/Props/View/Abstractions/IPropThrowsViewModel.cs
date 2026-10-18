using JetBrains.Collections.Viewable;
using JetBrains.Core;

namespace Bw.UseCases.Props.View.Abstractions
{
    public interface IPropThrowsViewModel
    {
        public ISource<Unit> WindUpStarted { get; }
        public ISource<PropThrow> Thrown { get; }
    }
}
