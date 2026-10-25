using System.Collections.Generic;
using Bw.Entities.Network.Prediction.Events;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network.Prediction.Events
{
    public sealed class StateCauseTests
    {
        [Test]
        public void NoneSwitchesToNone()
        {
            Assert.That(Described(StateCause.None), Is.EqualTo("none"));
        }

        [Test]
        public void DefaultCauseIsNone()
        {
            Assert.That(Described(default), Is.EqualTo("none"));
        }

        [Test]
        public void ActionCauseSwitchesToItsAction()
        {
            Assert.That(Described(StateCause.Of(new ActionId(9UL, 33))), Is.EqualTo("9@33"));
        }

        [Test]
        public void ActionOfInitiatorZeroAtTickZeroIsStillAnAction()
        {
            Assert.That(Described(StateCause.Of(default)), Is.EqualTo("0@0"));
        }

        [Test]
        public void SwitchPassesItsStateToTheBranch()
        {
            var seen = new List<string>();

            StateCause.None.Switch(seen, static list => list.Add("none"), static (list, _) => list.Add("action"));
            StateCause.Of(new ActionId(1UL, 2)).Switch(seen, static list => list.Add("none"), static (list, _) => list.Add("action"));

            Assert.That(seen, Is.EqualTo(new[] { "none", "action" }));
        }

        public static string Described(StateCause cause)
        {
            var described = new List<string>(1);
            cause.Switch(
                described,
                static list => list.Add("none"),
                static (list, action) => list.Add($"{action.Initiator.ToString()}@{action.Tick.ToString()}"));
            return described[0];
        }
    }
}
