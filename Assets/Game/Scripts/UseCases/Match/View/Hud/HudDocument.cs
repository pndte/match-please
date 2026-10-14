using System;
using Bw.Entities.Extensions;
using Bw.Entities.Network.Objects;
using JetBrains.Lifetimes;
using UnityEngine.UIElements;

namespace Bw.UseCases.Match.View.Hud
{
    public sealed class HudDocument
    {
        private readonly INetworkLifetimedObject _networkLifetimed;
        private readonly UIDocument _document;

        public HudDocument(INetworkLifetimedObject networkLifetimed, UIDocument document)
        {
            _networkLifetimed = networkLifetimed;
            _document = document;
        }

        public void WhenShown(Lifetime lifetime, Action<Lifetime, VisualElement> handler) =>
            _networkLifetimed.SpawnedLifetime.WhenAlive(lifetime, spawnedLifetime => handler(spawnedLifetime, _document.rootVisualElement));
    }
}
