using System;
using Bw.Entities.Network;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network.Room
{
    public sealed class LevelQueryTests
    {
        [Test]
        public void QuestionIsTold()
        {
            Assert.That(LevelQuery.IsAsked(LevelQuery.Payload), Is.True);
        }

        [Test]
        public void ConnectionRequestIsNoQuestion()
        {
            var request = ConnectionRequestPayload.Of(new ConnectionRequest("Сева", 4, Guid.NewGuid(), Guid.Empty));

            Assert.That(LevelQuery.IsAsked(request), Is.False);
        }

        [Test]
        public void AnswerNamesTheLevel()
        {
            Assert.That(LevelQuery.TryReadAnswer(LevelQuery.Answer("MeatPlant"), out var level), Is.True);
            Assert.That(level, Is.EqualTo("MeatPlant"));
        }

        [TestCase("")]
        [TestCase(null)]
        [TestCase("Disconnected by the server")]
        [TestCase("level:")]
        public void OtherReasonsAreNoAnswer(string reason)
        {
            Assert.That(LevelQuery.TryReadAnswer(reason, out _), Is.False);
        }
    }
}
