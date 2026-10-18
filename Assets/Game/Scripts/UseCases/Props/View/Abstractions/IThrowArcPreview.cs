using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Props.View.Abstractions
{
    public interface IThrowArcPreview
    {
        public void Show(Lifetime lifetime, IReadonlyProperty<ThrowArc> arc);
    }
}
