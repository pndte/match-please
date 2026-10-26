using Bw.Entities.Network.Prediction.Events;
using Bw.UseCases.Mortality.Guarding;

namespace Bw.UseCases.Mortality.Network.Guarding
{
    public sealed class AffectableGuard : IAffectable<Impacts>
    {
        private readonly IGuardBlows _guard;

        public AffectableGuard(IGuardBlows guard)
        {
            _guard = guard;
        }

        public void Affect(StateCause cause, Impacts impacts) =>
            _guard.Absorb(impacts);
    }
}
