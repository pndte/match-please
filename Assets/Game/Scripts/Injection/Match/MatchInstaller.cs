using System;
using Bw.Entities.Extensions;
using Bw.Entities.Network;
using Bw.Entities.Network.Objects;
using Bw.Entities.Network.Variables;
using Bw.Injection.ControlledBy;
using Bw.Injection.Network.Variables;
using Bw.Injection.Ownership;
using Bw.UseCases.Match;
using Bw.UseCases.Match.Network;
using Bw.UseCases.Match.Network.Requests;
using Bw.UseCases.Match.View.Hud;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

namespace Bw.Injection.Match
{
    public class MatchInstaller : MonoInstaller
    {
        [Inject] private IRuntimeSettings _runtimeSettings;

        [Header("Network")]
        [SerializeField] private NetworkObject _networkObject;
        [SerializeField] private NetworkLifetimedBehaviour _networkLifetimed;

        [Header("Hud")]
        [SerializeField] private UIDocument _hud;
        [SerializeField] private VisualTreeAsset _scoreRow;

        public override void InstallBindings()
        {
            OwnershipInstaller.Install(Container, _runtimeSettings);
            ControlledByInstaller.Install(Container, _runtimeSettings);

            Container.Bind<NetworkObject>().FromInstance(_networkObject).AsSingle();
            Container.Bind<INetworkLifetimedObject>().FromInstance(_networkLifetimed).AsSingle().NonLazy();
            Container.BindInstance(gameObject.Lifetime()).AsSingle();

            var netSchema = new NetEntriesSchemaBuilder();
            OwnershipServicesInstaller.Install(Container, _runtimeSettings, netSchema);
            ControlledByServicesInstaller.Install(Container, _runtimeSettings, netSchema);
            var table = netSchema.DeclareProperty(MatchTable.Empty, NetworkDelivery.ReliableFragmentedSequenced, NetworkPermissions.Server);
            NetTablesInstaller.Install(Container, _runtimeSettings, netSchema.Build());

            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    BindPublishing(table);
                    break;
                case PeerType.Client:
                    Container.Bind(typeof(IReadonlyMatchBoard), typeof(IMatchBoard)).To<MatchBoard>().AsSingle()
                        .WriterOnlyInto<IMatchBoard>(typeof(MatchReceiver));
                    Container.Bind<MatchReceiver>().AsSingle().WithArguments(table).NonLazy();
                    BindHudVisuals();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void BindPublishing(NetPropertyDeclaration<MatchTable> table)
        {
            Container.Bind<MatchTables>().AsSingle();
            Container.Bind<MatchTableWriter>().AsSingle().WithArguments(table);
            Container.Bind<MatchPublisher>().AsSingle().NonLazy();
        }

        private void BindHudVisuals()
        {
            Container.Bind<HudDocument>().AsSingle().WithArguments(_hud);
            Container.Bind<MatchCountdown>().AsSingle();
            Container.Bind<ScoreboardView>().AsSingle().WithArguments(_scoreRow).NonLazy();
            Container.Bind<WinnerView>().AsSingle().NonLazy();
            Container.Bind<MatchTimerView>().AsSingle().NonLazy();
        }
    }
}
