using System.Collections.Generic;
using Bw.UseCases.Match;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Match
{
    public sealed class KillFeedTests
    {
        private LifetimeDefinition _lifetime;
        private KillFeed _feed;
        private List<KillNote> _reported;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _feed = new KillFeed();
            _reported = new List<KillNote>();
            _feed.Reported.Advise(_lifetime.Lifetime, kill => _reported.Add(kill));
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void ReportedKillIsAnnounced()
        {
            _feed.Report(new KillNote(new KillParty("Сева", true), new KillParty("Гошак", false), null));

            Assert.That(_reported, Has.Count.EqualTo(1));
            Assert.That(_reported[0].Killer.Nickname, Is.EqualTo("Сева"));
            Assert.That(_reported[0].Killer.IsMine, Is.True);
            Assert.That(_reported[0].Victim.Nickname, Is.EqualTo("Гошак"));
            Assert.That(_reported[0].Victim.IsMine, Is.False);
        }

        [Test]
        public void EveryKillIsAnnouncedInOrder()
        {
            _feed.Report(new KillNote(new KillParty("a", false), new KillParty("b", false), null));
            _feed.Report(new KillNote(new KillParty("c", false), new KillParty("d", false), null));

            Assert.That(_reported.ConvertAll(kill => kill.Killer.Nickname), Is.EqualTo(new[] { "a", "c" }));
        }
    }
}
