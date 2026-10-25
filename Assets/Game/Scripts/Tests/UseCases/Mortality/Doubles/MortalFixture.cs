using System;
using Bw.Entities;
using Bw.UseCases.Mortality;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bw.Tests.UseCases.Mortality.Doubles
{
    public sealed class MortalFixture : IDisposable
    {
        public Health Health { get; }
        public Mortal Mortal { get; }

        private readonly HealthConfig _config;

        public MortalFixture(float max = 100f)
        {
            _config = ScriptableObject.CreateInstance<HealthConfig>();
            _config.Max = max;
            Health = new Health(_config);
            Mortal = new Mortal(Health);
        }

        public void Dispose() =>
            Object.DestroyImmediate(_config);
    }
}
