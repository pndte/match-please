namespace Bw.UseCases.Combat.View.Crosshair.Abstractions
{
    public interface IReloadTimer
    {
        public float Seconds { get; }
        public float SecondsLeft { get; }
    }
}
