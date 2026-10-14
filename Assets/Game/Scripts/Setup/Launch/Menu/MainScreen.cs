using System.Collections.Generic;
using System.Linq;
using Bw.Entities.Players;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;
using UnityEngine.UIElements;
using Unit = JetBrains.Core.Unit;

namespace Setup.Launch.Menu
{
    public sealed class MainScreen : IMenuScreen
    {
        private const string Locked = "button--locked";
        private static readonly string[] ShownLayers = { "backdrop-layer", "cast-layer", "main-menu-layer" };

        public IReadOnlyCollection<string> Layers => ShownLayers;
        public ISource<Unit> CreateChosen => _createChosen;
        public ISource<Unit> JoinChosen => _joinChosen;

        private readonly Signal<Unit> _createChosen = new();
        private readonly Signal<Unit> _joinChosen = new();
        private readonly MenuDocument _document;
        private readonly IPlayerProfile _profile;
        private readonly TextField _nickname;
        private readonly Button _createRoom;
        private readonly Button _joinRoom;

        public MainScreen(Lifetime lifetime, MenuDocument document, IPlayerProfile profile)
        {
            var layer = document.Layer("main-menu-layer");
            _document = document;
            _profile = profile;
            _nickname = layer.Q<TextField>("player-name");
            _createRoom = layer.Q<Button>("create-room");
            _joinRoom = layer.Q<Button>("join-room");

            _nickname.AdviseChanged(lifetime, Clean);
            _createRoom.AdviseClicked(lifetime, () => Proceed(_createChosen));
            _joinRoom.AdviseClicked(lifetime, () => Proceed(_joinChosen));
            layer.Q<Button>("quit").AdviseClicked(lifetime, Application.Quit);
        }

        public void Show()
        {
            var saved = _profile.Nickname;
            _nickname.SetValueWithoutNotify(NicknameRules.IsValid(saved) ? saved : string.Empty);
            RefreshLocks();
            _document.Show(this);
            if (_nickname.value.Length == 0)
                _nickname.schedule.Execute(() => _nickname.Focus());
        }

        public void Back()
        {
        }

        public void Confirm()
        {
        }

        private void Clean(string text)
        {
            var allowed = new string(text.Where(NicknameRules.IsAllowed).Take(NicknameRules.MaxLength).ToArray());
            if (allowed != text)
                _nickname.SetValueWithoutNotify(allowed);

            RefreshLocks();
        }

        private void RefreshLocks()
        {
            var locked = !NicknameRules.IsValid(_nickname.value.Trim());
            _createRoom.EnableInClassList(Locked, locked);
            _joinRoom.EnableInClassList(Locked, locked);
        }

        private void Proceed(Signal<Unit> chosen)
        {
            var nickname = _nickname.value.Trim();
            if (!NicknameRules.IsValid(nickname))
                return;

            _profile.SaveNickname(nickname);
            chosen.Fire(Unit.Instance);
        }
    }
}
