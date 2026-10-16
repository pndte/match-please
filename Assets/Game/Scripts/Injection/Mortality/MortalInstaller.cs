using System;
using Bw.Entities;
using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Events;
using Bw.Entities.Network.Prediction.Events.Requests;
using Bw.Entities.Network.Variables;
using Bw.Injection.Network;
using Bw.Injection.Network.Prediction;
using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Network;
using Bw.UseCases.Mortality.Network.Prediction;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Bw.Injection.Mortality
{
    public class MortalInstaller : Installer<IRuntimeSettings, INetEntriesSchemaBuilder, DeathConfig, GameObject, MortalInstaller>
    {
        private readonly IRuntimeSettings _runtimeSettings;
        private readonly INetEntriesSchemaBuilder _netSchema;
        private readonly DeathConfig _death;
        private readonly GameObject _body;

        public MortalInstaller(
            IRuntimeSettings runtimeSettings,
            INetEntriesSchemaBuilder netSchema,
            DeathConfig death,
            GameObject body)
        {
            _runtimeSettings = runtimeSettings;
            _netSchema = netSchema;
            _death = death;
            _body = body;
        }

        public override void InstallBindings()
        {
            BindMortal();
            var vitals = _netSchema.DeclareResult<CausedState<Vitals>>(NetworkDelivery.Reliable);
            EventPredictionTargetInstaller<IReadonlyMortal, Vitals, Impacts>.Install(Container, _runtimeSettings, vitals);

            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    Container.Bind<DeadDespawner>().AsSingle().NonLazy();
                    Container.BindInterfacesTo<VitalsEffectRules>().AsSingle();
                    BindHeals(vitals);
                    break;
                case PeerType.Client:
                    Container.BindInterfacesTo<VitalsPredictionRules>().AsSingle();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void BindMortal()
        {
            Container.BindInstance(_death).AsSingle();
            Container.Bind(typeof(IReadonlyHealth), typeof(IHealth)).To<Health>().AsSingle()
                .WriterOnlyInto<IHealth>(typeof(Mortal));
            Container.Bind(typeof(IReadonlyMortal), typeof(IReadonlyAppliedState<Vitals>), typeof(IAppliedState<Vitals>))
                .To<Mortal>().AsSingle()
                .WriterOnlyInto<IAppliedState<Vitals>>(
                    typeof(AuthoritativeState<Vitals, Impacts>), typeof(AuthoritativeState<Vitals, Heal>), typeof(PredictedState<Vitals, Impacts>));
            Container.Bind<CorpseHitbox>().AsSingle().WithArguments(_death.CorpseLayer.SingleLayer("corpse")).NonLazy();
            Container.InstantiateComponent<ReadonlyMortalHolder>(_body);
        }

        private void BindHeals(NetResultDeclaration<CausedState<Vitals>> vitals)
        {
            Container.BindResultSender(vitals).WhenInjectedInto<AuthoritativeState<Vitals, Heal>>();
            Container.Bind<IAffectable<Heal>>().To<AuthoritativeState<Vitals, Heal>>().AsSingle();
            Container.BindInterfacesTo<VitalsHealRules>().AsSingle();
        }
    }
}
