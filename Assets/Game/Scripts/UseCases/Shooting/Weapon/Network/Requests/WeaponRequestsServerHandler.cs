using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.Weapon.Network.Requests
{
    public class WeaponRequestsServerHandler
    {
        public WeaponRequestsServerHandler(
            Lifetime lifetime,
            IMouseShootRequest mouseShootRequest,
            IShootRequestResult shootRequestResult,
            IReloadRequest reloadRequest,
            IWeapon weapon,
            IReloader reloader)
        {
            var reloadSequentialLifetimes = new SequentialLifetimes(lifetime);
            weapon.CanShoot.WhenTrue(lifetime,
                canShootLifetime => mouseShootRequest.Requested.Advise(canShootLifetime, shootRequestDto =>
                {
                    shootRequestResult.Received.Fire(shootRequestDto);
                    weapon.Shoot(shootRequestDto.TargetPosition);
                }));
            
            reloader.CanReload.WhenTrue(lifetime, canReloadLifetime => 
                reloadRequest.Requested.Advise(canReloadLifetime, _ => reloader.Reload(reloadSequentialLifetimes.Next())));
        }
    }
}