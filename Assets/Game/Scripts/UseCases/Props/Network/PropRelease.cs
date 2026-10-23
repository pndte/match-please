using Bw.Entities.Extensions;
using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Props.Network
{
    public sealed class PropRelease
    {
        private readonly IWeapon _prop;
        private readonly IWeaponCarriers _carriers;
        private readonly PropLauncher _launcher;

        private PropLaunch _launch = PropLaunch.Dropped;

        public PropRelease(
            Lifetime lifetime,
            IWeapon prop,
            IWeaponCarriers carriers,
            PropLauncher launcher)
        {
            _prop = prop;
            _carriers = carriers;
            _launcher = launcher;

            prop.Fired.Advise(lifetime, shot => Throw(shot.Aim));
            carriers.ByWeapon.ViewForKey(lifetime, prop, (carriedLifetime, carrier) =>
                carriedLifetime.OnTermination(() =>
                {
                    if (lifetime.IsAlive)
                        Release(carrier);
                }));
        }

        private void Throw(float aim)
        {
            _launch = PropLaunch.Thrown(aim);
            _carriers.ByWeapon[_prop].Inventory.Drop(_prop);
        }

        private void Release(IReadonlyCharacter holder)
        {
            var launch = _launch;
            _launch = PropLaunch.Dropped;
            launch.Switch(() => _launcher.Drop(holder), aim => _launcher.Throw(holder, aim));
        }
    }
}
