using System;
using Bw.CoreAdapters.Weapons;
using Bw.Entities.Extensions;
using Bw.Entities.Network;
using Bw.Entities.Network.Objects;
using Bw.Entities.Network.Variables;
using Bw.Injection.ControlledBy;
using Bw.Injection.Network;
using Bw.Injection.Network.Variables;
using Bw.Injection.Ownership;
using Bw.UseCases.Bots.Network.Requests;
using Bw.UseCases.Bots.Network.Room;
using Bw.UseCases.Bots.View.Adapters;
using Bw.UseCases.Bots.View.Roster;
using Bw.UseCases.Bots.View.Window;
using Bw.UseCases.Match;
using Bw.UseCases.Match.Network;
using Bw.UseCases.Match.Network.Requests;
using Bw.UseCases.Match.View.Adapters;
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
        [Inject] private WeaponCatalog _weapons;

        [Header("Network")]
        [SerializeField] private NetworkObject _networkObject;
        [SerializeField] private NetworkLifetimedBehaviour _networkLifetimed;

        [Header("Hud")]
        [SerializeField] private UIDocument _hud;
        [SerializeField] private VisualTreeAsset _scoreRow;
        [SerializeField] private KillFeedConfig _killFeed;
        [SerializeField] private WeaponIconsConfig _weaponIcons;

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
            var kills = netSchema.DeclareResult<KillNotice>(NetworkDelivery.ReliableSequenced);
            var localPlayer = netSchema.DeclareResult<int>(NetworkDelivery.Reliable);
            var bots = new BotRosterEntries(
                netSchema.DeclareProperty(BotTable.Empty, NetworkDelivery.ReliableFragmentedSequenced, NetworkPermissions.Server),
                netSchema.DeclareRequest<RoomBot>(NetworkDelivery.Reliable),
                netSchema.DeclareRequest<int>(NetworkDelivery.Reliable),
                netSchema.DeclareRequest<BotPresetChange>(NetworkDelivery.Reliable));
            NetTablesInstaller.Install(Container, _runtimeSettings, netSchema.Build());

            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    BindPublishing(table);
                    BindKillPublishing(kills);
                    BindLocalPlayerPublishing(localPlayer);
                    BindBotRosterServing(bots);
                    break;
                case PeerType.Client:
                    BindLocalPlayerReceiving(localPlayer);
                    Container.Bind(typeof(IReadonlyMatchBoard), typeof(IMatchBoard)).To<MatchBoard>().AsSingle()
                        .WriterOnlyInto<IMatchBoard>(typeof(MatchReceiver));
                    Container.Bind<MatchReceiver>().AsSingle().WithArguments(table).NonLazy();
                    BindKillReceiving(kills);
                    BindBotRosterOrdering(bots);
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

        private void BindKillPublishing(NetResultDeclaration<KillNotice> kills)
        {
            Container.Bind<KillNotices>().AsSingle();
            Container.BindResultSender(kills).WhenInjectedInto<KillPublisher>();
            Container.Bind<KillPublisher>().AsSingle().NonLazy();
        }

        private void BindLocalPlayerPublishing(NetResultDeclaration<int> localPlayer)
        {
            Container.BindResultSender(localPlayer).WhenInjectedInto<LocalPlayerPublisher>();
            Container.Bind<LocalPlayerPublisher>().AsSingle().NonLazy();
        }

        private void BindLocalPlayerReceiving(NetResultDeclaration<int> localPlayer)
        {
            Container.Bind<ILocalPlayer>().To<LocalPlayer>().AsSingle();
            Container.BindResultReceiver(localPlayer).WhenInjectedInto<LocalPlayerReceiver>();
            Container.Bind<LocalPlayerReceiver>().AsSingle().NonLazy();
        }

        private void BindKillReceiving(NetResultDeclaration<KillNotice> kills)
        {
            Container.Bind(typeof(IReadonlyKillFeed), typeof(IKillFeed)).To<KillFeed>().AsSingle()
                .WriterOnlyInto<IKillFeed>(typeof(KillReceiver));
            Container.BindResultReceiver(kills).WhenInjectedInto<KillReceiver>();
            Container.Bind<KillReceiver>().AsSingle().NonLazy();
        }

        private void BindBotRosterServing(BotRosterEntries bots)
        {
            Container.Bind<RoomHostAssigner>().AsSingle().NonLazy();
            Container.Bind<BotTablePublisher>().AsSingle().WithArguments(bots.Table).NonLazy();
            Container.BindRequestReceiver(bots.Joins).WhenInjectedInto<BotJoinServerHandler>();
            Container.Bind<BotJoinServerHandler>().AsSingle().NonLazy();
            Container.BindRequestReceiver(bots.Removals).WhenInjectedInto<BotSeatServerHandler>();
            Container.BindRequestReceiver(bots.Changes).WhenInjectedInto<BotSeatServerHandler>();
            Container.Bind<BotSeatServerHandler>().AsSingle().NonLazy();
        }

        private void BindBotRosterOrdering(BotRosterEntries bots)
        {
            Container.Bind(typeof(IReadonlyBotSeats), typeof(IBotSeats)).To<BotSeats>().AsSingle()
                .WriterOnlyInto<IBotSeats>(typeof(BotTableReceiver));
            Container.Bind<BotTableReceiver>().AsSingle().WithArguments(bots.Table).NonLazy();
            Container.BindRequestSender(bots.Joins).WhenInjectedInto<BotOrderSender>();
            Container.BindRequestSender(bots.Removals).WhenInjectedInto<BotOrderSender>();
            Container.BindRequestSender(bots.Changes).WhenInjectedInto<BotOrderSender>();
            Container.Bind<IBotOrders>().To<BotOrderSender>().AsSingle();
        }

        private void BindHudVisuals()
        {
            if (_killFeed == null)
                throw new InvalidOperationException("The match HUD has no kill feed config: assign Assets/Game/Content/Match/KillFeedConfig.");

            RequireWeaponIcons();
            Container.Bind<MatchCountdown>().AsSingle();
            Container.Bind<IMatchViewModel>().To<MatchViewModel>().AsSingle();
            Container.Bind<HudDocument>().AsSingle().WithArguments(_hud);
            Container.Bind<ScoreboardView>().AsSingle().WithArguments(_scoreRow).NonLazy();
            Container.Bind<WinnerView>().AsSingle().NonLazy();
            Container.Bind<MatchTimerView>().AsSingle().NonLazy();
            Container.Bind<IKillFeedViewModel>().To<KillFeedViewModel>().AsSingle().WithArguments(_weaponIcons);
            Container.Bind<KillFeedView>().AsSingle().WithArguments(_killFeed).NonLazy();
            Container.Bind<IBotRosterViewModel>().To<MatchBotRosterViewModel>().AsSingle();
            Container.Bind<IBotsWindowViewModel>().To<BotsWindowViewModel>().AsSingle();
            Container.Bind<BotsWindowView>().AsSingle().NonLazy();
        }

        private void RequireWeaponIcons()
        {
            if (_weaponIcons == null)
                throw new InvalidOperationException("The match HUD has no weapon icons: assign Assets/Game/Content/Match/WeaponIconsConfig.");

            foreach (var entry in _weapons.Weapons)
                if (Array.FindIndex(_weaponIcons.Icons, row => row.Weapon == entry.Weapon && row.Icon != null) < 0)
                    throw new InvalidOperationException($"Weapon '{entry.Weapon.name}' has no icon in '{_weaponIcons.name}': the kill feed shows the weapon of every kill.");
        }
    }
}
