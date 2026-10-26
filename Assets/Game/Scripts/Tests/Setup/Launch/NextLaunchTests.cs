using NUnit.Framework;
using Setup.Launch;

namespace Bw.Tests.Setup.Launch
{
    public sealed class NextLaunchTests
    {
        [Test]
        public void ProcessStartsWithTheInitialLaunch()
        {
            var next = new NextLaunch();

            Assert.That(KindOf(next), Is.EqualTo("initial"));
        }

        [Test]
        public void ReturnToTheMenuOpensTheMenuNext()
        {
            var next = new NextLaunch();

            next.OpenMenu();

            Assert.That(KindOf(next), Is.EqualTo("menu"));
        }

        [Test]
        public void RelaunchStartsWithTheSameChoice()
        {
            var next = new NextLaunch();
            var choice = new LaunchChoice.Server(7777);

            next.StartWith(choice);

            Assert.That(next.Switch(() => null, () => null, chosen => chosen), Is.SameAs(choice));
        }

        [Test]
        public void LaterRequestReplacesTheEarlierOne()
        {
            var next = new NextLaunch();

            next.StartWith(new LaunchChoice.Server(7777));
            next.OpenMenu();

            Assert.That(KindOf(next), Is.EqualTo("menu"));
        }

        private static string KindOf(NextLaunch next) =>
            next.Switch(() => "initial", () => "menu", _ => "chosen");
    }
}
