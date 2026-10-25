using System;
using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Events;
using Bw.Entities.Network.Variables;
using Bw.Entities.Simulation;
using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Guarding;
using Bw.UseCases.Mortality.Network.Guarding;
using Unity.Netcode;
using Zenject;

namespace Bw.Injection.Mortality
{
    public sealed class GuardInstaller : Installer<IRuntimeSettings, INetEntriesSchemaBuilder, GuardConfig, GuardInstaller>
    {
        private readonly IRuntimeSettings _runtimeSettings;
        private readonly INetEntriesSchemaBuilder _netSchema;
        private readonly GuardConfig _config;

        public GuardInstaller(IRuntimeSettings runtimeSettings, INetEntriesSchemaBuilder netSchema, GuardConfig config)
        {
            _runtimeSettings = runtimeSettings;
            _netSchema = netSchema;
            _config = config;
        }

        public override void InstallBindings()
        {
            var shown = _netSchema.DeclareProperty(new GuardState(_config.MaxHealth, false, false), NetworkDelivery.Reliable, NetworkPermissions.Server);
            Container.BindInstance(_config).AsSingle();

            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    Container.Bind(typeof(IReadonlyGuard), typeof(IGuardCover), typeof(IGuardBlows), typeof(ISteppable<GuardStance>)).To<Guard>().AsSingle();
                    BindBlows();
                    Container.Bind<GuardPublisher>().AsSingle().WithArguments(shown).NonLazy();
                    break;
                case PeerType.Client: //TODO: клиент стрелка предсказывает попадание в поднятый блок как полный урон по жизням, и сервер его потом поправляет: прогнозу нужны правила с блоком
                    Container.Bind(typeof(IReadonlyGuard), typeof(IGuard)).To<Guard>().AsSingle()
                        .WriterOnlyInto<IGuard>(typeof(GuardReceiver));
                    Container.Bind<GuardReceiver>().AsSingle().WithArguments(shown).NonLazy();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_runtimeSettings.CurrentPeerType), _runtimeSettings.CurrentPeerType, null);
            }
        }

        private void BindBlows()
        {
            Container.Bind<GuardBlows>().AsSingle();
            Container.Bind<AffectableRegistration<IReadonlyMortal, Impacts>>()
                .FromMethod(context => context.Container.Instantiate<AffectableRegistration<IReadonlyMortal, Impacts>>(new object[] { context.Container.Resolve<GuardBlows>() }))
                .AsCached()
                .NonLazy();
            Container.Decorate<IEffectRules<Vitals, Impacts>>().With<GuardedVitalsRules>();
        }
    }
}
