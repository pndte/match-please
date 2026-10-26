using Cysharp.Threading.Tasks;
using JetBrains.Lifetimes;
using Setup.Launch.Menu;

namespace Setup.Launch
{
    public sealed class StartMenu
    {
        private readonly MainScreen _main;
        private readonly CreateRoomScreen _create;
        private readonly JoinRoomScreen _join;
        private readonly CharacterSelectScreen _characters;

        public StartMenu(
            MainScreen main,
            CreateRoomScreen create,
            JoinRoomScreen join,
            CharacterSelectScreen characters)
        {
            _main = main;
            _create = create;
            _join = join;
            _characters = characters;
        }

        public UniTask<LaunchChoice> Choose(Lifetime lifetime)
        {
            var chosen = new UniTaskCompletionSource<LaunchChoice>();
            var joins = new SequentialLifetimes(lifetime);
            _main.CreateChosen.Advise(lifetime, _ => _create.Show());
            _main.JoinChosen.Advise(lifetime, _ => _join.Show());
            _create.BackChosen.Advise(lifetime, _ => _main.Show());
            _create.PortChosen.Advise(lifetime, port => chosen.TrySetResult(RoomChoice(port)));
            _join.BackChosen.Advise(lifetime, _ => _main.Show());
            _join.ServerChosen.Advise(lifetime, server => PickCharacter(joins.Next(), server, chosen));
            _characters.BackChosen.Advise(lifetime, _ => _join.Show());
            _main.Show();
            return chosen.Task;
        }

        private void PickCharacter(Lifetime joined, LaunchChoice.Client server, UniTaskCompletionSource<LaunchChoice> chosen)
        {
            _characters.CharacterChosen.Advise(joined, _ => chosen.TrySetResult(server));
            _characters.Show(server);
        }

        private static LaunchChoice RoomChoice(ushort port) =>
            new LaunchChoice.Room(port);
    }
}
