using Bw.UseCases.Combat.View.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.View
{
    public sealed class FlashlightView
    {
        public FlashlightView(
            Lifetime lifetime,
            IFlashlightViewModel flashlight,
            IWeaponCarryViewModel carry,
            IFlashlightBeam beam) =>
            carry.Drawn.WhenTrue(lifetime, drawnLifetime =>
                flashlight.Lit.WhenTrue(drawnLifetime, litLifetime =>
                {
                    beam.Show();
                    litLifetime.OnTermination(beam.Hide);
                }));
    }
}
