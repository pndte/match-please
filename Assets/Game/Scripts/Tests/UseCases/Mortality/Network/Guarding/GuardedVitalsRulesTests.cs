using System.Collections.Generic;
using Bw.Entities;
using Bw.Entities.Network.Prediction.Events;
using Bw.Tests.UseCases.Mortality.Doubles;
using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Extensions;
using Bw.UseCases.Mortality.Guarding;
using Bw.UseCases.Mortality.Network.Guarding;
using Bw.UseCases.Mortality.Network.Prediction;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Mortality.Network.Guarding
{
    public sealed class GuardedVitalsRulesTests
    {
        private readonly RightSideCover _cover = new();

        [Test]
        public void UncoveredBlowsReachTheVitals()
        {
            var rules = new GuardedVitalsRules(new VitalsEffectRules(), _cover);

            var vitals = rules.Apply(VitalsOf.Alive(100f), Blows.Of(Blows.At(-1f, 0f, 30f)));

            Assert.That(vitals.Health, Is.EqualTo(new HealthState(70f, -30f)));
        }

        [Test]
        public void CoveredBlowsNeverReachTheVitals()
        {
            var vitals = new RecordedRules();
            var rules = new GuardedVitalsRules(vitals, _cover);

            var after = rules.Apply(VitalsOf.Alive(100f, -5f), Blows.Of(Blows.At(1f, 0f, 30f), Blows.At(2f, 0f, 30f)));

            Assert.That(vitals.Applied, Is.Empty);
            Assert.That(after.Health, Is.EqualTo(new HealthState(100f, 0f)));
            Assert.That(after.State, Is.EqualTo(LifeState.Alive));
        }

        [Test]
        public void SplitBuckshotPassesOnlyTheUncoveredPellets()
        {
            var vitals = new RecordedRules();
            var rules = new GuardedVitalsRules(vitals, _cover);

            rules.Apply(VitalsOf.Alive(100f), Blows.Of(Blows.At(1f, 0f, 10f), Blows.At(-1f, 0f, 20f), Blows.At(-2f, 0f, 40f)));

            Assert.That(vitals.Applied, Has.Count.EqualTo(1));
            Assert.That(vitals.Applied[0].Count, Is.EqualTo(2));
            Assert.That(vitals.Applied[0].Damage(), Is.EqualTo(60f));
        }

        [Test]
        public void LethalUncoveredPelletsStillKill()
        {
            var rules = new GuardedVitalsRules(new VitalsEffectRules(), _cover);

            var vitals = rules.Apply(VitalsOf.Alive(25f), Blows.Of(Blows.At(1f, 0f, 30f), Blows.At(-1f, 0f, 30f)));

            Assert.That(vitals.State, Is.EqualTo(LifeState.Dead));
            Assert.That(vitals.Health, Is.EqualTo(new HealthState(0f, -25f)));
        }

        [Test]
        public void FullyCoveredBlowOnTheDeadLeavesThemDead()
        {
            var rules = new GuardedVitalsRules(new VitalsEffectRules(), _cover);

            var vitals = rules.Apply(VitalsOf.Dead(0f, -10f), Blows.Of(Blows.At(1f, 0f, 30f)));

            Assert.That(vitals.State, Is.EqualTo(LifeState.Dead));
            Assert.That(vitals.Health.Change, Is.Zero);
        }

        private sealed class RightSideCover : IGuardCover
        {
            public bool InCover(Contact contact) =>
                contact.Point.x > 0f;
        }

        private sealed class RecordedRules : IEffectRules<Vitals, Impacts>
        {
            public List<Impacts> Applied { get; } = new();

            public Vitals Apply(Vitals state, Impacts effect)
            {
                Applied.Add(effect);
                return state;
            }
        }
    }
}
