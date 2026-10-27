using System;
using System.Collections.Generic;
using Bw.UseCases.Bots;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Bots
{
    public sealed class BotNamesTests
    {
        private static readonly string[] Names = { "Пельмень", "Батон", "Кефир" };

        [Test]
        public void PickedNameIsFree()
        {
            var taken = new HashSet<string> { "Пельмень", "Батон" };

            Assert.That(BotNames.Pick(Names, taken, new Random(1)), Is.EqualTo("Кефир"));
        }

        [Test]
        public void PickedNameComesFromTheList()
        {
            Assert.That(Names, Does.Contain(BotNames.Pick(Names, new HashSet<string>(), new Random(7))));
        }

        [Test]
        public void NoFreeNameThrows()
        {
            Assert.Throws<InvalidOperationException>(() => BotNames.Pick(Names, new HashSet<string>(Names), new Random(1)));
        }
    }
}
