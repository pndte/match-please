using Bw.Entities;
using Bw.Entities.Network;
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
            Container.BindInterfacesTo<PlayerCamera>().AsSingle().WithArguments(_camera).NonLazy(); // TODO: сделать только для клиента
        }
    }
}