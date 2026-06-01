using Bw.UseCases.Shooting.Weapon.Abstractions;
using Bw.UseCases.Shooting.Weapon.Extensions;
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
            IReloadRequestResult reloadRequestResult,
            IWeapon weapon,
            IReloader reloader,
            IReadonlyAmmo ammo)
        {
            var reloadSequentialLifetimes = new SequentialLifetimes(lifetime);

            mouseShootRequest.Requested.Advise(lifetime, shootRequestDto =>
            {
                var accepted = weapon.CanShoot.Value
                               && !ammo.Empty()
                               && reloader.State.Value != ReloadState.Reloading;

                shootRequestResult.Received.Fire(
                    new ShootRequestResultDto(shootRequestDto.RequestId, shootRequestDto.TargetPosition, accepted));

                if (!accepted)
                    return;

                weapon.Shoot(shootRequestDto.TargetPosition);
            });

            reloadRequest.Requested.Advise(lifetime, reloadRequestDto =>
            {
                var accepted = reloader.CanReload.Value;

                reloadRequestResult.Received.Fire(
                    new ReloadRequestResultDto(reloadRequestDto.RequestId, accepted));

                if (!accepted)
                    return;

                reloader.Reload(reloadSequentialLifetimes.Next()).Forget();
            });
        }
    }
}
