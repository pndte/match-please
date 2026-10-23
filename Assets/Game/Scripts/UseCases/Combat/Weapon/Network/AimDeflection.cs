using Bw.Entities.Network.Ticks;
using Bw.UseCases.Combat.Weapon.Abstractions;

namespace Bw.UseCases.Combat.Weapon.Network
{
    public sealed class AimDeflection : IAimDeflection
    {
        public float Current => _spread.Returned(_kicked, (_ticks.Current - _kickTick) * _ticks.Duration);

        private readonly ISpread _spread;
        private readonly INetworkTicks _ticks;

        private float _kicked;
        private int _kickTick;

        public AimDeflection(ISpread spread, INetworkTicks ticks)
        {
            _spread = spread;
            _ticks = ticks;
        }

        public void Kick(float degrees)
        {
            _kicked = Current + degrees;
            _kickTick = _ticks.Current;
        }
    }
}
