using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Shooting.View.Abstractions
{
    public interface ISeenShots
    {
        public ISource<float> Seen { get; }
    }
}
