using Bw.Entities.Pool;
using Bw.UseCases.Shooting.View.Impact.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Shooting.View.Impact
{
    public sealed class ParticleImpactEffect : MonoBehaviour, IResource<IImpactEffect>
    {
        [SerializeField] private ParticleSystem _particles;

        public IImpactEffect Facade(Lifetime lifetime) =>
            new ParticleImpactPlayback(lifetime, transform, _particles);
    }
}
