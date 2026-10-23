using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Combat.View.Highlight
{
    public interface IHighlightViewModel
    {
        public IReadonlyProperty<bool> Highlighted { get; }
    }
}
