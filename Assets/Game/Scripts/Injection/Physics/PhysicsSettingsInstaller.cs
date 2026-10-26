using System;
using Bw.Entities.Physics;
using UnityEngine;
using Zenject;

namespace Bw.Injection.Physics
{
    [CreateAssetMenu(fileName = "PhysicsSettings", menuName = "Installers/PhysicsSettings")]
    public sealed class PhysicsSettingsInstaller : ScriptableObjectInstaller<PhysicsSettingsInstaller>
    {
        [SerializeField] private PhysicsConfig _config;

        public override void InstallBindings()
        {
            RequireConfig();
            Container.BindInstance(_config).AsSingle();
        }

        private void RequireConfig()
        {
            if (_config == null)
                throw new InvalidOperationException($"Physics settings '{name}' have no config: assign Assets/Game/Content/Physics/PhysicsConfig.asset.");

            if (_config.SubSteps < 1)
                throw new InvalidOperationException($"Physics config '{_config.name}' has {_config.SubSteps.ToString()} sub-steps: a step needs at least one.");

            if (_config.Workers < 0)
                throw new InvalidOperationException($"Physics config '{_config.name}' has {_config.Workers.ToString()} workers: zero steps on the calling thread, more use the job system.");
        }
    }
}
