using Bw.UseCases.Level.View.Abstractions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Level.View.Lighting
{
    public sealed class LampHazeView
    {
        public LampHazeView(Lifetime lifetime, ILampHazeViewModel model, ILampHaze haze) =>
            model.Live.Advise(lifetime, live =>
            {
                if (live)
                    haze.ShowLive();
                else
                    haze.ShowBaked();
            });
    }
}
