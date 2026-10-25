using System.Collections.Generic;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Shooting;
using Bw.UseCases.Combat.Weapon;
using Bw.UseCases.Combat.Weapon.Abstractions;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Combat.Weapon
{
    public sealed class GuardedWeaponSimulatorTests
    {
        private static readonly GunState Reloading = new(0f, 1, 0, 5, 0);
        private static readonly GunState Interrupted = new(0f, 1, 0, 0, 0);

        private RecordingSimulator _inner;
        private GuardedWeaponSimulator<GunState> _guarded;

        [SetUp]
        public void SetUp()
        {
            _inner = new RecordingSimulator();
            _guarded = new GuardedWeaponSimulator<GunState>(_inner);
        }

        [Test]
        public void UnguardedInputStepsTheStateAsItIs()
        {
            _guarded.Step(Reloading, new WeaponInput(0f, true, false, false));

            Assert.That(_inner.Calls, Is.EqualTo(new[] { "step" }));
            Assert.That(_inner.Stepped, Is.EqualTo(new[] { Reloading }));
        }

        [Test]
        public void GuardedInputInterruptsTheWeaponBeforeTheStep()
        {
            _guarded.Step(Reloading, new WeaponInput(0f, true, false, true));

            Assert.That(_inner.Calls, Is.EqualTo(new[] { "interrupt", "step" }));
            Assert.That(_inner.Stepped, Is.EqualTo(new[] { Interrupted }));
        }

        [Test]
        public void GuardedStepAnswersWithTheInnerStep()
        {
            var stepped = _guarded.Step(Reloading, new WeaponInput(0f, false, false, true));

            Assert.That(stepped, Is.EqualTo(RecordingSimulator.Next));
        }

        [Test]
        public void InterruptionIsTheInnerOne()
        {
            Assert.That(_guarded.Interrupted(Reloading), Is.EqualTo(Interrupted));
            Assert.That(_inner.Calls, Is.EqualTo(new[] { "interrupt" }));
        }

        [Test]
        public void RestIsTheInnerOne()
        {
            Assert.That(_guarded.Rested(Reloading), Is.EqualTo(RecordingSimulator.RestedState));
            Assert.That(_inner.Calls, Is.EqualTo(new[] { "rest" }));
        }

        private sealed class RecordingSimulator : IWeaponSimulator<GunState>
        {
            public static readonly GunState Next = new(10f, 0, 3, 0, 1);
            public static readonly GunState RestedState = new(0f, 1, 0, 0, 7);

            public List<string> Calls { get; } = new();
            public List<GunState> Stepped { get; } = new();

            public GunState Step(GunState state, WeaponInput input)
            {
                Calls.Add("step");
                Stepped.Add(state);
                return Next;
            }

            public GunState Interrupted(GunState state)
            {
                Calls.Add("interrupt");
                return new GunState(state.Aim, state.Ammo, state.CooldownTicks, 0, state.Shots);
            }

            public GunState Rested(GunState state)
            {
                Calls.Add("rest");
                return RestedState;
            }
        }
    }
}
