using Bw.UseCases.Movement;
using Bw.UseCases.Movement.Extensions;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Movement.Extensions
{
    public sealed class MovementInputExtensionsTests
    {
        [Test]
        public void WithoutJumpDropsOnlyThePress()
        {
            var input = new MovementInput(-0.5f, true, true, true, new Slowdown(0.25f, 0.5f));

            var withoutJump = input.WithoutJump();

            Assert.That(withoutJump.JumpPressed, Is.False);
            Assert.That(withoutJump.JumpHeld, Is.True);
            Assert.That(withoutJump.Horizontal, Is.EqualTo(-0.5f));
            Assert.That(withoutJump.Down, Is.True);
            Assert.That(withoutJump.Slowdown.Speed, Is.EqualTo(0.25f));
            Assert.That(withoutJump.Slowdown.JumpHeight, Is.EqualTo(0.5f));
        }
    }
}
