using System.Collections.Generic;
using Bw.Entities.Network.Characters;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine.UIElements;
using Unit = JetBrains.Core.Unit;

namespace Setup.Launch.Menu
{
    public sealed class CharacterSelectScreen : IMenuScreen
    {
        public const string CharacterClass = "character";
        private const string Selected = "character--selected";
        private static readonly string[] ShownLayers = { "backdrop-layer", "character-select-layer" };

        public IReadOnlyCollection<string> Layers => ShownLayers;
        public ISource<Unit> BackChosen => _backChosen;
        public ISource<Unit> CharacterChosen => _characterChosen;

        private readonly Signal<Unit> _backChosen = new();
        private readonly Signal<Unit> _characterChosen = new();
        private readonly MenuDocument _document;
        private readonly IPlayerProfile _profile;
        private readonly VisualElement _layer;
        private readonly Label _server;
        private string _picked;

        public CharacterSelectScreen(
            Lifetime lifetime,
            MenuDocument document,
            IPlayerProfile profile,
            CharacterCatalog characters)
        {
            _layer = document.Layer("character-select-layer");
            _document = document;
            _profile = profile;
            _server = _layer.Q<Label>("server");
            _picked = characters.Accepted(profile.CharacterKey);

            _layer.Query<Button>(className: CharacterClass).ForEach(button => button.AdviseClicked(lifetime, () => Pick(button.name)));
            _layer.Q<Button>("disconnect").AdviseClicked(lifetime, Back);
            _layer.Q<Button>("fight").AdviseClicked(lifetime, Confirm);
        }

        public void Show(LaunchChoice.Client server)
        {
            _server.text = server.Endpoint();
            Pick(_picked);
            _document.Show(this);
        }

        public void Back() =>
            _backChosen.Fire(Unit.Instance);

        public void Confirm()
        {
            _profile.SaveCharacterKey(_picked);
            _characterChosen.Fire(Unit.Instance);
        }

        private void Pick(string key)
        {
            _picked = key;
            _layer.Query<Button>(className: CharacterClass).ForEach(button => button.EnableInClassList(Selected, button.name == key));
        }
    }
}
