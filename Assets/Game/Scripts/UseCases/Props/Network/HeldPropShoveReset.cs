using Bw.Entities.Extensions;
using Bw.UseCases.Movement.Network.Prediction;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Props.Network
{
    public sealed class HeldPropShoveReset
    {
        public HeldPropShoveReset(Lifetime lifetime, IWeaponHold hold, IShoveHistory shoves)
        {
            hold.HeldLifetime.WhenAlive(lifetime, _ => shoves.Clear());
        }
    }
}
