using Bw.UseCases.Level.View.Abstractions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Level.View.Lighting
{
    public sealed class LampLightView
    {
        public LampLightView(Lifetime lifetime, ILampLightViewModel model, ILampLight light) =>
            model.Live.Advise(lifetime, live =>
            {
                if (live)
                    light.ShowLive();
                else
                    light.ShowBaked();
            });
    }
}
