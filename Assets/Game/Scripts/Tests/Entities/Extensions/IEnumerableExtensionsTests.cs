using System;
using System.Collections.Generic;
using Bw.Entities.Extensions;
using NUnit.Framework;

namespace Bw.Tests.Entities.Extensions
{
    public sealed class IEnumerableExtensionsTests
    {
        [Test]
        public void ForEachVisitsEveryItemInOrder()
        {
            var visited = new List<int>();

            new[] { 3, 1, 2 }.ForEach(visited.Add);

            Assert.That(visited, Is.EqualTo(new[] { 3, 1, 2 }));
        }

        [Test]
        public void ForEachOfNothingVisitsNothing()
        {
            var visited = new List<int>();

            Array.Empty<int>().ForEach(visited.Add);

            Assert.That(visited, Is.Empty);
        }

        [Test]
        public void ForEachStopsAtAThrowingAction()
        {
            var visited = new List<int>();

            Assert.Throws<InvalidOperationException>(() => new[] { 1, 2, 3 }.ForEach(item =>
            {
                visited.Add(item);
                if (item == 2)
                    throw new InvalidOperationException();
            }));
            Assert.That(visited, Is.EqualTo(new[] { 1, 2 }));
        }
    }
}
