using Bw.UseCases.Bots.Goals;
using Bw.UseCases.Bots.Goap;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Bots.Goals
{
    public sealed class GoalsTests
    {
        private const BotFact Seen = BotFact.TargetNoticed | BotFact.TargetInSight;
        private const BotFact Everything = Seen | BotFact.TargetWounded | BotFact.TargetReloading | BotFact.Threatened | BotFact.GuardReady | BotFact.Hurt;

        [Test]
        public void HuntingGoalsWantTheTargetDown()
        {
            Assert.That(new KillGoal(1f).Wanted, Is.EqualTo(BotFact.TargetDown));
            Assert.That(new FinishOffGoal(1f).Wanted, Is.EqualTo(BotFact.TargetDown));
            Assert.That(new PunishReloadGoal(1f).Wanted, Is.EqualTo(BotFact.TargetDown));
        }

        [TestCase(BotFact.None, 0f)]
        [TestCase(BotFact.TargetInSight, 0f)]
        [TestCase(BotFact.TargetNoticed, 1f)]
        [TestCase(BotFact.TargetNoticed | BotFact.Hurt, 1f)]
        public void KillGoalMattersOnlyOnceATargetIsNoticed(BotFact facts, float priority)
        {
            Assert.That(new KillGoal(1f).PriorityIn(At(facts)), Is.EqualTo(priority));
        }

        [Test]
        public void WeightScalesTheGoal()
        {
            Assert.That(new KillGoal(1.5f).PriorityIn(At(Seen)), Is.EqualTo(1.5f));
            Assert.That(new KiteGoal(0.5f).PriorityIn(At(Seen)), Is.EqualTo(0.5f));
        }

        [TestCase(BotFact.TargetNoticed | BotFact.TargetWounded, 1.4f)]
        [TestCase(BotFact.TargetNoticed, 0f)]
        [TestCase(BotFact.TargetWounded, 0f)]
        public void FinishOffGoalMattersForAWoundedTarget(BotFact facts, float priority)
        {
            Assert.That(new FinishOffGoal(1f).PriorityIn(At(facts)), Is.EqualTo(priority).Within(1e-5f));
        }

        [TestCase(BotFact.TargetNoticed | BotFact.TargetReloading, 1.6f)]
        [TestCase(BotFact.TargetNoticed, 0f)]
        [TestCase(BotFact.TargetReloading, 0f)]
        public void PunishReloadGoalMattersWhileTheTargetReloads(BotFact facts, float priority)
        {
            Assert.That(new PunishReloadGoal(1f).PriorityIn(At(facts)), Is.EqualTo(priority).Within(1e-5f));
        }

        [Test]
        public void GuardGoalWantsTheAttackParried()
        {
            Assert.That(new GuardGoal(1f).Wanted, Is.EqualTo(BotFact.Parried));
        }

        [TestCase(BotFact.Threatened | BotFact.GuardReady, 1.8f)]
        [TestCase(BotFact.Threatened, 0f)]
        [TestCase(BotFact.GuardReady | BotFact.TargetNoticed, 0f)]
        public void GuardGoalMattersWhenAttackedWithAWholeGuard(BotFact facts, float priority)
        {
            Assert.That(new GuardGoal(1f).PriorityIn(At(facts)), Is.EqualTo(priority).Within(1e-5f));
        }

        [Test]
        public void RetreatGoalWantsARetreat()
        {
            Assert.That(new RetreatGoal(1f).Wanted, Is.EqualTo(BotFact.Retreated));
        }

        [TestCase(BotFact.TargetNoticed, 0f)]
        [TestCase(BotFact.Hurt, 0f)]
        [TestCase(BotFact.TargetNoticed | BotFact.Hurt, 1.9f)]
        public void RetreatGoalMattersWhenHurtWithATargetAround(BotFact facts, float priority)
        {
            Assert.That(new RetreatGoal(1f).PriorityIn(new BotAssessment(facts, 0.3f)), Is.EqualTo(priority).Within(1e-5f));
        }

        [Test]
        public void RetreatGrowsUrgentAsHealthDrops()
        {
            var goal = new RetreatGoal(1f);
            const BotFact facts = BotFact.TargetNoticed | BotFact.Hurt;

            Assert.That(goal.PriorityIn(new BotAssessment(facts, 0.1f)), Is.GreaterThan(goal.PriorityIn(new BotAssessment(facts, 0.4f))));
        }

        [Test]
        public void KiteGoalWantsAKite()
        {
            Assert.That(new KiteGoal(1f).Wanted, Is.EqualTo(BotFact.Kited));
        }

        [TestCase(Seen, 1f)]
        [TestCase(BotFact.TargetNoticed, 0f)]
        public void KiteGoalMattersOnlyWithTheTargetInSight(BotFact facts, float priority)
        {
            Assert.That(new KiteGoal(1f).PriorityIn(At(facts)), Is.EqualTo(priority));
        }

        [Test]
        public void AtEqualWeightsAnAttackOutranksReloadsWoundsAndKilling()
        {
            var assessment = At(Everything);
            var guard = new GuardGoal(1f).PriorityIn(assessment);
            var punish = new PunishReloadGoal(1f).PriorityIn(assessment);
            var finish = new FinishOffGoal(1f).PriorityIn(assessment);
            var kill = new KillGoal(1f).PriorityIn(assessment);

            Assert.That(guard, Is.GreaterThan(punish));
            Assert.That(punish, Is.GreaterThan(finish));
            Assert.That(finish, Is.GreaterThan(kill));
        }

        [Test]
        public void RoamGoalWantsARoam()
        {
            Assert.That(new RoamGoal().Wanted, Is.EqualTo(BotFact.Roamed));
        }

        [TestCase(BotFact.None)]
        [TestCase(BotFact.TargetNoticed)]
        [TestCase(Everything)]
        public void RoamGoalIsAlwaysTheLowestWish(BotFact facts)
        {
            var roam = new RoamGoal().PriorityIn(At(facts));

            Assert.That(roam, Is.EqualTo(0.1f));
            Assert.That(roam, Is.LessThan(new KillGoal(1f).PriorityIn(At(facts | BotFact.TargetNoticed))));
        }

        private static BotAssessment At(BotFact facts) =>
            new(facts, 1f);
    }
}
