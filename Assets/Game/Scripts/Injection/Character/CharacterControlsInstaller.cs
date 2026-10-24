using System;
using Bw.Entities;
using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Variables;
using Bw.Entities.Simulation;
using Bw.Injection.Network.Prediction;
using Bw.UseCases.Character.Network;
using Bw.UseCases.Character.Network.Requests;
using Bw.UseCases.Combat.Hands.Network;
using Bw.UseCases.Combat.Weapon.Network;
using Zenject;

namespace Bw.Injection
{
    public sealed class CharacterControlsInstaller : Installer<IRuntimeSettings, INetEntriesSchemaBuilder, CharacterControlsInstaller>
    {
        private readonly IRuntimeSettings _runtimeSettings;
        private readonly INetEntriesSchemaBuilder _netSchema;

        public CharacterControlsInstaller(IRuntimeSettings runtimeSettings, INetEntriesSchemaBuilder netSchema)
        {
            _runtimeSettings = runtimeSettings;
            _netSchema = netSchema;
        }

        public override void InstallBindings()
        {
            Container.Bind<MovementControlsForm>().AsSingle();
            Container.Bind(typeof(CharacterTrigger), typeof(IReadonlyCharacterTrigger)).To<CharacterTrigger>().AsSingle();
            Container.Bind<CharacterTriggerRegistration>().AsSingle().NonLazy();
            Container.Bind<ISteppable<CharacterControls>>().To<CharacterSteps>().AsSingle();
            Container.Decorate<ISteppable<CharacterControls>>().With<RootedCharacterSteps>();
            Container.Bind<IInputContinuation<CharacterControls>>().To<CharacterControlsContinuation>().AsSingle();
            PredictionInputInstaller<CharacterControls>.Install(Container, _runtimeSettings, _netSchema);

            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    Container.Bind<IInputPolicy<CharacterControls>>().To<CharacterControlsPolicy>().AsSingle();
                    Container.Decorate<ISteppable<CharacterControls>>().With<GuardedCharacterSteps>();
                    Container.Bind<IInputAudience>().To<CharacterInputAudience>().AsSingle();
                    break;
                case PeerType.Client:
                    Container.Bind<CursorPoint>().AsSingle();
                    Container.Bind<GuardKey>().AsSingle();
                    Container.Bind<GuardStanceSampler>().AsSingle();
                    Container.Bind<IInputSampler<CharacterControls>>().To<CharacterControlsSampler>().AsSingle();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_runtimeSettings.CurrentPeerType), _runtimeSettings.CurrentPeerType, null);
            }
        }
    }
}
