namespace Bw.Entities.Network.Prediction.Events
{
    public interface IAffectable<TEffect> where TEffect : struct
    {
        public void Affect(ActionId action, TEffect effect);
    }
}
