using System;
using Bw.UseCases;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.UseCases
{
    public sealed class ControlledByTests
    {
        [Test]
        public void ObjectIsNotControlledByMeAtFirst()
        {
            Assert.That(new ControlledBy().Me.Value, Is.False);
        }

        [Test]
        public void ControlIsMineForTheGivenLifetime()
        {
            var controlledBy = new ControlledBy();
            var control = new LifetimeDefinition();

            controlledBy.SetMe(control.Lifetime);

            Assert.That(controlledBy.Me.Value, Is.True);

            control.Terminate();

            Assert.That(controlledBy.Me.Value, Is.False);
        }

        [Test]
        public void ControlIsGivenOnceAtATime()
        {
            var controlledBy = new ControlledBy();
            controlledBy.SetMe(new LifetimeDefinition().Lifetime);

            Assert.Throws<InvalidOperationException>(() => controlledBy.SetMe(new LifetimeDefinition().Lifetime));
        }

        [Test]
        public void ControlIsGivenAgainAfterThePreviousEnds()
        {
            var controlledBy = new ControlledBy();
            var first = new LifetimeDefinition();
            controlledBy.SetMe(first.Lifetime);
            first.Terminate();

            controlledBy.SetMe(new LifetimeDefinition().Lifetime);

            Assert.That(controlledBy.Me.Value, Is.True);
        }

        [Test]
        public void ControlForAnEndedLifetimeIsRefusedWithoutTakingControl()
        {
            var controlledBy = new ControlledBy();

            Assert.Throws<InvalidOperationException>(() => controlledBy.SetMe(Lifetime.Terminated));
            Assert.That(controlledBy.Me.Value, Is.False);
        }
    }
}
