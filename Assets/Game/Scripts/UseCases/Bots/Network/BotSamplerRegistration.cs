using Bw.Entities.Extensions;
using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Players;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Bots.Network
{
    public sealed class BotSamplerRegistration<TInput> where TInput : struct
    {
        public BotSamplerRegistration(
            Lifetime lifetime,
            IPlayer player,
            IControllerSamplers<TInput> samplers,
            IInputSampler<TInput> sampler)
        {
            samplers.AddLifetimed(lifetime, player, sampler);
        }
    }
}
