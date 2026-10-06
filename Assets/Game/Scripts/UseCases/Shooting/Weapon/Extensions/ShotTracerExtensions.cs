using Bw.UseCases.Shooting.Weapon.Abstractions;

namespace Bw.UseCases.Shooting.Weapon.Extensions
{
    public static class ShotTracerExtensions
    {
        public static ShotTrace Trace(this IShotTracer tracer, float aim) =>
            tracer.Cast(tracer.Aim(aim));
    }
}
