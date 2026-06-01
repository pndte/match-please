using System;
using Bw.Entities.Network.Repository;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using Bw.UseCases.Shooting.Weapon.Extensions;
using Bw.UseCases.Shooting.Weapon.Network.Requests;
using Cysharp.Threading.Tasks;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.Weapon
{
    public sealed class InterruptableReloader : IReloader
    {
        public IReadonlyProperty<ReloadState> State => _reloadingState;
        public IReadonlyProperty<bool> CanReload => _canReload;

        private readonly IViewableProperty<ReloadState> _reloadingState;
        private readonly IViewableProperty<bool> _canReload;
        private readonly IReadonlyAmmo _ammo;
        private readonly ShootingWeaponConfig _config;

        public InterruptableReloader(
            Lifetime lifetime,
            IViewableProperty<ReloadState> reloadingState,
            ShootingWeaponConfig config,
            IReadonlyAmmo ammo)
        {
            _reloadingState = reloadingState;
            _config = config;
            _ammo = ammo;
            _canReload = new ViewableProperty<bool>(true);

            RefreshCanReload();
            _reloadingState.Advise(lifetime, _ => RefreshCanReload());
            _ammo.Advise(lifetime, _ => RefreshCanReload());
        }

        public async UniTaskVoid Reload(Lifetime lifetime)
        {
            _reloadingState.Value = ReloadState.Reloading;
            RefreshCanReload();

            var cancelled = await UniTask
                .Delay(TimeSpan.FromSeconds(_config.ReloadTime), cancellationToken: lifetime)
                .SuppressCancellationThrow();
            if (cancelled)
            {
                _reloadingState.Value = ReloadState.Interrupted;
                RefreshCanReload();
                return;
            }

            _reloadingState.Value = ReloadState.Complete;
            RefreshCanReload();
        }

        private void RefreshCanReload() =>
            _canReload.Value = _reloadingState.Value != ReloadState.Reloading && !_ammo.Full();

        public sealed class NetworkHandler
        {
            public NetworkHandler(
                Lifetime lifetime,
                InterruptableReloader reloader,
                IReloadRequestResult reloadRequestResult,
                IRequestIdsRepository requestIdsRepository,
                PendingReloadLifetimes pendingReloadLifetimes)
            {
                var reloadLifetimes = new SequentialLifetimes(lifetime);

                reloadRequestResult.Received.Advise(lifetime, result =>
                {
                    var wasPredicted = requestIdsRepository.TryRemoveIdFor<ReloadRequestDto>(result.RequestId);

                    if (wasPredicted)
                    {
                        pendingReloadLifetimes.TryRelease(result.RequestId, cancel: !result.Accepted);
                        return;
                    }

                    if (result.Accepted)
                        reloader.Reload(reloadLifetimes.Next()).Forget();
                });
            }
        }
    }
}
