using System.Collections.Generic;
using Bw.UseCases.Bots.Senses;
using Bw.UseCases.Bots.Senses.Extensions;
using Bw.UseCases.Mortality;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Bots.Goap
{
    public sealed class BotSituation
    {
        private const float CloseSlack = 1.5f;
        private const float TargetHalfWidth = 0.4f;

        private readonly BotInventory _inventory;
        private readonly BotProps _props;
        private readonly IReadonlyMortal _self;
        private readonly BotDifficultyConfig _difficulty;
        private readonly BotsConfig _bots;

        public BotSituation(
            BotInventory inventory,
            BotProps props,
            IReadonlyMortal self,
            BotDifficultyConfig difficulty,
            BotsConfig bots)
        {
            _inventory = inventory;
            _props = props;
            _self = self;
            _difficulty = difficulty;
            _bots = bots;
        }

        public BotFact FactsAfter(BotFact previous, Vector2 eye, IReadOnlyList<Sighting> sightings)
        {
            var facts = sightings.TryFindTarget(eye, out var target) ? FactsAbout(target, eye, previous) : BotFact.None;
            if (_inventory.IsDrawn(WeaponKind.Gun))
                facts |= BotFact.GunDrawn;

            if (_inventory.IsDrawn(WeaponKind.Fists))
                facts |= BotFact.FistsDrawn;

            if (_inventory.IsDrawn(WeaponKind.Prop))
                facts |= BotFact.PropHeld;

            if (_props.TryFindNearest(out _))
                facts |= BotFact.PropNearby;

            if (_self.Health.State.Value.Current <= _self.Health.Max * _difficulty.HurtShare)
                facts |= BotFact.Hurt;

            return facts;
        }

        private BotFact FactsAbout(Sighting target, Vector2 eye, BotFact previous)
        {
            if (!target.Visible)
                return BotFact.TargetNoticed;

            var facts = BotFact.TargetNoticed | BotFact.TargetInSight;
            var distance = (target.Position - eye).magnitude;
            if (distance <= _difficulty.CloseRange + ((previous & BotFact.TargetClose) != 0 ? CloseSlack : 0f))
                facts |= BotFact.TargetClose;

            if (distance <= _bots.Fists.Reach + _bots.Fists.Radius + TargetHalfWidth)
                facts |= BotFact.TargetInReach;

            return facts;
        }
    }
}
