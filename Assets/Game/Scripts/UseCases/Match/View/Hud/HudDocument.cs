using System;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine.UIElements;

namespace Bw.UseCases.Match.View.Hud
{
    public sealed class HudDocument
    {
        private readonly IMatchViewModel _match;
        private readonly UIDocument _document;

        public HudDocument(IMatchViewModel match, UIDocument document)
        {
            _match = match;
            _document = document;
        }

        public void WhenShown(Lifetime lifetime, Action<Lifetime, VisualElement> handler) =>
            _match.Shown.WhenTrue(lifetime, shownLifetime => handler(shownLifetime, _document.rootVisualElement));
    }
}
