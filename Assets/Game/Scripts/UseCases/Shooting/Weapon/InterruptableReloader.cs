using System;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using Cysharp.Threading.Tasks;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.Weapon
{
    public class InterruptableReloader : IReloader
    {
        public IReadonlyProperty<ReloadState> State => _reloadingState;
        public IReadonlyProperty<bool> CanReload => _canReload;
        
        private readonly IViewableProperty<ReloadState> _reloadingState;
        private readonly IViewableProperty<bool> _canReload;
        
        private readonly ShootingWeaponConfig _config;

        public InterruptableReloader(IViewableProperty<ReloadState> reloadingState, ShootingWeaponConfig config)
        {
            _reloadingState = reloadingState;
            _config = config;
            _canReload = new ViewableProperty<bool>(true);
        }

        public async UniTaskVoid Reload(Lifetime lifetime)
        {
            _reloadingState.Value = ReloadState.Reloading; 
            
            var cancelled = await UniTask.Delay(TimeSpan.FromSeconds(_config.ReloadTime), cancellationToken: lifetime).SuppressCancellationThrow();
            if (cancelled)
            {
                _reloadingState.Value = ReloadState.Interrupted;
                return;
            }
            
            _reloadingState.Value = ReloadState.Complete;
        }
    }
}