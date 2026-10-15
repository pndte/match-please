using System.Collections.Generic;
using Bw.CoreAdapters.Characters;
using Bw.UseCases.Character;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine.UIElements;
using Unit = JetBrains.Core.Unit;

namespace Setup.Launch.Menu
{
    public sealed class CharacterSelectScreen : IMenuScreen
    {
        public const string SampleName = "character-sample";
        public const string CardClass = "character";
        public const string FigureName = "figure";
        public const string NameLabel = "name";
        private const string Selected = "character--selected";
        private static readonly string[] ShownLayers = { "backdrop-layer", "character-select-layer" };

        public IReadOnlyCollection<string> Layers => ShownLayers;
        public ISource<Unit> BackChosen => _backChosen;
        public ISource<Unit> CharacterChosen => _characterChosen;

        private readonly Signal<Unit> _backChosen = new();
        private readonly Signal<Unit> _characterChosen = new();
        private readonly Dictionary<CharacterDescription, Button> _cards = new();
        private readonly MenuDocument _document;
        private readonly IPlayerProfile _profile;
        private readonly CharacterCatalog _characters;
        private readonly CharacterLooksConfig _looks;
        private readonly Label _server;
        private readonly VisualElement _cast;
        private readonly VisualTreeAsset _card;
        private CharacterDescription _picked;

        public CharacterSelectScreen(
            Lifetime lifetime,
            MenuDocument document,
            IPlayerProfile profile,
            CharacterCatalog characters,
            CharacterLooksConfig looks)
        {
            var layer = document.Layer("character-select-layer");
            var sample = layer.Q<TemplateContainer>(SampleName);
            _document = document;
            _profile = profile;
            _characters = characters;
            _looks = looks;
            _server = layer.Q<Label>("server");
            _cast = sample.parent;
            _card = sample.templateSource;
            _picked = characters.Accepted(profile.CharacterKey);

            layer.Q<Button>("disconnect").AdviseClicked(lifetime, Back);
            layer.Q<Button>("fight").AdviseClicked(lifetime, Confirm);
        }

        public void Show(LaunchChoice.Client server)
        {
            _server.text = server.Endpoint();
            FillCast();
            Pick(_picked);
            _document.Show(this);
        }

        public void Back() =>
            _backChosen.Fire(Unit.Instance);

        public void Confirm()
        {
            _profile.SaveCharacterKey(_picked.Key);
            _characterChosen.Fire(Unit.Instance);
        }

        private void FillCast()
        {
            _cast.Clear();
            _cards.Clear();
            foreach (var entry in _characters.Characters)
            {
                var card = CardOf(entry.Character);
                _cards.Add(entry.Character, card);
                _cast.Add(card);
            }
        }

        private Button CardOf(CharacterDescription character)
        {
            var look = _looks.LookOf(character);
            var card = _card.Instantiate().Q<Button>(className: CardClass);
            card.name = character.name;
            card.Q(FigureName).style.backgroundImage = new StyleBackground(look.Portrait);
            card.Q<Label>(NameLabel).text = look.DisplayName;
            card.clicked += () => Pick(character);
            return card;
        }

        private void Pick(CharacterDescription character)
        {
            _picked = character;
            foreach (var (shown, card) in _cards)
                card.EnableInClassList(Selected, shown == character);
        }
    }
}
