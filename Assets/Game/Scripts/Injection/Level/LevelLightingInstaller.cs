using System;
using Bw.Entities.Extensions;
using Bw.Entities.Network;
using Bw.UseCases.Level.View.Adapters;
using Bw.UseCases.Level.View.Lighting;
using UnityEngine;
using Zenject;

namespace Bw.Injection.Level
{
    public sealed class LevelLightingInstaller : MonoInstaller
    {
        [Inject] private IRuntimeSettings _runtimeSettings;

        [Header("Lighting")]
        [SerializeField] private LampHaze _haze;

        public override void InstallBindings()
        {
            RequireHaze();

            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    break;
                case PeerType.Client:
                    BindLightingVisuals();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_runtimeSettings.CurrentPeerType), _runtimeSettings.CurrentPeerType, null);
            }
        }

        private void BindLightingVisuals()
        {
            var hazeLifetime = _haze.gameObject.Lifetime();
            Container.BindInterfacesTo<LampHazeViewModel>().AsSingle().WithArguments(hazeLifetime);
            Container.Bind<LampHazeView>().AsSingle().WithArguments(hazeLifetime, _haze).NonLazy();
        }

        private void RequireHaze()
        {
            if (_haze == null)
                throw new InvalidOperationException(
                    $"Level lighting of '{gameObject.scene.name}' has no lamp haze: set its LampHaze, which swaps the lamps' live volumetric haze for the baked one.");
        }
    }
}
