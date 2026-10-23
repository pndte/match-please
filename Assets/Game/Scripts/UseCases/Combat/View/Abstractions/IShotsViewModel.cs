using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Combat.View.Abstractions
{
    public interface IShotsViewModel
    {
        public ISource<SeenShot> Seen { get; }
    }
}
