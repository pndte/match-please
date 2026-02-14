using Bw.Entities.Network;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Bw.UseCases.Shooting.Weapon.Triggers.Network
{
    public class ShootingWeaponTriggers : NetworkLifetimedBehaviour, IMouseShootTrigger, IReloadTrigger // TODO: декомпозировать, избавиться от RPC и NetworkLifetimedBehaviour.
    {
        ISignal<Vector3> IMouseShootTrigger.Triggered => _shotTriggered;
        ISignal<Lifetime> IReloadTrigger.Triggered => _reloadTriggered;
        
        private readonly ISignal<Vector3> _shotTriggered = new Signal<Vector3>();
        private readonly ISignal<Lifetime> _reloadTriggered = new Signal<Lifetime>();
        private SequentialLifetimes _reloadLifetimes = new(Lifetime.Terminated);

        [Inject]
        private void Construct(Lifetime lifetime, IReadonlyControlledBy enabled)
        {
            _reloadLifetimes = new SequentialLifetimes(lifetime);
            enabled.Me.WhenTrue(lifetime, OnAlive);
        }

        private void OnAlive(Lifetime lifetime)
        {
            if (!IsOwner) return; 
            
            Debug.Log("Advised");
            _reloadTriggered.Advise(lifetime, _ => TriggerReloadServerRpc());
            _shotTriggered.Advise(lifetime, mousePosition =>
            {
                TriggerShotServerRpc(mousePosition);
            });
        }

        [Rpc(SendTo.Server)]
        private void TriggerShotServerRpc(Vector3 mousePosition, RpcParams rpcParams = default)
        {
            Debug.Log($"{nameof(TriggerShotServerRpc)} called");
            _shotTriggered.Fire(mousePosition);
            var target = RpcTarget.Not(rpcParams.Receive.SenderClientId, RpcTargetUse.Temp);
            TriggerShotClientRpc(mousePosition, new RpcParams {Send = new RpcSendParams {Target = target}});
        }
        
        [Rpc(SendTo.SpecifiedInParams)]
        private void TriggerShotClientRpc(Vector3 mousePosition, RpcParams rpcParams = default)
        {
            _shotTriggered.Fire(mousePosition); //TODO: это не работает, надо вызывать рпс на weapon.Shot
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