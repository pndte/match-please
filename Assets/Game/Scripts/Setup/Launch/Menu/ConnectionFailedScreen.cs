using System.Collections.Generic;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine.UIElements;
using Unit = JetBrains.Core.Unit;

namespace Setup.Launch.Menu
{
    public sealed class ConnectionFailedScreen : IMenuScreen
    {
        private static readonly string[] ShownLayers = { "backdrop-layer", "cast-layer", "connection-failed-layer" };

        public IReadOnlyCollection<string> Layers => ShownLayers;
        public ISource<Unit> BackChosen => _backChosen;
        public ISource<Unit> RetryChosen => _retryChosen;

        private readonly Signal<Unit> _backChosen = new();
        private readonly Signal<Unit> _retryChosen = new();
        private readonly MenuDocument _document;
        private readonly Label _reason;

        public ConnectionFailedScreen(Lifetime lifetime, MenuDocument document)
        {
            var layer = document.Layer("connection-failed-layer");
            _document = document;
            _reason = layer.Q<Label>("reason");

            layer.Q<Button>("back").AdviseClicked(lifetime, Back);
            layer.Q<Button>("retry").AdviseClicked(lifetime, Confirm);
        }

        public void Show(string reason)
        {
            _reason.text = reason;
            _document.Show(this);
        }

        public void Back() =>
            _backChosen.Fire(Unit.Instance);

        public void Confirm() =>
            _retryChosen.Fire(Unit.Instance);
    }
}
