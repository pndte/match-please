using Bw.Entities.Network;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using Unity.Netcode;
using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon.Triggers
{
    public class ShootingWeaponTriggers : NetworkLifetimedBehaviour, IMouseShootTrigger, IReloadTrigger // TODO: декомпозировать.
    {
        ISignal<Vector3> IMouseShootTrigger.Triggered => _shotTriggered;
        ISignal<Lifetime> IReloadTrigger.Triggered => _reloadTriggered;
        
        private readonly ISignal<Vector3> _shotTriggered = new Signal<Vector3>();
        private readonly ISignal<Lifetime> _reloadTriggered = new Signal<Lifetime>();
        private SequentialLifetimes _reloadLifetimes = new(Lifetime.Terminated);

        private void Start()
        {
            _reloadLifetimes = new SequentialLifetimes(AliveLifetime.Value);
            if (IsServer || !IsOwner) return;
            
            WhenAlive(OnAlive);
        }

        private void OnAlive(Lifetime lifetime)
        {
            _reloadTriggered.Advise(lifetime, _ => TriggerReloadServerRpc());
            _shotTriggered.Advise(lifetime, mousePosition =>
            {
                TriggerShotServerRpc(mousePosition);
            });
        }

        [Rpc(SendTo.Server)]
        private void TriggerShotServerRpc(Vector3 mousePosition, RpcParams rpcParams = default)
        {
            _shotTriggered.Fire(mousePosition);
            var target = RpcTarget.Not(rpcParams.Receive.SenderClientId, RpcTargetUse.Temp);
            TriggerShotClientRpc(mousePosition, new RpcParams {Send = new RpcSendParams {Target = target}});
        }
        
        [Rpc(SendTo.SpecifiedInParams)]
        private void TriggerShotClientRpc(Vector3 mousePosition, RpcParams rpcParams = default)
        {
            _shotTriggered.Fire(mousePosition);
        }
        
        [Rpc(SendTo.Server)]
        private void TriggerReloadServerRpc(RpcParams rpcParams = default)
        {
            _reloadTriggered.Fire(_reloadLifetimes.Next());
            var target = RpcTarget.Not(rpcParams.Receive.SenderClientId, RpcTargetUse.Temp);
            TriggerReloadClientRpc(new RpcParams {Send = new RpcSendParams {Target = target}});
        }
        
        [Rpc(SendTo.SpecifiedInParams)]
        private void TriggerReloadClientRpc(RpcParams rpcParams = default)
        {
            _reloadTriggered.Fire(_reloadLifetimes.Next());
        }
    }
}