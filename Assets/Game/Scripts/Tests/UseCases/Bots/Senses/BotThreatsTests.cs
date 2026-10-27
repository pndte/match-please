using Bw.Entities.Simulation;
using Bw.Tests.Entities.Network.Doubles;
using Bw.Tests.UseCases.PropsMatchBots.Doubles;
using Bw.UseCases.Bots;
using Bw.UseCases.Bots.Senses;
using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Melee;
using Bw.UseCases.Combat.Shooting;
using Bw.UseCases.Combat.Weapon;
using Bw.UseCases.Props;
using JetBrains.Lifetimes;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Bots.Senses
{
    public sealed class BotThreatsTests
    {
        private const float TowardTheBot = 180f;

        private LifetimeDefinition _lifetime;
        private BotDifficultyConfig _difficulty;
        private BotsConfig _bots;
        private StubWeaponCarriers _inHands;
        private WeaponStates<GunState> _guns;
        private WeaponStates<MeleeState> _blows;
        private WeaponStates<PropState> _throws;
        private StubCharacter _self;
        private StubCharacter _shooter;
        private StubWeapon _gun;
        private BotThreats _threats;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _difficulty = ScriptableObject.CreateInstance<BotDifficultyConfig>();
            _difficulty.ReactionTime = 0.5f;
            _bots = ScriptableObject.CreateInstance<BotsConfig>();
            _bots.ThreatMemory = 1f;
            _inHands = new StubWeaponCarriers();
            _guns = new WeaponStates<GunState>();
            _blows = new WeaponStates<MeleeState>();
            _throws = new WeaponStates<PropState>();
            _self = new StubCharacter(Vector2.zero);
            _shooter = Armed(new Vector2(10f, 0f), WeaponKind.Gun, out _gun);
            var hearing = new BotHearing(_lifetime.Lifetime, _self, _inHands, _bots);
            var weapons = new BotWeapons(_inHands, _guns, _blows, _throws);
            _threats = new BotThreats(hearing, weapons, _self, _difficulty, _bots, new Step());
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            Object.DestroyImmediate(_difficulty);
            Object.DestroyImmediate(_bots);
        }

        [Test]
        public void ShotAtTheBotIsNoticedAfterTheReaction()
        {
            _gun.Fire(TowardTheBot);
            var first = _threats.ThreatAfter(default);
            _gun.Fire(TowardTheBot);
            var second = _threats.ThreatAfter(first);

            Assert.That(first.Attacker, Is.SameAs(_shooter));
            Assert.That(first.Noticed, Is.False);
            Assert.That(second.Noticed, Is.True);
        }

        [TestCase(90f)]
        [TestCase(0f)]
        public void ShotPastOrAwayFromTheBotIsNoAttack(float aim)
        {
            _gun.Fire(aim);

            Assert.That(_threats.ThreatAfter(default).Attacker, Is.Null);
        }

        [Test]
        public void ShotFromBeyondTheRangeIsNoAttack()
        {
            Armed(new Vector2(_bots.ThreatRange + 1f, 0f), WeaponKind.Gun, out var gun);
            gun.Fire(TowardTheBot);

            Assert.That(_threats.ThreatAfter(default).Attacker, Is.Null);
        }

        [Test]
        public void OwnShotIsNoAttack()
        {
            var own = new StubWeapon();
            _self.Inventory.Carry(new WeaponSlot(), own, new WeaponKindRules { Kind = WeaponKind.Gun });
            _inHands.Add(_lifetime.Lifetime, own, (IReadonlyCharacter)_self);
            own.Fire(TowardTheBot);

            Assert.That(_threats.ThreatAfter(default).Attacker, Is.Null);
        }

        [Test]
        public void AttackIsRememberedForTheMemoryAfterTheLastShot()
        {
            var threat = Noticed();
            for (var tick = 0; tick < 4; tick++)
                threat = _threats.ThreatAfter(threat);

            Assert.That(threat.Noticed, Is.True);
            Assert.That(_threats.ThreatAfter(threat).Noticed, Is.False);
        }

        [Test]
        public void AttackAfterTheMemoryNeedsTheReactionAgain()
        {
            var threat = Noticed();
            for (var tick = 0; tick < 5; tick++)
                threat = _threats.ThreatAfter(threat);

            _gun.Fire(TowardTheBot);

            Assert.That(_threats.ThreatAfter(threat).Noticed, Is.False);
        }

        [Test]
        public void NewAttackerNeedsTheReactionAgain()
        {
            var threat = Noticed();
            Armed(new Vector2(-10f, 0f), WeaponKind.Gun, out var gun);
            gun.Fire(0f);

            Assert.That(_threats.ThreatAfter(threat).Noticed, Is.False);
        }

        [Test]
        public void BlowChargedNearbyIsAnAttack()
        {
            var brawler = Armed(new Vector2(2f, 0f), WeaponKind.Fists, out var fists);
            _blows.Add(_lifetime.Lifetime, fists, new SimulationValue<MeleeState>(new MeleeState(0f, MeleePhase.Charging, 0, true, 0, 0)));
            var threat = _threats.ThreatAfter(_threats.ThreatAfter(default));

            Assert.That(threat.Attacker, Is.SameAs(brawler));
            Assert.That(threat.Noticed, Is.True);
        }

        [Test]
        public void ThrowWoundUpNearbyIsAnAttack()
        {
            var thrower = Armed(new Vector2(-2f, 0f), WeaponKind.Prop, out var crate);
            _throws.Add(_lifetime.Lifetime, crate, new SimulationValue<PropState>(new PropState(0f, PropPhase.WindingUp, 0, 0, true)));

            Assert.That(_threats.ThreatAfter(default).Attacker, Is.SameAs(thrower));
        }

        [Test]
        public void FistsAtRestNearbyAreNoAttack()
        {
            Armed(new Vector2(2f, 0f), WeaponKind.Fists, out var fists);
            _blows.Add(_lifetime.Lifetime, fists, new SimulationValue<MeleeState>(new MeleeState(0f, MeleePhase.Idle, 0, false, 0, 0)));

            Assert.That(_threats.ThreatAfter(default).Attacker, Is.Null);
        }

        [Test]
        public void BlowChargedFarAwayIsNoAttack()
        {
            Armed(new Vector2(_bots.ChargeThreatRange + 1f, 0f), WeaponKind.Fists, out var fists);
            _blows.Add(_lifetime.Lifetime, fists, new SimulationValue<MeleeState>(new MeleeState(0f, MeleePhase.Charging, 0, true, 0, 0)));

            Assert.That(_threats.ThreatAfter(default).Attacker, Is.Null);
        }

        private Threat Noticed()
        {
            _gun.Fire(TowardTheBot);
            var threat = _threats.ThreatAfter(default);
            _gun.Fire(TowardTheBot);
            return _threats.ThreatAfter(threat);
        }

        private StubCharacter Armed(Vector2 position, WeaponKind kind, out StubWeapon weapon)
        {
            var character = new StubCharacter(position);
            character.Hands.Pose = new HoldPose(position, new Rect(position, Vector2.zero));
            weapon = new StubWeapon();
            character.Inventory.Carry(new WeaponSlot(), weapon, new WeaponKindRules { Kind = kind });
            _inHands.Add(_lifetime.Lifetime, weapon, (IReadonlyCharacter)character);
            return character;
        }

        private sealed class Step : ISimulationStep
        {
            public float Duration => 0.25f;
        }
    }
}
