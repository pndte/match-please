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
        public List<float> Cursor { get; } = new();
        public List<PreviewShot> Shots { get; } = new();

        private PreviewTrack()
        {
        }

        public static PreviewTrack Simulate(PreviewWeapon weapon, ISimulationStep step, int requested, float interval)
        {
            var track = new PreviewTrack();
            var states = track.Fire(weapon, step, requested, interval);
            track.Animate(weapon.Recoil, states, step.Duration);
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

        private List<WeaponState> Fire(PreviewWeapon weapon, ISimulationStep step, int requested, float interval)
        {
            var simulator = weapon.Shooting.SimulatorFor(step);
            var rolls = new Random(RollSeed);
            var state = weapon.Shooting.Initial;
            var states = new List<WeaponState> { state };
            var cursor = 0f;
            var accepted = 0;

            for (var tick = 1; tick * step.Duration <= MaxTime; tick++)
            {
                var time = (tick - 1) * step.Duration;
                var trigger = accepted < requested && accepted * interval <= time + step.Duration * 0.5f;
                var next = simulator.Step(state, new WeaponInput(cursor, trigger, false, 0f));
                if (weapon.Shooting.Accepted(state, next))
                    accepted++;

                if (next.Shots != state.Shots)
                {
                    var thrown = weapon.Shooting.Spread.ThrowOf(next.Aim, (float)(rolls.NextDouble() * 2.0 - 1.0));
                    Shots.Add(new PreviewShot(tick * step.Duration, next.Aim, thrown));
                    cursor = Mathf.DeltaAngle(0f, cursor + thrown);
                }

                state = next;
                states.Add(state);

                if (Shots.Count >= requested && tick * step.Duration - Shots[^1].Time >= SettleTime)
                    break;
            }

            return states;
        }

        private void Animate(WeaponRecoilConfig recoil, List<WeaponState> states, float tickDuration)
        {
            var frames = Mathf.CeilToInt((states.Count - 1) * tickDuration / Frame) + 1;
            for (var frame = 0; frame < frames; frame++)
            {
                var time = frame * Frame;
                Kick.Add(Punched(recoil.Kick, time));
                Tilt.Add(Punched(recoil.Tilt, time));
                Barrel.Add(ShownBarrel(states, time / tickDuration));
                Cursor.Add(ThrownBy(time));
            }
        }

        private float Punched(RecoilPunch punch, float time)
        {
            var offset = 0f;
            foreach (var shot in Shots)
                offset += punch.OffsetAt(time - shot.Time);

            return offset;
        }

        private float ThrownBy(float time)
        {
            var cursor = 0f;
            foreach (var shot in Shots)
                if (shot.Time <= time)
                    cursor = Mathf.DeltaAngle(0f, cursor + shot.Thrown);

            return cursor;
        }

        private static float ShownBarrel(List<WeaponState> states, float ticks)
        {
            var to = Mathf.Clamp(Mathf.FloorToInt(ticks), 0, states.Count - 1);
            var from = Mathf.Max(0, to - 1);
            return Mathf.DeltaAngle(0f, Mathf.LerpAngle(states[from].Aim, states[to].Aim, ticks - Mathf.Floor(ticks)));
        }
    }

    public readonly struct PreviewShot
    {
        public readonly float Time;
        public readonly float Aim;
        public readonly float Thrown;

        public PreviewShot(float time, float aim, float thrown)
        {
            Time = time;
            Aim = aim;
            Thrown = thrown;
        }
    }
}
