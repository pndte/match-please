using System;
using Bw.Entities.Network;
using Bw.Entities.Network.LagCompensation;
using Bw.Entities.Simulation;
using Bw.Injection.Network;
using Bw.UseCases.Movement;
using Bw.UseCases.Movement.Abstractions;
using Bw.UseCases.Movement.Network.Platforms;
using Bw.UseCases.Movement.Physics;
using Zenject;

namespace Bw.Injection.Movement
{
    public class BodyInstaller<TState> : Installer<IRuntimeSettings, IBodyConfig, BodyInstaller<TState>>
        where TState : struct, IBodyState<TState>
    {
        private readonly IRuntimeSettings _runtimeSettings;
        private readonly IBodyConfig _config;

        public BodyInstaller(IRuntimeSettings runtimeSettings, IBodyConfig config)
        {
            _runtimeSettings = runtimeSettings;
            _config = config;
        }

        public override void InstallBindings()
        {
            RequirePushDeceleration();

            Container.Bind<IBodyConfig>().FromInstance(_config).AsSingle();
            Container.BindInterfacesTo<BoxBody>().AsSingle();
            Container.BindInterfacesTo<BodyMotor>().AsSingle();
            LagCompensationInstaller<TState, BodyStateView<TState>>.Install(Container, _runtimeSettings);

            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    Container.Decorate<IRewindable>().With<RiderRewinder>();
                    break;
                case PeerType.Client:
                    Container.Decorate<IStateView<TState>>().With<RiderStateView<TState>>();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void RequirePushDeceleration()
        {
            if (_config.PushDeceleration <= 0f)
                throw new InvalidOperationException(
                    $"Body config '{_config}' has no push deceleration: a knockback would carry the body forever, and the shooter's predicted push would never meet the server's.");
        }
    }
}
