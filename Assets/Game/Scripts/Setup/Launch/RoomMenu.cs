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
        private readonly GameRestarter _restarter;

        public RoomMenu(
            Lifetime lifetime,
            RoomScreen room,
            CreateRoomScreen create,
            GameRestarter restarter)
        {
            _lifetime = lifetime;
            _room = room;
            _create = create;
            _restarter = restarter;

            room.CloseChosen.Advise(lifetime, _ => restarter.ReturnToMenu());
        }

        public void ShowOpening(ushort port) =>
            _room.Show(port);

        public UniTask<LaunchChoice.Room> RetryRoom(Lifetime lifetime, ushort busyPort)
        {
            var retried = new UniTaskCompletionSource<LaunchChoice.Room>();
            _create.BackChosen.Advise(lifetime, _ => _restarter.ReturnToMenu());
            _create.RoomChosen.Advise(lifetime, room => retried.TrySetResult(room));
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
