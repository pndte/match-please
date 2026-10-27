using System;
using Bw.Entities.Network;
using Bw.Injection.Level;
using Bw.UseCases.Bots;
using Bw.UseCases.Bots.Navigation;
using Bw.UseCases.Bots.Network;
using Bw.UseCases.Bots.Network.Room;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Bw.Injection.Bots
{
    public sealed class BotsInstaller : MonoInstaller
    {
        public const string NavigationField = nameof(_navigation);
        public const string NavigationConfigField = nameof(_navigationConfig);

        [Inject] private IRuntimeSettings _runtimeSettings;

        [SerializeField] private BotsConfig _bots;

        [Header("Navigation")]
        [SerializeField] private NavGraphAsset _navigation;
        [SerializeField] private NavGraphConfig _navigationConfig;

        public override void InstallBindings()
        {
            if (_runtimeSettings.CurrentPeerType != PeerType.Server)
                return;

            RequireBots();
            RequireNavigation();
            Container.BindInstance(_bots).AsSingle();
            Container.BindInstance(_navigationConfig).AsSingle();
            Container.Bind<NavGraph>().FromMethod(_ => NavGraph.Of(_navigation)).AsSingle();
            Container.Bind<NavPathfinder>().AsSingle();
            Container.Bind(typeof(IReadonlyBotRoster), typeof(IBotRoster)).To<BotRoster>().AsSingle()
                .WriterOnlyInto<IBotRoster>(typeof(RoomBotsSeeder), typeof(BotJoinServerHandler), typeof(BotSeatServerHandler));
            Container.Bind<RoomBotsSeeder>().AsSingle().NonLazy();
            Container.Bind<IBotLauncher>().To<BotLauncher>().AsSingle();
            Container.Bind<BotLives>().AsSingle().NonLazy();
            Container.Bind<StandInBots>().AsSingle().NonLazy();
        }

        private void RequireBots()
        {
            if (_bots == null)
                throw new InvalidOperationException($"'{name}' has no bots config: assign Assets/Game/Content/Bots/BotsConfig.");

            if (_bots.StandIn == null)
                throw new InvalidOperationException($"Bots config '{_bots.name}' has no stand-in difficulty: a bot of it plays for a player whose client left, assign Assets/Game/Content/Bots/Light.");

            if (_bots.Fists == null)
                throw new InvalidOperationException($"Bots config '{_bots.name}' has no fists config: bots judge their punches by Assets/Game/Content/Weapons/FistsMeleeConfig.");

        }

        private void RequireNavigation()
        {
            if (_navigation == null || _navigationConfig == null)
                throw new InvalidOperationException($"'{name}' has no navigation for its bots: assign Assets/Game/Content/Bots/NavGraphConfig and bake the graph (Tools → Bots → Bake Navigation).");

            var fingerprint = NavFingerprint.Of(LevelBoxes.In(gameObject.scene), _navigationConfig, 1f / NetworkManager.Singleton.NetworkConfig.TickRate);
            if (_navigation.Fingerprint != fingerprint)
                throw new InvalidOperationException($"Navigation graph '{_navigation.name}' was baked for another level or movement: bake it again (Tools → Bots → Bake Navigation).");
        }
    }
}
