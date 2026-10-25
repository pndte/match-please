using System;
using Bw.Entities.Simulation;

namespace Bw.Tests.Doubles
{
    public sealed class CountingSteps : ISteppable
    {
        public int Count { get; private set; }

        private readonly Action _onStep;

        public CountingSteps(Action onStep = null)
        {
            _onStep = onStep;
        }

        public void Step()
        {
            Count++;
            _onStep?.Invoke();
        }
    }
}
