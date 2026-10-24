using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Combat.Weapon.Network.Prediction
{
    public interface IStrikes
    {
        public ISource<Strike> Struck { get; }
    }
}
