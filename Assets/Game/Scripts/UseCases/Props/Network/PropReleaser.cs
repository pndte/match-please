using Bw.Entities.Extensions;
using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Props.Network
{
    public sealed class PropReleaser
    {
        public ISource<IReadonlyCharacter> Released => _released;

        private readonly Signal<IReadonlyCharacter> _released = new();
        private readonly IWeapon _prop;
        private readonly IReadonlyWeaponCarriers _carriers;
        private readonly PropLauncher _launcher;

        private IReadonlyCharacter _releaser;
        private bool _unheld;
        private Vector2 _velocity;

        public PropReleaser(
            Lifetime lifetime,
            IWeapon prop,
            IReadonlyWeaponCarriers carriers,
            PropLauncher launcher)
        {
            _prop = prop;
            _carriers = carriers;
            _launcher = launcher;

            prop.Fired.Advise(lifetime, shot => Throw(shot.Aim));
            carriers.ByWeapon.ViewForKey(lifetime, prop, (carriedLifetime, carrier) =>
            {
                _unheld = false;
                carriedLifetime.OnTermination(() =>
                {
                    if (lifetime.IsAlive)
                        Release(carrier);
                });
            });
        }

        public bool TryGetReleaser(out IReadonlyCharacter releaser)
        {
            releaser = _unheld ? _releaser : default;
            return _unheld;
        }

        private void Throw(float aim)
        {
            var carrier = _carriers.ByWeapon[_prop];
            _velocity = _launcher.ThrowVelocity(carrier, aim);
            carrier.Inventory.Drop(_prop);
        }

        private void Release(IReadonlyCharacter holder)
        {
            var velocity = _velocity;
            _velocity = Vector2.zero;
            _launcher.Launch(holder, velocity);
            _releaser = holder;
            _unheld = true;
            _released.Fire(holder);
        }
    }
}
