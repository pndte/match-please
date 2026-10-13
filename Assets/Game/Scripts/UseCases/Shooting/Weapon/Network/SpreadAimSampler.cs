using Bw.Entities.Network.Prediction.Stream;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using Bw.UseCases.Shooting.Weapon.Extensions;

namespace Bw.UseCases.Shooting.Weapon.Network
{
    public sealed class SpreadAimSampler : IInputSampler<WeaponInput>
    {
        private readonly IInputSampler<WeaponInput> _sampler;
        private readonly IAimDeflection _deflection;

        public SpreadAimSampler(IInputSampler<WeaponInput> sampler, IAimDeflection deflection)
        {
            _sampler = sampler;
            _deflection = deflection;
        }

        public WeaponInput Sample() =>
            _sampler.Sample().Deflected(_deflection.Current);
    }
}
