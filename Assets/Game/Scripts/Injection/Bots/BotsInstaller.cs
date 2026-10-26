using System;
using Bw.CoreAdapters.Characters;
using Bw.Entities.Network;
using Bw.Entities.Players;
using Bw.Injection.Level;
using Bw.UseCases.Bots;
using Bw.UseCases.Bots.Navigation;
using Bw.UseCases.Bots.Network;
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
        [Inject] private CharacterCatalog _characters;

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
            Container.Bind<IBotRoster>().To<BotRoster>().AsSingle().NonLazy();
            Container.Bind<IBotLauncher>().To<BotLauncher>().AsSingle();
            Container.Bind<BotLives>().AsSingle().NonLazy();
            Container.Bind<StandInBots>().AsSingle().NonLazy();
        }

        private void RequireBots()
        {
            if (_bots == null)
                throw new InvalidOperationException($"'{name}' has no bots config: assign Assets/Game/Content/Bots/BotsConfig, whose empty list means a level without bots.");

            if (_bots.StandIn == null)
                throw new InvalidOperationException($"Bots config '{_bots.name}' has no stand-in difficulty: a bot of it plays for a player whose client left, assign Assets/Game/Content/Bots/Light.");

            if (_bots.Fists == null)
                throw new InvalidOperationException($"Bots config '{_bots.name}' has no fists config: bots judge their punches by Assets/Game/Content/Weapons/FistsMeleeConfig.");

            foreach (var bot in _bots.Bots)
            {
                if (bot.Difficulty == null)
                    throw new InvalidOperationException($"Bot '{bot.Nickname}' in '{_bots.name}' has no difficulty: assign one from Assets/Game/Content/Bots.");

                if (!NicknameRules.IsValid(bot.Nickname))
                    throw new InvalidOperationException($"Bot '{bot.Nickname}' in '{_bots.name}' has a nickname no player may have: {NicknameRules.MinLength}-{NicknameRules.MaxLength} letters, digits, spaces, '_' or '-'.");

                if (bot.Character == null || Array.FindIndex(_characters.Characters, entry => entry.Character == bot.Character) < 0)
                    throw new InvalidOperationException($"Bot '{bot.Nickname}' in '{_bots.name}' plays a character the catalog '{_characters.name}' lacks: assign a description listed there.");
            }
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
