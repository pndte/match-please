using Bw.Entities.Infrastructure;
using JetBrains.Diagnostics;
using NUnit.Framework;

namespace Bw.Tests
{
    [SetUpFixture]
    public sealed class TestLogging
    {
        private ILogFactory _previous;

        [OneTimeSetUp]
        public void RouteToUnity()
        {
            _previous = Log.DefaultFactory;
            Log.DefaultFactory = new UnityLogFactory();
        }

        [OneTimeTearDown]
        public void Restore() =>
            Log.DefaultFactory = _previous;
    }
}
