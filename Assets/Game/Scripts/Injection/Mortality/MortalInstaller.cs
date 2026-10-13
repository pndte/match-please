using System;
using Bw.Entities;
using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Events;
using Bw.Entities.Network.Variables;
using Bw.Injection.Network.Prediction;
using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Network;
using Bw.UseCases.Mortality.Network.Prediction;
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
            EventPredictionTargetInstaller<IReadonlyMortal, Vitals, Hit>.Install(Container, _runtimeSettings, _netSchema);

            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    Container.Bind<DeadDespawner>().AsSingle().NonLazy();
                    Container.BindInterfacesTo<VitalsEffectRules>().AsSingle();
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
                    typeof(AuthoritativeState<Vitals, Hit>), typeof(PredictedState<Vitals, Hit>));
            Container.Bind<CorpseHitbox>().AsSingle().WithArguments(_death.CorpseLayer.SingleLayer("corpse")).NonLazy();
            Container.InstantiateComponent<ReadonlyMortalHolder>(_body);
        }
    }
}
