using System.Collections.Generic;

namespace Bw.Entities.Network.Prediction.Stream
{
    public interface IInputAudience
    {
        public void Collect(List<IClient> clients);
    }
}
