using JetBrains.Collections.Viewable;

namespace Bw.Injection.Network
{
    public class TestPropertyScript
    {
        public readonly IViewableProperty<int> Health;

        public TestPropertyScript(IViewableProperty<int> health)
        {
            Health = health;
        }
    }
}