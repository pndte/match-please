using System.Collections.Generic;
using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Rollback;
using Bw.Tests.Entities.Network.Prediction.Doubles;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network.Prediction.Stream
{
    public sealed class PredictedInputsTests
    {
        private Toggle _predicted;
        private Toggle _controlled;
        private TickHistory<int> _own;
        private ScriptedRemote _remote;
        private PredictedInputs<int> _inputs;

        [SetUp]
        public void SetUp()
        {
            _predicted = new Toggle();
            _controlled = new Toggle();
            _own = new TickHistory<int>(8);
            _remote = new ScriptedRemote();
            _inputs = new PredictedInputs<int>(_predicted, _controlled, _own, _remote);
            _own.Record(5, 50);
            _remote.ByTick[5] = 500;
        }

        [Test]
        public void ObjectNotPredictedTakesNoInput()
        {
            _controlled.Set(true);

            var taken = _inputs.TryTakeInputAt(5, out var input);

            Assert.That(taken, Is.False);
            Assert.That(input, Is.Zero);
            Assert.That(_remote.Asked, Is.Empty);
        }

        [Test]
        public void ObjectControlledHereTakesTheOwnInput()
        {
            _predicted.Set(true);
            _controlled.Set(true);

            var taken = _inputs.TryTakeInputAt(5, out var input);

            Assert.That(taken, Is.True);
            Assert.That(input, Is.EqualTo(50));
            Assert.That(_remote.Asked, Is.Empty);
        }

        [Test]
        public void OwnTickWithoutAnInputTakesNothing()
        {
            _predicted.Set(true);
            _controlled.Set(true);

            Assert.That(_inputs.TryTakeInputAt(6, out _), Is.False);
            Assert.That(_remote.Asked, Is.Empty);
        }

        [Test]
        public void ObjectControlledElsewhereTakesTheRemoteInput()
        {
            _predicted.Set(true);

            var taken = _inputs.TryTakeInputAt(5, out var input);

            Assert.That(taken, Is.True);
            Assert.That(input, Is.EqualTo(500));
            Assert.That(_remote.Asked, Is.EqualTo(new[] { 5 }));
        }

        [Test]
        public void RemoteTickWithoutAnInputTakesNothing()
        {
            _predicted.Set(true);

            Assert.That(_inputs.TryTakeInputAt(6, out _), Is.False);
        }

        private sealed class ScriptedRemote : IRemoteInputs<int>
        {
            public Dictionary<int, int> ByTick { get; } = new();
            public List<int> Asked { get; } = new();

            public bool TryTakeInputAt(int tick, out int input)
            {
                Asked.Add(tick);
                return ByTick.TryGetValue(tick, out input);
            }
        }
    }
}
