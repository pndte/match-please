using Bw.Entities.Extensions;
using Bw.Entities.Network.Objects;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Simulation;
using Bw.UseCases.Combat.Weapon.Abstractions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Weapon.Network
{
    public sealed class WeaponClock //TODO: часы и руки шагают в одной фазе Default, и порядок их подписок задан только тем, кто создан раньше: если часы идут до рук, отдых за тик, в котором оружием не пользовались, приходит тиком позже, и на тике смены оружия пушка у сервера и у клиента может постареть на тик больше или меньше
    {
        private readonly ISteppable _weapon;

        private bool _used;

        public WeaponClock(
            Lifetime lifetime,
            INetworkLifetimedObject networkObject,
            ISteppable weapon,
            IWeaponFeed feed,
            INetworkTicks ticks)
        {
            _weapon = weapon;

            feed.Fed.Advise(lifetime, _ => _used = true);
            networkObject.SpawnedLifetime.WhenAlive(lifetime, spawnedLifetime =>
                ticks.Ticked(TickPhase.Default).Advise(spawnedLifetime, _ => Tick()));
        }

        private void Tick()
        {
            if (!_used)
                _weapon.Step();

            _used = false;
        }
    }
}
