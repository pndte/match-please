using System.Collections.Generic;
using Bw.UseCases.Bots.Senses;
using Bw.UseCases.Bots.Senses.Extensions;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Bots.Senses.Extensions
{
    public sealed class SightingExtensionsTests
    {
        [Test]
        public void SeenSightingMovesToThePositionAndCountsTheLook()
        {
            var seen = new Sighting(null, Vector2.zero, false, false, 0, 4).Seen(new Vector2(3f, 1f), 5);

            Assert.That(seen.Position, Is.EqualTo(new Vector2(3f, 1f)));
            Assert.That(seen.Visible, Is.True);
            Assert.That(seen.SeenTicks, Is.EqualTo(1));
            Assert.That(seen.UnseenTicks, Is.Zero);
        }

        [TestCase(1, 3, false)]
        [TestCase(2, 3, true)]
        [TestCase(5, 3, true)]
        [TestCase(0, 1, true)]
        public void SightingIsNoticedOnceSeenForTheReactionTime(int seenTicks, int reaction, bool noticed)
        {
            var seen = new Sighting(null, Vector2.zero, true, false, seenTicks, 0).Seen(Vector2.zero, reaction);

            Assert.That(seen.Noticed, Is.EqualTo(noticed));
        }

        [Test]
        public void NoticedSightingStaysNoticedWhenSeenAgain()
        {
            var seen = new Sighting(null, Vector2.zero, false, true, 0, 9).Seen(Vector2.one, 100);

            Assert.That(seen.Noticed, Is.True);
        }

        [Test]
        public void UnseenSightingKeepsTheLastPositionAndCountsTheMiss()
        {
            var unseen = new Sighting(null, new Vector2(3f, 1f), true, true, 7, 2).Unseen();

            Assert.That(unseen.Position, Is.EqualTo(new Vector2(3f, 1f)));
            Assert.That(unseen.Visible, Is.False);
            Assert.That(unseen.Noticed, Is.True);
            Assert.That(unseen.SeenTicks, Is.Zero);
            Assert.That(unseen.UnseenTicks, Is.EqualTo(3));
        }

        [Test]
        public void NothingNoticedIsNoTarget()
        {
            var sightings = new List<Sighting> { Visible(new Vector2(1f, 0f), false) };

            Assert.That(sightings.TryFindTarget(Vector2.zero, out _), Is.False);
        }

        [Test]
        public void EmptyMemoryIsNoTarget()
        {
            Assert.That(new List<Sighting>().TryFindTarget(Vector2.zero, out _), Is.False);
        }

        [Test]
        public void VisibleSightingBeatsARememberedOne()
        {
            var sightings = new List<Sighting> { Remembered(new Vector2(1f, 0f), 0), Visible(new Vector2(9f, 0f), true) };

            sightings.TryFindTarget(Vector2.zero, out var target);

            Assert.That(target.Position, Is.EqualTo(new Vector2(9f, 0f)));
        }

        [Test]
        public void NearestVisibleSightingIsTheTarget()
        {
            var sightings = new List<Sighting> { Visible(new Vector2(6f, 0f), true), Visible(new Vector2(-2f, 1f), true), Visible(new Vector2(0f, 4f), true) };

            sightings.TryFindTarget(Vector2.zero, out var target);

            Assert.That(target.Position, Is.EqualTo(new Vector2(-2f, 1f)));
        }

        [Test]
        public void FreshestRememberedSightingIsTheTargetWhenNoneIsVisible()
        {
            var sightings = new List<Sighting> { Remembered(new Vector2(1f, 0f), 30), Remembered(new Vector2(20f, 0f), 2), Remembered(new Vector2(3f, 0f), 9) };

            sightings.TryFindTarget(Vector2.zero, out var target);

            Assert.That(target.Position, Is.EqualTo(new Vector2(20f, 0f)));
        }

        [Test]
        public void UnnoticedSightingIsNeverTheTargetHoweverClose()
        {
            var sightings = new List<Sighting> { Visible(new Vector2(0.5f, 0f), false), Visible(new Vector2(8f, 0f), true) };

            sightings.TryFindTarget(Vector2.zero, out var target);

            Assert.That(target.Position, Is.EqualTo(new Vector2(8f, 0f)));
        }

        private static Sighting Visible(Vector2 position, bool noticed) =>
            new(null, position, true, noticed, 1, 0);

        private static Sighting Remembered(Vector2 position, int unseenTicks) =>
            new(null, position, false, true, 0, unseenTicks);
    }
}
