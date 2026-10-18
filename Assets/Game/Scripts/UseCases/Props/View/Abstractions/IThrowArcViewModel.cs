using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Props.View.Abstractions
{
    public interface IThrowArcViewModel
    {
        public IReadonlyProperty<bool> Aiming { get; }
        public IReadonlyProperty<ThrowArc> Arc { get; }
    }
}
