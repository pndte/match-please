using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Shooting.View.Abstractions
{
    public interface IShotsViewModel
    {
        public ISource<SeenShot> Seen { get; }
    }
}
