using System;
using System.Collections.Generic;
using Bw.Entities.Network;
using Bw.UseCases.Bots.View.Roster;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine.UIElements;
using Unit = JetBrains.Core.Unit;

namespace Setup.Launch.Menu
{
    public sealed class CreateRoomScreen : IMenuScreen
    {
        private const string HintError = "error";
        private const string BusyHint = "Порт занят, выбери другой";
        private const string Unchosen = "button--paper";
        private static readonly string[] ShownLayers = { "backdrop-layer", "cast-layer", "create-room-layer" };

        public IReadOnlyCollection<string> Layers => ShownLayers;
        public ISource<Unit> BackChosen => _backChosen;
        public ISource<LaunchChoice.Room> RoomChosen => _roomChosen;

        private readonly Signal<Unit> _backChosen = new();
        private readonly Signal<LaunchChoice.Room> _roomChosen = new();
        private readonly MenuDocument _document;
        private readonly IPlayerProfile _profile;
        private readonly RoomLevelsConfig _levels;
        private readonly RoomBotsViewModel _bots;
        private readonly TextField _port;
        private readonly Label _hint;
        private readonly string _freeHint;
        private readonly VisualElement _addresses;
        private readonly VisualElement _levelButtons;

        private string _level;

        public CreateRoomScreen(
            Lifetime lifetime,
            MenuDocument document,
            IPlayerProfile profile,
            RoomLevelsConfig levels,
            RoomBotsViewModel bots)
        {
            var layer = document.Layer("create-room-layer");
            _document = document;
            _profile = profile;
            _levels = levels;
            _bots = bots;
            _port = layer.Q<TextField>("port");
            _hint = layer.Q<Label>("port-hint");
            _freeHint = _hint.text;
            _addresses = layer.Q("addresses");
            _levelButtons = layer.Q("levels");

            _port.AdviseDigitsOnly(lifetime);
            layer.Q<Button>("back").AdviseClicked(lifetime, Back);
            layer.Q<Button>("create").AdviseClicked(lifetime, Confirm);
            new BotRosterView(lifetime, layer.Q("bots"), bots);
        }

        public void Show()
        {
            ShowPort(_profile.RoomPort);
            ShowLevels(_profile.RoomLevel);
            _hint.text = _freeHint;
            _hint.RemoveFromClassList(HintError);
            _document.Show(this);
        }

        public void ShowBusy(ushort port)
        {
            ShowPort(port);
            _port.AddToClassList(MenuClasses.FieldError);
            _hint.text = BusyHint;
            _hint.AddToClassList(HintError);
            _document.Show(this);
        }

        public void Back() =>
            _backChosen.Fire(Unit.Instance);

        public void Confirm()
        {
            if (!PortFields.TryParse(_port.value, out var port))
            {
                _port.AddToClassList(MenuClasses.FieldError);
                return;
            }

            _profile.SaveRoomPort(port);
            _profile.SaveRoomLevel(_level);
            _roomChosen.Fire(new LaunchChoice.Room(port, new RoomSettings(_level, Guid.NewGuid(), _bots.RoomBots)));
        }

        private void ShowLevels(string saved)
        {
            _level = Array.Exists(_levels.Levels, level => level.Scene == saved) ? saved : _levels.Levels[0].Scene;
            _levelButtons.Clear();
            foreach (var level in _levels.Levels)
            {
                var scene = level.Scene;
                var button = new Button(() => ChooseLevel(scene)) { text = level.DisplayName };
                button.AddToClassList("button");
                button.AddToClassList("button--small");
                button.EnableInClassList(Unchosen, scene != _level);
                button.userData = scene;
                _levelButtons.Add(button);
            }
        }

        private void ChooseLevel(string scene)
        {
            _level = scene;
            foreach (var button in _levelButtons.Children())
                button.EnableInClassList(Unchosen, (string)button.userData != scene);
        }

        private void ShowPort(ushort port)
        {
            _port.SetValueWithoutNotify(port.ToString());
            _port.RemoveFromClassList(MenuClasses.FieldError);
            AddressChips.Fill(_addresses);
        }
    }
}
