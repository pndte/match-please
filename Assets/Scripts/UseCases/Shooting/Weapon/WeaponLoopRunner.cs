using Bw.UseCases.Shooting.Weapon.Triggers;
using Bw.Entities.Loop;
using JetBrains.Lifetimes;
using Setup;
using UnityEngine;
using Zenject;

namespace Bw.UseCases.Shooting.Weapon
{
    public class ShootingWeaponLoopRunner : MonoBehaviour
    {
        [Inject] private IRuntimeSettings _settings;
        [InjectOptional] private KeyboardWeaponTriggersConnector _keyboardWeaponTriggersConnector; //TODO: уродливо

        private Sequence _enabledUpdateSequence;
        private Sequence _enabledAndServerUpdateSequence;

        private IReadonlyControlledBy _controlledBy;

        [Inject]
        private void Construct(
            Lifetime lifetime,
            ShootingWeapon shootingWeapon,
            IReadonlyControlledBy controlledBy)
        {
            _enabledAndServerUpdateSequence = new Sequence(shootingWeapon);
            _controlledBy = controlledBy;
            _enabledUpdateSequence = _keyboardWeaponTriggersConnector == null
                ? new Sequence()
                : new Sequence(_keyboardWeaponTriggersConnector);
        }

        private void Update()
        {
            Debug.Log($"{gameObject.name}: controlled by me: {_controlledBy.Me.Value}");
            if (_controlledBy.Me.Value)
                _enabledUpdateSequence.Update();
            if (_settings.CurrentPeerType == PeerType.Server
                || _controlledBy.Me.Value)
                _enabledAndServerUpdateSequence.Update();
        }
    }
}