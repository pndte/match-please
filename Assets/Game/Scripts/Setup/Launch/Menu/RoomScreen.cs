using System.Collections.Generic;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine.UIElements;
using Unit = JetBrains.Core.Unit;

namespace Setup.Launch.Menu
{
    public sealed class RoomScreen : IMenuScreen
    {
        private static readonly string[] ShownLayers = { "backdrop-layer", "cast-layer", "room-open-layer" };

        public IReadOnlyCollection<string> Layers => ShownLayers;
        public ISource<Unit> CloseChosen => _closeChosen;

        private readonly Signal<Unit> _closeChosen = new();
        private readonly MenuDocument _document;
        private readonly Label _port;
        private readonly Label _players;
        private readonly VisualElement _addresses;

        public RoomScreen(Lifetime lifetime, MenuDocument document)
        {
            var layer = document.Layer("room-open-layer");
            _document = document;
            _port = layer.Q<Label>("port");
            _players = layer.Q<Label>("players");
            _addresses = layer.Q("addresses");

            layer.Q<Button>("close-room").AdviseClicked(lifetime, () => _closeChosen.Fire(Unit.Instance));
        }

        public void Show(ushort port)
        {
            _port.text = $"Порт {port}";
            ShowPlayers(0);
            AddressChips.Fill(_addresses);
            _document.Show(this);
        }

        public void ShowPlayers(int count) =>
            _players.text = $"Игроков: {count}";

        public void Back()
        {
        }

        public void Confirm()
        {
        }
    }
}
