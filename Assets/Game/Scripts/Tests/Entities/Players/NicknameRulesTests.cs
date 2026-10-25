using Bw.Entities.Players;
using NUnit.Framework;

namespace Bw.Tests.Entities.Players
{
    public sealed class NicknameRulesTests
    {
        [TestCase("Саня")]
        [TestCase("abc")]
        [TestCase("Player_One-2")]
        [TestCase("Ёжик в тумане")]
        [TestCase("1234567890123456")]
        public void AllowedNicknameIsValid(string nickname)
        {
            Assert.That(NicknameRules.IsValid(nickname), Is.True);
        }

        [TestCase("ab")]
        [TestCase("")]
        [TestCase("12345678901234567")]
        [TestCase(" Саня")]
        [TestCase("Саня ")]
        [TestCase("Sanya!")]
        [TestCase("日本語名")]
        [TestCase("tab\tname")]
        public void NicknameOutsideTheRulesIsInvalid(string nickname)
        {
            Assert.That(NicknameRules.IsValid(nickname), Is.False);
        }

        [Test]
        public void AcceptedNicknameIsTrimmed()
        {
            Assert.That(NicknameRules.Accepted("  Сева  "), Is.EqualTo("Сева"));
        }

        [Test]
        public void LengthIsCountedAfterTrimming()
        {
            Assert.That(NicknameRules.Accepted("  1234567890123456  "), Is.EqualTo("1234567890123456"));
        }

        [Test]
        public void AcceptedNicknameKeepsItsInnerSpaces()
        {
            Assert.That(NicknameRules.Accepted("Ёжик в тумане"), Is.EqualTo("Ёжик в тумане"));
        }

        [TestCase("x")]
        [TestCase("   ")]
        [TestCase("<script>")]
        public void InvalidRequestFallsBackToTheDefaultName(string requested)
        {
            Assert.That(NicknameRules.Accepted(requested), Is.EqualTo(NicknameRules.Fallback));
        }

        [Test]
        public void FallbackItselfIsValid()
        {
            Assert.That(NicknameRules.IsValid(NicknameRules.Fallback), Is.True);
        }

        [TestCase('a', true)]
        [TestCase('Z', true)]
        [TestCase('5', true)]
        [TestCase(' ', true)]
        [TestCase('_', true)]
        [TestCase('-', true)]
        [TestCase('Ѐ', true)]
        [TestCase('џ', true)]
        [TestCase('.', false)]
        [TestCase('@', false)]
        [TestCase('Ѡ', false)]
        [TestCase('A', true)]
        [TestCase('z', true)]
        [TestCase('0', true)]
        [TestCase('9', true)]
        [TestCase('/', false)]
        [TestCase(':', false)]
        [TestCase('[', false)]
        [TestCase('`', false)]
        [TestCase('{', false)]
        [TestCase('Ͽ', false)]
        public void SymbolIsAllowedOnlyFromTheLatinCyrillicSet(char symbol, bool allowed)
        {
            Assert.That(NicknameRules.IsAllowed(symbol), Is.EqualTo(allowed));
        }
    }
}
