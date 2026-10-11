namespace Bw.UseCases.Movement.Network.Root
{
    public interface IReadonlyMovementRoot
    {
        public bool RootedAt(int tick);
    }

    public interface IMovementRoot : IReadonlyMovementRoot
    {
        public void Root(int firstTick, int lastTick);
    }
}
