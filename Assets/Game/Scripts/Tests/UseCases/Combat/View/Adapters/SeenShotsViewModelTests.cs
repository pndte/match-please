using System;
using System.Collections.Generic;
using Bw.Tests.UseCases.Combat.Weapon.Doubles;
using Bw.UseCases.Character.View.Abstractions;
using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Combat.Hands;
using Bw.UseCases.Combat.Shooting;
using Bw.UseCases.Combat.View.Abstractions;
using Bw.UseCases.Combat.View.Adapters;
using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Guarding;
using Bw.UseCases.Mortality.View.Guarding;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Combat.View.Adapters
{
    public sealed class SeenShotsViewModelTests
    {
        private static readonly Vector2 Hands = new(0f, 0.8f);
        private static readonly Vector2 Drawn = new(0f, 0.4f);
        private static readonly Vector2 MuzzleOffset = new(0.3f, 0f);

        private LifetimeDefinition _lifetime;
        private ShootingRange _range;
        private Views _views;
        private FakeCharacter _shooter;
        private SeenShotsViewModel<GunState> _shots;
        private List<SeenShot> _seen;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _range = new ShootingRange();
            _range.Wall(new Vector2(1.5f, 0.5f), Vector2.one);
            _range.Person(new Vector2(6f, 1.25f));
            var gun = new FakeWeapon();
            _shooter = new FakeCharacter();
            _shooter.Hands.MoveTo(Hands);
            var inHands = new WeaponsInHands();
            inHands.Add(_lifetime.Lifetime, gun, _shooter);
            _views = new Views();
            var tracer = _range.Gun(gun, inHands, MuzzleOffset, new PelletsConfig());
            _shots = new SeenShotsViewModel<GunState>(_lifetime.Lifetime, gun, inHands, tracer, _views, new NoCovers());
            _seen = new List<SeenShot>();
            _shots.Seen.Advise(_lifetime.Lifetime, _seen.Add);
            _shots.Show(Shots(0), Shots(0), 1f);
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            _range.Dispose();
        }

        [Test]
        public void ShotFromBehindCoverIsSeenStoppingAtTheCoverWhereverTheShooterIsDrawn()
        {
            _views.Draw(_shooter, Hands + Drawn);

            _shots.Show(Shots(0), Shots(1), 1f);

            var pellet = _seen[0].Pellets[0];
            Assert.That(pellet.Layer, Is.EqualTo(ShootingRange.Ground), "the shot really hits the cover: blood on the target behind it promises damage that never comes");
            Assert.That(pellet.To.x, Is.EqualTo(1f).Within(0.01f));
        }

        [Test]
        public void TracerStartsAtTheDrawnMuzzle()
        {
            _views.Draw(_shooter, Hands + Drawn);

            _shots.Show(Shots(0), Shots(1), 1f);

            Assert.That(Vector2.Distance(_seen[0].Pellets[0].From, Hands + MuzzleOffset + Drawn), Is.LessThan(0.01f));
        }

        [Test]
        public void ShotOverTheCoverIsSeenHittingTheTarget()
        {
            _shooter.Hands.MoveTo(Hands + Drawn);
            _views.Draw(_shooter, Hands + Drawn);

            _shots.Show(Shots(0), Shots(1), 1f);

            Assert.That(_seen[0].Pellets[0].Layer, Is.EqualTo(ShootingRange.Characters));
        }

        private static GunState Shots(int shots) =>
            new(0f, 0, 0, 0, shots);

        private sealed class Views : ICharacterViews
        {
            public IViewableMap<IReadonlyCharacter, ICharacterView> ByCharacter { get; } = new ViewableMap<IReadonlyCharacter, ICharacterView>();

            public void Draw(IReadonlyCharacter character, Vector2 position) =>
                ByCharacter[character] = new DrawnAt(position);
        }

        private sealed class DrawnAt : ICharacterView
        {
            public ICharacterPoses Poses => throw new NotSupportedException("The drawn character has no poses.");
            public IGripView GripView => throw new NotSupportedException("The drawn character has no grip view.");
            public IGuardViewModel Guard => throw new NotSupportedException("The drawn character has no guard.");
            public Vector2 Position { get; }

            public DrawnAt(Vector2 position)
            {
                Position = position;
            }
        }

        private sealed class NoCovers : IReadonlyGuardCovers, IGuardCover
        {
            public IGuardCover CoverOf(IReadonlyMortal mortal) =>
                this;

            public bool InCover(Contact contact) =>
                false;
        }
    }
}
