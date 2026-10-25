using Bw.UseCases.Mortality;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Mortality
{
    public sealed class HitTests
    {
        [Test]
        public void HitAlongADirectionDealsTheConfiguredDamage()
        {
            var hit = Hit.Along(Vector2.right, new HitConfig(25f, 6f));

            Assert.That(hit.Damage, Is.EqualTo(25f));
        }

        [Test]
        public void HitAlongADirectionPushesAlongItByTheConfiguredKnockback()
        {
            var hit = Hit.Along(new Vector2(0.6f, -0.8f), new HitConfig(25f, 5f));

            Assert.That(hit.Knockback.x, Is.EqualTo(3f).Within(1e-5f));
            Assert.That(hit.Knockback.y, Is.EqualTo(-4f).Within(1e-5f));
        }

        private sealed class HitConfig : IHitConfig
        {
            public float Damage { get; }
            public float Knockback { get; }

            public HitConfig(float damage, float knockback)
            {
                Damage = damage;
                Knockback = knockback;
            }
        }
    }
}
