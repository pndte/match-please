using System;
using System.Linq;
using Bw.Entities.Network.Characters;
using JetBrains.Lifetimes;
using Setup.Launch;
using Setup.Launch.Menu;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

namespace Bw.Injection.Launch
{
    public sealed class LaunchInstaller : MonoInstaller
    {
        [Header("Start Menu")]
        [SerializeField] private PanelSettings _menuPanel;
        [SerializeField] private VisualTreeAsset _menuScreen;

        [Header("Characters")]
        [SerializeField] private CharacterCatalog _characters;

        public override void InstallBindings()
        {
            RequireMenu();
            CharacterCatalogValidation.Require(_characters);
            RequireCharacterButtons();

            BindMenu();
            BindScreens();
            BindLaunch();
        }

        private void BindMenu()
        {
            Container.Bind<MenuDocument>().FromMethod(_ => MenuDocument.Create(_menuPanel, _menuScreen)).AsSingle();
            Container.Bind<Lifetime>().FromMethod(context => context.Container.Resolve<MenuDocument>().Lifetime).AsSingle();
            Container.Bind<IPlayerProfile>().To<PlayerPrefsProfile>().AsSingle();
            Container.BindInstance(_characters).AsSingle();
        }

        private void BindScreens()
        {
            Container.Bind<MainScreen>().AsSingle();
            Container.Bind<CreateRoomScreen>().AsSingle();
            Container.Bind<JoinRoomScreen>().AsSingle();
            Container.Bind<CharacterSelectScreen>().AsSingle();
            Container.Bind<RoomScreen>().AsSingle();
            Container.Bind<ConnectingScreen>().AsSingle();
            Container.Bind<ConnectionFailedScreen>().AsSingle();
        }

        private void BindLaunch()
        {
            Container.Bind<StartMenu>().AsSingle();
            Container.Bind<RoomMenu>().AsSingle();
            Container.Bind<ConnectionMenu>().AsSingle();
            Container.Bind<NetworkLaunch>().AsSingle();
        }

        private void RequireMenu()
        {
            if (_menuPanel == null || _menuScreen == null)
                throw new InvalidOperationException("The start menu needs its panel settings and screen: set them on LaunchInstaller in GameSetupScene.");
        }

        private void RequireCharacterButtons()
        {
            var buttons = _menuScreen.CloneTree().Query<Button>(className: CharacterSelectScreen.CharacterClass).ToList().Select(button => button.name).ToHashSet();
            var keys = _characters.Characters.Select(character => character.Key).ToHashSet();
            if (!buttons.SetEquals(keys))
                throw new InvalidOperationException(
                    $"The character buttons of '{_menuScreen.name}' ({string.Join(", ", buttons)}) differ from the keys of '{_characters.name}' ({string.Join(", ", keys)}): a key without a button can't be picked, a button without a key picks nothing.");
        }
    }
}
