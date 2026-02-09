using Bw.Entities;
using Setup;
using UnityEngine;
using Zenject;

namespace Bw.Injection.Entities.Camera
{
    public class MainCameraInstaller : MonoInstaller
    {
        [Inject] IRuntimeSettings _settings;
        
        [SerializeField] private UnityEngine.Camera _camera;
        public override void InstallBindings()
        {
            if (_settings.CurrentPeerType != PeerType.Client) return;
            
            Container.BindInterfacesAndSelfTo<PlayerCamera>().AsSingle().WithArguments(_camera);
        }
    }
}