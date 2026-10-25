using System;
using System.Collections.Generic;
using Bw.Entities.Network.Ticks;
using Bw.Tests.Doubles;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.Entities.Network.Ticks
{
    public sealed class InputDelayLogTests
    {
        private LifetimeDefinition _lifetime;
        private Delay _delay;
        private List<string> _logged;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _delay = new Delay();
            _logged = new List<string>();
            Application.logMessageReceived += Remember;
        }

        [TearDown]
        public void TearDown()
        {
            Application.logMessageReceived -= Remember;
            _lifetime.Terminate();
        }

        [Test]
        public void CurrentDelayIsReportedInTicksAndMilliseconds()
        {
            new InputDelayLog(_lifetime.Lifetime, _delay, new ManualClock(0.25f));

            Assert.That(_logged, Is.EqualTo(new[] { "[Input] own input applies 1 ticks after it is sampled (250 ms)." }));
        }

        [Test]
        public void EveryChangeIsReported()
        {
            new InputDelayLog(_lifetime.Lifetime, _delay, new ManualClock(1f / 30f));

            _delay.Set(3);

            Assert.That(_logged, Is.EqualTo(new[]
            {
                "[Input] own input applies 1 ticks after it is sampled (33 ms).",
                "[Input] own input applies 3 ticks after it is sampled (100 ms)."
            }));
        }

        [Test]
        public void EndedLogReportsNothing()
        {
            var log = new LifetimeDefinition();
            new InputDelayLog(log.Lifetime, _delay, new ManualClock(0.25f));
            log.Terminate();

            _delay.Set(2);

            Assert.That(_logged, Has.Count.EqualTo(1));
        }

        private void Remember(string message, string stackTrace, LogType type)
        {
            if (message.StartsWith("[Input]", StringComparison.Ordinal))
                _logged.Add(message);
        }

        private sealed class Delay : IInputDelay
        {
            public IReadonlyProperty<int> Ticks => _ticks;

            private readonly ViewableProperty<int> _ticks = new(1);

            public void Set(int ticks) =>
                _ticks.Value = ticks;
        }
    }
}
