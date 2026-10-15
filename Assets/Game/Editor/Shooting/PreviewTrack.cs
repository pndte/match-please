using System.Collections.Generic;
using Bw.Entities.Simulation;
using Bw.UseCases.Shooting.View.Recoil;
using Bw.UseCases.Shooting.Weapon;
using UnityEngine;
using Random = System.Random;

namespace Bw.EditorTools.Shooting
{
    public sealed class PreviewTrack
    {
        public const float Frame = 1f / 240f;

        private const float SettleTime = 1.2f;
        private const float MaxTime = 30f;
        private const int RollSeed = 7;

        public float Duration => (Kick.Count - 1) * Frame;
        public List<float> Kick { get; } = new();
        public List<float> Tilt { get; } = new();
        public List<float> Barrel { get; } = new();
        public List<PreviewShot> Shots { get; } = new();

        private PreviewTrack()
        {
        }

        public static PreviewTrack Simulate(
            PreviewWeapon weapon,
            ISimulationStep step,
            int requested,
            float interval)
        {
            var track = new PreviewTrack();
            var aims = track.Fire(weapon, step, requested, interval);
            track.Animate(weapon.Recoil, aims, step.Duration);
            return track;
        }

        public int FrameAt(float time) =>
            Mathf.Clamp(Mathf.RoundToInt(time / Frame), 0, Kick.Count - 1);

        public int FiredBy(float time)
        {
            var fired = 0;
            foreach (var shot in Shots)
                if (shot.Time <= time)
                    fired++;

            return fired;
        }

        public float SinceShot(float time)
        {
            var since = float.MaxValue;
            foreach (var shot in Shots)
                if (shot.Time <= time)
                    since = time - shot.Time;

            return since;
        }

        private List<float> Fire(
            PreviewWeapon weapon,
            ISimulationStep step,
            int requested,
            float interval)
        {
            var simulation = weapon.Shooting.SimulationFor(step);
            var rolls = new Random(RollSeed);
            var aims = new List<float> { simulation.Aim };
            var deflection = 0f;

            for (var tick = 1; tick * step.Duration <= MaxTime; tick++)
            {
                var time = (tick - 1) * step.Duration;
                var trigger = simulation.Accepted < requested && simulation.Accepted * interval <= time + step.Duration * 0.5f;
                var shots = simulation.Shots;
                simulation.Step(new WeaponInput(deflection, trigger, false, 0f, 0f, default));

                if (simulation.Shots != shots)
                {
                    var thrown = weapon.Shooting.ThrowOf(simulation.Aim, (float)(rolls.NextDouble() * 2.0 - 1.0));
                    Shots.Add(new PreviewShot(tick * step.Duration, simulation.Aim));
                    deflection = Mathf.DeltaAngle(0f, deflection + thrown);
                }

                deflection = weapon.Shooting.Returned(deflection, step.Duration);
                aims.Add(simulation.Aim);

                if (Shots.Count >= requested && tick * step.Duration - Shots[^1].Time >= SettleTime)
                    break;
            }

            return aims;
        }

        private void Animate(WeaponRecoilConfig recoil, List<float> aims, float tickDuration)
        {
            var frames = Mathf.CeilToInt((aims.Count - 1) * tickDuration / Frame) + 1;
            for (var frame = 0; frame < frames; frame++)
            {
                var time = frame * Frame;
                Kick.Add(Punched(recoil.Kick, time));
                Tilt.Add(Punched(recoil.Tilt, time));
                Barrel.Add(ShownBarrel(aims, time / tickDuration));
            }
        }

        private float Punched(RecoilPunch punch, float time)
        {
            var offset = 0f;
            foreach (var shot in Shots)
                offset += punch.OffsetAt(time - shot.Time);

            return offset;
        }

        private static float ShownBarrel(List<float> aims, float ticks)
        {
            var to = Mathf.Clamp(Mathf.FloorToInt(ticks), 0, aims.Count - 1);
            var from = Mathf.Max(0, to - 1);
            return Mathf.DeltaAngle(0f, Mathf.LerpAngle(aims[from], aims[to], ticks - Mathf.Floor(ticks)));
        }
    }

    public readonly struct PreviewShot
    {
        public readonly float Time;
        public readonly float Aim;

        public PreviewShot(float time, float aim)
        {
            Time = time;
            Aim = aim;
        }
    }
}
