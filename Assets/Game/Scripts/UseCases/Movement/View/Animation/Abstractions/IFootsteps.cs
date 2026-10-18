using JetBrains.Collections.Viewable;
using JetBrains.Core;

namespace Bw.UseCases.Movement.View.Animation.Abstractions
{
    public interface IFootsteps
    {
        public ISource<Unit> Stepped { get; }
    }
}
