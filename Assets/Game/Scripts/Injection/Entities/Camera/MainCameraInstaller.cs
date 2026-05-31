using Bw.Entities;
using Bw.Entities.Network;
using UnityEngine;
using Zenject;

namespace Bw.Injection.Entities.Camera
{
    public class MainCameraInstaller : MonoInstaller
    {
        [Inject] IRuntimeSettings _runtimeSettings;

        [SerializeField] private UnityEngine.Camera _camera;

        public override void InstallBindings()
        {
            if (_runtimeSettings.CurrentPeerType == PeerType.Client)
                Container.BindInterfacesTo<PlayerCamera>().AsSingle().WithArguments(_camera).NonLazy(); 
        }
    }
}