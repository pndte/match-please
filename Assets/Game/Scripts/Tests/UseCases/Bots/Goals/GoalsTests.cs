using Bw.UseCases.Bots.Goals;
using Bw.UseCases.Bots.Goap;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Bots.Goals
{
    public sealed class GoalsTests
    {
        private const BotFact Danger = BotFact.Hurt | BotFact.TargetNoticed | BotFact.PropNearby;

        [Test]
        public void KillGoalWantsTheTargetDown()
        {
            Assert.That(new KillGoal().Wanted, Is.EqualTo(BotFact.TargetDown));
        }

        [TestCase(BotFact.None, 0f)]
        [TestCase(BotFact.TargetInSight, 0f)]
        [TestCase(BotFact.TargetNoticed, 1f)]
        [TestCase(BotFact.TargetNoticed | BotFact.Hurt, 1f)]
        public void KillGoalMattersOnlyOnceATargetIsNoticed(BotFact facts, float priority)
        {
            Assert.That(new KillGoal().PriorityIn(facts), Is.EqualTo(priority));
        }

        [Test]
        public void GuardGoalWantsAPropInHands()
        {
            Assert.That(new GuardGoal().Wanted, Is.EqualTo(BotFact.PropHeld));
        }

        [TestCase(Danger, 1.5f)]
        [TestCase(Danger | BotFact.TargetInSight, 1.5f)]
        [TestCase(BotFact.Hurt | BotFact.TargetNoticed, 0f)]
        [TestCase(BotFact.Hurt | BotFact.PropNearby, 0f)]
        [TestCase(BotFact.TargetNoticed | BotFact.PropNearby, 0f)]
        [TestCase(BotFact.None, 0f)]
        public void GuardGoalOutranksKillingOnlyWhenHurtNoticedAndNearAProp(BotFact facts, float priority)
        {
            Assert.That(new GuardGoal().PriorityIn(facts), Is.EqualTo(priority));
        }

        [Test]
        public void GuardOutranksKillWhenItMatters()
        {
            Assert.That(new GuardGoal().PriorityIn(Danger), Is.GreaterThan(new KillGoal().PriorityIn(Danger)));
        }

        [Test]
        public void RoamGoalWantsARoam()
        {
            Assert.That(new RoamGoal().Wanted, Is.EqualTo(BotFact.Roamed));
        }

        [TestCase(BotFact.None)]
        [TestCase(BotFact.TargetNoticed)]
        [TestCase(Danger)]
        public void RoamGoalIsAlwaysTheLowestWish(BotFact facts)
        {
            var roam = new RoamGoal().PriorityIn(facts);

            Assert.That(roam, Is.EqualTo(0.1f));
            Assert.That(roam, Is.LessThan(new KillGoal().PriorityIn(facts | BotFact.TargetNoticed)));
        }
    }
}
