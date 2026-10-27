using System.Collections.Generic;
using Bw.UseCases.Bots.Extensions;
using Bw.UseCases.Bots.Senses;
using Bw.UseCases.Bots.Senses.Extensions;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Guarding;
using UnityEngine;

namespace Bw.UseCases.Bots.Goap
{
    public sealed class BotSituation
    {
        private readonly BotInventory _inventory;
        private readonly BotProps _props;
        private readonly TargetAssessor _targets;
        private readonly IReadonlyMortal _self;
        private readonly IReadonlyGuard _guard;
        private readonly BotPersonality _personality;

        public BotSituation(
            BotInventory inventory,
            BotProps props,
            TargetAssessor targets,
            IReadonlyMortal self,
            IReadonlyGuard guard,
            BotPersonality personality)
        {
            _inventory = inventory;
            _props = props;
            _targets = targets;
            _self = self;
            _guard = guard;
            _personality = personality;
        }

        public BotAssessment AssessmentAfter(
            BotAssessment previous,
            Vector2 eye,
            IReadOnlyList<Sighting> sightings,
            Threat threat)
        {
            var facts = sightings.TryFindTarget(eye, out var target) ? _targets.FactsAbout(target, eye, previous.Facts) : BotFact.None;
            if (_inventory.IsDrawn(WeaponKind.Gun))
                facts |= BotFact.GunDrawn;

            if (_inventory.GunLoaded)
                facts |= BotFact.GunReady;

            if (_inventory.IsDrawn(WeaponKind.Fists))
                facts |= BotFact.FistsDrawn;

            if (_inventory.IsDrawn(WeaponKind.Prop))
                facts |= BotFact.PropHeld;

            if (_props.TryFindNearest(out _))
                facts |= BotFact.PropNearby;

            var health = _self.Health.Share();
            if (health <= _personality.RetreatHealth)
                facts |= BotFact.Hurt;

            if (threat.Noticed)
                facts |= BotFact.Threatened;

            if (!_guard.State.Value.Broken)
                facts |= BotFact.GuardReady;

            return new BotAssessment(facts, health);
        }
    }
}
