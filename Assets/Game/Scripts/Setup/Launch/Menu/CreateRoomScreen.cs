using System.Collections.Generic;
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
        private static readonly string[] ShownLayers = { "backdrop-layer", "cast-layer", "create-room-layer" };

        public IReadOnlyCollection<string> Layers => ShownLayers;
        public ISource<Unit> BackChosen => _backChosen;
        public ISource<ushort> PortChosen => _portChosen;

        private readonly Signal<Unit> _backChosen = new();
        private readonly Signal<ushort> _portChosen = new();
        private readonly MenuDocument _document;
        private readonly IPlayerProfile _profile;
        private readonly TextField _port;
        private readonly Label _hint;
        private readonly string _freeHint;
        private readonly VisualElement _addresses;

        public CreateRoomScreen(Lifetime lifetime, MenuDocument document, IPlayerProfile profile)
        {
            var layer = document.Layer("create-room-layer");
            _document = document;
            _profile = profile;
            _port = layer.Q<TextField>("port");
            _hint = layer.Q<Label>("port-hint");
            _freeHint = _hint.text;
            _addresses = layer.Q("addresses");

            _port.AdviseDigitsOnly(lifetime);
            layer.Q<Button>("back").AdviseClicked(lifetime, Back);
            layer.Q<Button>("create").AdviseClicked(lifetime, Confirm);
        }

        public void Show()
        {
            ShowPort(_profile.RoomPort);
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
            _portChosen.Fire(port);
        }

        private void ShowPort(ushort port)
        {
            _port.SetValueWithoutNotify(port.ToString());
            _port.RemoveFromClassList(MenuClasses.FieldError);
            AddressChips.Fill(_addresses);
        }
    }
}
