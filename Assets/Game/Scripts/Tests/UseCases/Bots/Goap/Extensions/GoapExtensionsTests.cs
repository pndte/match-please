using Bw.UseCases.Bots.Goals;
using Bw.UseCases.Bots.Goap;
using Bw.UseCases.Bots.Goap.Extensions;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Bots.Goap.Extensions
{
    public sealed class GoapExtensionsTests
    {
        private const BotFact Seen = BotFact.TargetNoticed | BotFact.TargetInSight;

        [Test]
        public void ConditionIsMetWhenEveryNeededFactHolds()
        {
            var condition = new GoapCondition(Seen, BotFact.None);

            Assert.That(condition.IsMetBy(Seen | BotFact.GunDrawn), Is.True);
            Assert.That(condition.IsMetBy(BotFact.TargetNoticed), Is.False);
        }

        [Test]
        public void ConditionIsNotMetWhenAnyForbiddenFactHolds()
        {
            var condition = new GoapCondition(BotFact.TargetInSight, BotFact.TargetClose | BotFact.PropHeld);

            Assert.That(condition.IsMetBy(BotFact.TargetInSight), Is.True);
            Assert.That(condition.IsMetBy(BotFact.TargetInSight | BotFact.TargetClose), Is.False);
            Assert.That(condition.IsMetBy(BotFact.TargetInSight | BotFact.PropHeld), Is.False);
        }

        [Test]
        public void ConditionWithoutNeedsOrForbidsIsAlwaysMet()
        {
            var condition = new GoapCondition(BotFact.None, BotFact.None);

            Assert.That(condition.IsMetBy(BotFact.None), Is.True);
            Assert.That(condition.IsMetBy(Seen | BotFact.Hurt), Is.True);
        }

        [Test]
        public void EffectGivesFacts()
        {
            var effect = new GoapEffect(BotFact.GunDrawn, BotFact.None);

            Assert.That(effect.AppliedTo(BotFact.TargetNoticed), Is.EqualTo(BotFact.TargetNoticed | BotFact.GunDrawn));
        }

        [Test]
        public void EffectTakesFacts()
        {
            var effect = new GoapEffect(BotFact.GunDrawn, BotFact.FistsDrawn | BotFact.PropHeld);

            Assert.That(effect.AppliedTo(BotFact.FistsDrawn | BotFact.PropHeld | BotFact.Hurt), Is.EqualTo(BotFact.GunDrawn | BotFact.Hurt));
        }

        [Test]
        public void TakingWinsOverGivingTheSameFact()
        {
            var effect = new GoapEffect(BotFact.Roamed, BotFact.Roamed);

            Assert.That(effect.AppliedTo(BotFact.None), Is.EqualTo(BotFact.None));
        }

        [Test]
        public void GoalIsMetWhenAllItsWantedFactsHold()
        {
            var goal = new KillGoal();

            Assert.That(goal.IsMetBy(BotFact.TargetDown | BotFact.Hurt), Is.True);
            Assert.That(goal.IsMetBy(Seen), Is.False);
        }
    }
}
