using Bw.Tests.Doubles;
using Bw.UseCases.Combat.View.Tracer;
using UnityEngine;

namespace Bw.Tests.Entities.Doubles
{
    public sealed class FacadeLender
    {
        public BulletTracer Tracer { get; }
        public bool Hidden => !_core.enabled && !_glow.enabled && !_head.enabled;

        private readonly Renderer _core;
        private readonly Renderer _glow;
        private readonly Renderer _head;

        private FacadeLender(
            BulletTracer tracer,
            Renderer core,
            Renderer glow,
            Renderer head)
        {
            Tracer = tracer;
            _core = core;
            _glow = glow;
            _head = head;
        }

        public static FacadeLender On(GameObject owner, HiddenObjects objects)
        {
            var core = objects.Create("core").AddComponent<LineRenderer>();
            var glow = objects.Create("glow").AddComponent<LineRenderer>();
            var head = objects.Create("head").AddComponent<SpriteRenderer>();
            var tracer = owner.AddComponent<BulletTracer>()
                .With("_core", core)
                .With("_glow", glow)
                .With("_head", head);

            return new FacadeLender(tracer, core, glow, head);
        }
    }
}
