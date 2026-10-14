using System.Collections.Generic;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;
using UnityEngine.UIElements;
using Unit = JetBrains.Core.Unit;

namespace Setup.Launch.Menu
{
    public sealed class ConnectingScreen : IMenuScreen
    {
        private const float DegreesPerSecond = 270f;
        private static readonly string[] ShownLayers = { "backdrop-layer", "cast-layer", "connecting-layer" };

        public IReadOnlyCollection<string> Layers => ShownLayers;
        public ISource<Unit> CancelChosen => _cancelChosen;

        private readonly Signal<Unit> _cancelChosen = new();
        private readonly MenuDocument _document;
        private readonly Label _endpoint;
        private readonly VisualElement _spinner;

        public ConnectingScreen(Lifetime lifetime, MenuDocument document)
        {
            var layer = document.Layer("connecting-layer");
            _document = document;
            _endpoint = layer.Q<Label>("endpoint");
            _spinner = layer.Q("spinner");

            layer.Q<Button>("cancel").AdviseClicked(lifetime, Back);
        }

        public void Show(Lifetime spinning, string endpoint)
        {
            _endpoint.text = endpoint;
            var spin = _spinner.schedule.Execute(Spin).Every(0);
            spinning.OnTermination(spin.Pause);
            _document.Show(this);
        }

        public void Back() =>
            _cancelChosen.Fire(Unit.Instance);

        public void Confirm()
        {
        }

        private void Spin() =>
            _spinner.style.rotate = new Rotate(new Angle(Time.unscaledTime * DegreesPerSecond % 360f, AngleUnit.Degree));
    }
}
