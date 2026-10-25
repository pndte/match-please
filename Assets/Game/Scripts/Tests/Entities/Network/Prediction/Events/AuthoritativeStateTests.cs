using System;
using System.Collections.Generic;
using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Events;
using Bw.Entities.Network.Prediction.Events.Requests;
using Bw.Entities.Network.Variables;
using Bw.Tests.Entities.Network.Prediction.Doubles;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network.Prediction.Events
{
    public sealed class AuthoritativeStateTests
    {
        private static readonly StateCause Shot = StateCause.Of(new ActionId(4UL, 77));

        private LifetimeDefinition _lifetime;
        private List<string> _order;
        private AppliedValue<int> _state;
        private OrderedResults _results;
        private AuthoritativeState<int, int> _authoritative;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _order = new List<string>();
            _state = new AppliedValue<int>(100);
            _state.Applied.Advise(_lifetime.Lifetime, state => _order.Add($"applied {state.ToString()}"));
            _results = new OrderedResults(_order);
            _authoritative = new AuthoritativeState<int, int>(_state, new Damage(), _results);
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void EffectAppliesTheRulesToTheCurrentState()
        {
            _authoritative.Affect(Shot, 30);
            _authoritative.Affect(Shot, 25);

            Assert.That(_state.History, Is.EqualTo(new[] { 70, 45 }));
        }

        [Test]
        public void NewStateIsBroadcastWithItsCause()
        {
            _authoritative.Affect(Shot, 30);

            Assert.That(_results.Broadcasts, Has.Count.EqualTo(1));
            Assert.That(_results.Broadcasts[0].State, Is.EqualTo(70));
            Assert.That(StateCauseTests.Described(_results.Broadcasts[0].Cause), Is.EqualTo("4@77"));
        }

        [Test]
        public void UncausedChangeIsBroadcastWithoutACause()
        {
            _authoritative.Affect(StateCause.None, 10);

            Assert.That(StateCauseTests.Described(_results.Broadcasts[0].Cause), Is.EqualTo("none"));
        }

        [Test]
        public void StateIsBroadcastBeforeItIsApplied()
        {
            _authoritative.Affect(Shot, 30);

            Assert.That(_order, Is.EqualTo(new[] { "broadcast 70", "applied 70" }));
        }

        [Test]
        public void EffectThatChangesNothingIsStillBroadcast()
        {
            var dead = new AppliedValue<int>(0);
            var results = new OrderedResults(_order);
            var authoritative = new AuthoritativeState<int, int>(dead, new Damage(), results);

            authoritative.Affect(Shot, 30);

            Assert.That(results.Broadcasts, Has.Count.EqualTo(1));
            Assert.That(results.Broadcasts[0].State, Is.EqualTo(0));
            Assert.That(dead.History, Is.EqualTo(new[] { 0 }));
        }

        [Test]
        public void StateIsNeverSentToOneClient()
        {
            _authoritative.Affect(Shot, 30);

            Assert.That(_results.Targeted, Is.Zero);
        }

        private sealed class Damage : IEffectRules<int, int>
        {
            public int Apply(int state, int effect) =>
                Math.Max(0, state - effect);
        }

        private sealed class OrderedResults : INetResultSender<CausedState<int>>
        {
            public List<CausedState<int>> Broadcasts { get; } = new();
            public int Targeted { get; private set; }

            private readonly List<string> _order;

            public OrderedResults(List<string> order)
            {
                _order = order;
            }

            public void Broadcast(CausedState<int> result)
            {
                Broadcasts.Add(result);
                _order.Add($"broadcast {result.State.ToString()}");
            }

            public void SendTo(IClient recipient, CausedState<int> result) =>
                Targeted++;
        }
    }
}
