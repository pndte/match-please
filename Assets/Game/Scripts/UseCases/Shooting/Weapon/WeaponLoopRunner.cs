using Bw.Entities.Loop;
using Bw.Entities.Network;
using Bw.UseCases.Shooting.Weapon.Network;
using JetBrains.Lifetimes;
using UnityEngine;
using Zenject;

namespace Bw.UseCases.Shooting.Weapon
{
    public class ShootingWeaponLoopRunner : MonoBehaviour //TODO: delete
    {
        [Inject] private IRuntimeSettings _settings;

        private Sequence _enabledUpdateSequence;
        private Sequence _enabledAndServerUpdateSequence;

        private IReadonlyControlledBy _controlledBy;

        [Inject]
        private void Construct(
            Lifetime lifetime,
            ShootingWeapon shootingWeapon,
            IReadonlyControlledBy controlledBy)
        {
            _enabledUpdateSequence = new Sequence(shootingWeapon);
            _enabledAndServerUpdateSequence = _enabledUpdateSequence;
            _controlledBy = controlledBy;
        }

        private void Update()
        {
            if (_controlledBy == null || _settings == null || _enabledUpdateSequence == null ||
                _enabledAndServerUpdateSequence == null)
                return;

            if (_controlledBy.Me.Value)
                _enabledUpdateSequence.Update();
            if (_settings.CurrentPeerType == PeerType.Server
                || _controlledBy.Me.Value)
                _enabledAndServerUpdateSequence.Update();
        }
    }
}