using Cysharp.Threading.Tasks;
using JetBrains.Lifetimes;
using Setup.Launch.Menu;
using Unity.Netcode;

namespace Setup.Launch
{
    public sealed class RoomMenu
    {
        private readonly Lifetime _lifetime;
        private readonly RoomScreen _room;
        private readonly CreateRoomScreen _create;

        public RoomMenu(Lifetime lifetime, RoomScreen room, CreateRoomScreen create)
        {
            _lifetime = lifetime;
            _room = room;
            _create = create;

            room.CloseChosen.Advise(lifetime, _ => GameRestarter.ReturnToMenu());
        }

        public void ShowOpening(ushort port) =>
            _room.Show(port);

        public UniTask<ushort> RetryPort(Lifetime lifetime, ushort busyPort)
        {
            var retried = new UniTaskCompletionSource<ushort>();
            _create.BackChosen.Advise(lifetime, _ => GameRestarter.ReturnToMenu());
            _create.PortChosen.Advise(lifetime, port => retried.TrySetResult(port));
            _create.ShowBusy(busyPort);
            return retried.Task;
        }

        public void ShowOpen(NetworkManager manager, ushort port)
        {
            _room.Show(port);
            manager.AdviseConnectionEvents(_lifetime, _ => _room.ShowPlayers(manager.ConnectedClientsIds.Count));
        }
    }
}
