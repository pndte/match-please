using Bw.UseCases.Props.View.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Props.View.Aim
{
    public sealed class ThrowArcRegistration
    {
        public ThrowArcRegistration(Lifetime lifetime, IThrowArcViewModel arcs, IThrowArcPreview preview)
        {
            arcs.Aiming.WhenTrue(lifetime, aimingLifetime => preview.Show(aimingLifetime, arcs.Arc));
        }
    }
}
