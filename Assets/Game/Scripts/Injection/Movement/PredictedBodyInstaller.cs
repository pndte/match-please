using System;
using Bw.Entities.Network;
using Bw.Entities.Network.Variables;
using Bw.Injection.Network;
using Bw.UseCases.Movement;
using Bw.UseCases.Movement.Physics;
using Zenject;

namespace Bw.Injection.Movement
{
    public class PredictedBodyInstaller : Installer<IRuntimeSettings, INetEntriesSchemaBuilder, DynamicBodyConfig, PredictedBodyInstaller>
    {
        private readonly IRuntimeSettings _runtimeSettings;
        private readonly INetEntriesSchemaBuilder _netSchema;
        private readonly DynamicBodyConfig _config;

        public PredictedBodyInstaller(
            IRuntimeSettings runtimeSettings,
            INetEntriesSchemaBuilder netSchema,
            DynamicBodyConfig config)
        {
            _runtimeSettings = runtimeSettings;
            _netSchema = netSchema;
            _config = config;
        }

        public override void InstallBindings()
        {
            Container.Bind<ShovableRegistration>().AsSingle().NonLazy();
            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    BindServer();
                    break;
                case PeerType.Client:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            DynamicBodyInstaller.Install(Container, _runtimeSettings, _netSchema, _config);
        }

        private void BindServer()
        {
            LagCompensationInstaller<RigidState, RigidPoseView>.Install(Container, _runtimeSettings);
        }
    }
}
