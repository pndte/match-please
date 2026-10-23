using Bw.UseCases.Combat.Weapon.Abstractions;

namespace Bw.UseCases.Combat.Weapon.Network
{
    public sealed class SteadyAim : IReadonlyAimDeflection
    {
        public float Current => 0f;
    }
}
