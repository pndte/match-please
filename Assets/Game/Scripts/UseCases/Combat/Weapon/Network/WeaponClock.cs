using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Simulation;
using Bw.UseCases.Combat.Weapon.Abstractions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Weapon.Network
{
    public sealed class WeaponClock //TODO: на клиенте часы гасят и оружие, которое клиент не предсказывает: его симуляция зря отдыхает каждый тик, пока вид показывает снимки сервера; гасить только предсказанное
    {
        private readonly ISteppable _weapon;

        private bool _used;

        public WeaponClock(
            Lifetime lifetime,
            ISteppable weapon,
            IWeaponFeed feed,
            IWorldStep worldStep)
        {
            _weapon = weapon;

            feed.Fed.Advise(lifetime, _ => _used = true);
            worldStep.Stepped(TickPhase.Late).Advise(lifetime, _ => Tick());
        }

        private void Tick()
        {
            if (!_used)
                _weapon.Step();

            _used = false;
        }
    }
}
