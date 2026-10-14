using System.Collections.Generic;
using Bw.Entities.Network;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine.UIElements;
using Unit = JetBrains.Core.Unit;

namespace Setup.Launch.Menu
{
    public sealed class JoinRoomScreen : IMenuScreen
    {
        private static readonly string[] ShownLayers = { "backdrop-layer", "cast-layer", "join-room-layer" };

        public IReadOnlyCollection<string> Layers => ShownLayers;
        public ISource<Unit> BackChosen => _backChosen;
        public ISource<LaunchChoice.Client> ServerChosen => _serverChosen;

        private readonly Signal<Unit> _backChosen = new();
        private readonly Signal<LaunchChoice.Client> _serverChosen = new();
        private readonly MenuDocument _document;
        private readonly IPlayerProfile _profile;
        private readonly ConnectionConfig _connection;
        private readonly TextField _address;
        private readonly TextField _port;
        private readonly Label _error;
        private readonly Button _recent;
        private readonly Label _recentAddress;
        private readonly SequentialLifetimes _shows;

        public JoinRoomScreen(
            Lifetime lifetime,
            MenuDocument document,
            IPlayerProfile profile,
            ConnectionConfig connection)
        {
            var layer = document.Layer("join-room-layer");
            _document = document;
            _profile = profile;
            _connection = connection;
            _address = layer.Q<TextField>("address");
            _port = layer.Q<TextField>("port");
            _error = layer.Q<Label>("error");
            _recent = layer.Q<Button>("recent");
            _recentAddress = layer.Q<Label>("recent-address");
            _shows = new SequentialLifetimes(lifetime);

            _port.AdviseDigitsOnly(lifetime);
            _address.AdviseChanged(lifetime, TakeAddress);
            layer.Q<Button>("back").AdviseClicked(lifetime, Back);
            layer.Q<Button>("connect").AdviseClicked(lifetime, Confirm);
        }

        public void Show()
        {
            var shown = _shows.Next();
            if (_profile.TryGetLastServer(out var recent))
                ShowRecent(shown, recent);
            else
                ShowDefault();

            _error.AddToClassList(MenuClasses.Hidden);
            _address.RemoveFromClassList(MenuClasses.FieldError);
            _port.RemoveFromClassList(MenuClasses.FieldError);
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

            if (!LaunchChoice.Client.TryParse(_address.value, port, out var server))
            {
                _error.RemoveFromClassList(MenuClasses.Hidden);
                _address.AddToClassList(MenuClasses.FieldError);
                return;
            }

            _profile.SaveLastServer(server);
            _serverChosen.Fire(server);
        }

        private void ShowRecent(Lifetime shown, LaunchChoice.Client recent)
        {
            _recent.parent.RemoveFromClassList(MenuClasses.Hidden);
            _recentAddress.text = recent.Endpoint();
            _recent.AdviseClicked(shown, () => Fill(recent.Address, recent.Port));
            Fill(recent.Address, recent.Port);
        }

        private void ShowDefault()
        {
            _recent.parent.AddToClassList(MenuClasses.Hidden);
            var address = LaunchChoice.Client.TryParse(_connection.ServerAddress, _connection.Port, out var server) ? server.Address : string.Empty;
            Fill(address, _connection.Port);
        }

        private void Fill(string address, ushort port)
        {
            _address.SetValueWithoutNotify(address);
            _port.SetValueWithoutNotify(port.ToString());
        }

        private void TakeAddress(string text)
        {
            _error.AddToClassList(MenuClasses.Hidden);
            _address.RemoveFromClassList(MenuClasses.FieldError);
            var parts = text.Trim().Split(':');
            if (parts.Length != 2)
                return;

            _address.SetValueWithoutNotify(parts[0]);
            _port.value = parts[1];
        }
    }
}
