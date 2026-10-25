using System.Collections.Generic;
using Bw.Entities.Simulation;
using NUnit.Framework;

namespace Bw.Tests.Entities.Simulation
{
    public sealed class CompositeStateViewTests
    {
        [Test]
        public void EveryViewShowsTheSameFrameInOrder()
        {
            var shown = new List<(string, int, int, float)>();
            var view = new CompositeStateView<int>(new RecordingView("first", shown), new RecordingView("second", shown));

            view.Show(1, 2, 0.5f);

            Assert.That(shown, Is.EqualTo(new[] { ("first", 1, 2, 0.5f), ("second", 1, 2, 0.5f) }));
        }

        [Test]
        public void CompositeOfNoViewsShowsNothing()
        {
            Assert.DoesNotThrow(() => new CompositeStateView<int>().Show(1, 2, 0.5f));
        }

        [Test]
        public void CompositeCanHoldAnotherComposite()
        {
            var shown = new List<(string, int, int, float)>();
            var inner = new CompositeStateView<int>(new RecordingView("inner", shown));
            var view = new CompositeStateView<int>(inner, new RecordingView("outer", shown));

            view.Show(3, 3, 0f);

            Assert.That(shown, Is.EqualTo(new[] { ("inner", 3, 3, 0f), ("outer", 3, 3, 0f) }));
        }

        private sealed class RecordingView : IStateView<int>
        {
            private readonly string _name;
            private readonly List<(string, int, int, float)> _shown;

            public RecordingView(string name, List<(string, int, int, float)> shown)
            {
                _name = name;
                _shown = shown;
            }

            public void Show(int from, int to, float progress) =>
                _shown.Add((_name, from, to, progress));
        }
    }
}
