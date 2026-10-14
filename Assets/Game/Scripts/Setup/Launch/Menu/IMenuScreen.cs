using System.Collections.Generic;

namespace Setup.Launch.Menu
{
    public interface IMenuScreen
    {
        public IReadOnlyCollection<string> Layers { get; }
        public void Back();
        public void Confirm();
    }
}
