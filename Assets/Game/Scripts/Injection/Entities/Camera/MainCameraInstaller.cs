using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.Entities.Network;
using Bw.UseCases.Camera.View.Follow;
using Bw.UseCases.Camera.View.Shake;
using UnityEngine;
using Zenject;

namespace Bw.Injection.Entities.Camera
{
    public class MainCameraInstaller : MonoInstaller
    {
        [Inject] IRuntimeSettings _runtimeSettings;

        [SerializeField] private UnityEngine.Camera _camera;
        [SerializeField] private CameraFollowConfig _followConfig;

        public override void InstallBindings()
        {
            if (_runtimeSettings.CurrentPeerType != PeerType.Client)
                return;

            Container.BindInterfacesTo<PlayerCamera>().AsSingle().WithArguments(_camera).NonLazy();
            Container.Bind<ICameraShake>().To<CameraShake>().FromComponentOn(_camera.gameObject).AsSingle();
            Container.BindInterfacesTo<CameraTargets>().AsSingle();
            Container.Bind<CameraFollow>().AsSingle().WithArguments(_camera.gameObject.Lifetime(), _camera, _followConfig).NonLazy();
        }
    }
}