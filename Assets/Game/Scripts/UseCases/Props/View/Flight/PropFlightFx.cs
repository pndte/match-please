using UnityEngine;

namespace Bw.UseCases.Props.View.Flight
{
    public sealed class PropFlightFx : MonoBehaviour
    {
        public TrailRenderer Trail => _trail;
        public ParticleSystem Lines => _lines;
        public ParticleSystem Ghosts => _ghosts;

        [SerializeField] private TrailRenderer _trail;
        [SerializeField] private ParticleSystem _lines;
        [SerializeField] private ParticleSystem _ghosts;
    }
}
