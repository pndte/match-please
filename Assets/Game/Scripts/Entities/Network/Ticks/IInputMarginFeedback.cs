namespace Bw.Entities.Network.Ticks
{
    public interface IInputMarginFeedback
    {
        public void Report(int marginTicks);
    }
}
