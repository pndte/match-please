using Bw.UseCases.Combat.Weapon.Extensions;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Combat.Weapon.Extensions
{
    public sealed class WeaponAimExtensionsTests
    {
        [TestCase(0f, 90f, 20f, 20f)]
        [TestCase(80f, 90f, 20f, 90f)]
        [TestCase(0f, -90f, 20f, -20f)]
        [TestCase(170f, -170f, 30f, -170f)]
        [TestCase(-170f, 170f, 30f, 170f)]
        [TestCase(350f, 10f, 5f, -5f)]
        [TestCase(720f, 0f, 5f, 0f)]
        [TestCase(30f, 30f, 10f, 30f)]
        [TestCase(0f, -90f, 0f, 0f)]
        [TestCase(-100f, 100f, 30f, -130f)]
        public void AimTurnsTheShortWayByAtMostTheStep(
            float aim,
            float target,
            float step,
            float turned)
        {
            Assert.That(aim.TurnedTowards(target, step), Is.EqualTo(turned).Within(1e-4f));
        }

        [Test]
        public void AimTurnedOntoTheBackOfTheCircleIsHalfATurn()
        {
            Assert.That(Mathf.Abs(170f.TurnedTowards(-170f, 10f)), Is.EqualTo(180f).Within(1e-4f));
        }

        [Test]
        public void TurnedAimStaysWithinHalfATurnEitherWay()
        {
            for (var aim = -720f; aim <= 720f; aim += 37f)
                for (var target = -540f; target <= 540f; target += 53f)
                    Assert.That(aim.TurnedTowards(target, 25f), Is.InRange(-180f, 180f));
        }
    }
}
