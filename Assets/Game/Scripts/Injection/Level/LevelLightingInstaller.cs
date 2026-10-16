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
        [SerializeField] private LampLight _light;
        [SerializeField] private LampHaze _haze;

        public override void InstallBindings()
        {
            RequireLamps();

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
            var lightLifetime = _light.gameObject.Lifetime();
            Container.BindInterfacesTo<LampLightViewModel>().AsSingle().WithArguments(lightLifetime);
            Container.Bind<LampLightView>().AsSingle().WithArguments(lightLifetime, _light).NonLazy();

            var hazeLifetime = _haze.gameObject.Lifetime();
            Container.BindInterfacesTo<LampHazeViewModel>().AsSingle().WithArguments(hazeLifetime);
            Container.Bind<LampHazeView>().AsSingle().WithArguments(hazeLifetime, _haze).NonLazy();
        }

        private void RequireLamps()
        {
            if (_light == null || _haze == null)
                throw new InvalidOperationException(
                    $"Level lighting of '{gameObject.scene.name}' needs its LampLight and LampHaze: they swap the lamps' live light and volumetric haze for the baked ones.");
        }
    }
}
