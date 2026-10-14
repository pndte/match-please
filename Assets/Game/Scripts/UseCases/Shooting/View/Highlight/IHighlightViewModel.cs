using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Shooting.View.Highlight
{
    public interface IHighlightViewModel
    {
        public IReadonlyProperty<bool> Highlighted { get; }
    }
}
