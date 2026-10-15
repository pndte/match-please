using System;
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

        public override void InstallBindings()
        {
            RequireMenu();
            RequireCharacterCard();

            BindMenu();
            BindScreens();
            BindLaunch();
        }

        private void BindMenu()
        {
            Container.Bind<MenuDocument>().FromMethod(_ => MenuDocument.Create(_menuPanel, _menuScreen)).AsSingle();
            Container.Bind<Lifetime>().FromMethod(context => context.Container.Resolve<MenuDocument>().Lifetime).AsSingle();
            Container.Bind<IPlayerProfile>().To<PlayerPrefsProfile>().AsSingle();
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

        private void RequireCharacterCard()
        {
            var sample = _menuScreen.CloneTree().Q<TemplateContainer>(CharacterSelectScreen.SampleName);
            if (sample == null || sample.templateSource == null)
                throw new InvalidOperationException(
                    $"The character select of '{_menuScreen.name}' has no card sample '{CharacterSelectScreen.SampleName}': the screen builds a card for every catalog character from the sample's template.");

            var card = sample.templateSource.Instantiate().Q<Button>(className: CharacterSelectScreen.CardClass);
            if (card == null || card.Q(CharacterSelectScreen.FigureName) == null || card.Q<Label>(CharacterSelectScreen.NameLabel) == null)
                throw new InvalidOperationException(
                    $"The character card '{sample.templateSource.name}' needs a button of class '{CharacterSelectScreen.CardClass}' with the '{CharacterSelectScreen.FigureName}' element and the '{CharacterSelectScreen.NameLabel}' label.");
        }
    }
}
