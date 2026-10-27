using System;
using System.Collections.Generic;
using Bw.Entities.Network.Prediction.Stream;
using Bw.UseCases.Bots;
using Bw.UseCases.Bots.Abstractions;
using Bw.UseCases.Bots.Goals;
using Bw.UseCases.Bots.Goap;
using Bw.UseCases.Bots.Navigation;
using Bw.UseCases.Bots.Network;
using Bw.UseCases.Bots.Senses;
using Bw.UseCases.Bots.Skills;
using Bw.UseCases.Character.Network.Codecs;
using Bw.UseCases.Character.Network.Requests;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Hands.Network;
using Zenject;

namespace Bw.Injection.Bots
{
    public sealed class BotLifeInstaller : Installer<BotDifficultyConfig, BotPersonality, BotLifeInstaller>
    {
        private readonly BotDifficultyConfig _difficulty;
        private readonly BotPersonality _personality;
        private readonly NavGraphConfig _navigation;
        private readonly BotsConfig _bots;

        public BotLifeInstaller(
            BotDifficultyConfig difficulty,
            BotPersonality personality,
            NavGraphConfig navigation,
            BotsConfig bots)
        {
            _difficulty = difficulty;
            _personality = personality;
            _navigation = navigation;
            _bots = bots;
        }

        public override void InstallBindings()
        {
            if (_personality == null)
                throw new InvalidOperationException($"A bot of difficulty '{_difficulty.name}' came without a personality: every bot entry and the stand-in of '{_bots.name}' name one.");

            Container.BindInstance(_difficulty).AsSingle();
            Container.BindInstance(_personality).AsSingle();
            Container.Bind(typeof(IBotGamepad), typeof(IReadonlyBotGamepad)).To<BotGamepad>().AsSingle();
            BindSamplers();

            switch (_difficulty.Brain)
            {
                case BotBrainKind.Idle:
                    Container.Bind<IBotBrain>().To<IdleBrain>().AsSingle();
                    break;
                case BotBrainKind.Goap:
                    BindGoap();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_difficulty.Brain), _difficulty.Brain, $"Difficulty '{_difficulty.name}' asks for a brain the bots lack.");
            }

            Container.Bind<BotBrainStepper>().AsSingle().NonLazy();
        }

        private void BindSamplers()
        {
            Container.Bind<HandsControlsForm>().AsSingle();
            Container.Bind<IInputSampler<CharacterControls>>().To<BotCharacterSampler>().AsSingle();
            Container.Decorate<IInputSampler<CharacterControls>>().With<WireRoundedSampler<CharacterControls, TickedCharacterControlsCodec>>();
            Container.Bind<BotSamplerRegistrar<CharacterControls>>().AsSingle().NonLazy();
        }

        private void BindGoap()
        {
            Container.Bind<BotSight>().AsSingle();
            Container.Bind<BotHearing>().AsSingle();
            Container.Bind<BotWeapons>().AsSingle();
            Container.Bind<BotThreats>().AsSingle();
            Container.Bind<BotInventory>().AsSingle();
            Container.Bind<BotProps>().AsSingle();
            Container.Bind<TargetAssessor>().AsSingle();
            Container.Bind<BotSituation>().AsSingle();
            Container.Bind<BotGunner>().AsTransient();
            Container.Bind<PathFollower>().AsSingle().WithArguments(_navigation.Movement);
            Container.Bind<BotNavigator>().AsSingle();
            Container.Bind<IBotBrain>()
                .FromMethod(context => new GoapBrain(
                    context.Container.Resolve<BotSight>(),
                    context.Container.Resolve<BotThreats>(),
                    context.Container.Resolve<BotSituation>(),
                    Goals(),
                    new GoapPlanner(Actions(context.Container)),
                    context.Container.Resolve<IBotGamepad>()))
                .AsSingle();
        }

        private IReadOnlyList<IGoapGoal> Goals()
        {
            var goals = new List<IGoapGoal>();
            if (_difficulty.Hostile)
                AddHunting(goals);

            if (IsParrying())
                goals.Add(new GuardGoal(_personality.Guard));

            if (IsRetreating())
                goals.Add(new RetreatGoal(_personality.Retreat));

            if (IsKiting())
                goals.Add(new KiteGoal(_personality.Kite));

            if (_difficulty.Moving)
                goals.Add(new RoamGoal());

            return goals;
        }

        private void AddHunting(List<IGoapGoal> goals)
        {
            if (_personality.Kill > 0f)
                goals.Add(new KillGoal(_personality.Kill));

            if (_personality.FinishOff > 0f)
                goals.Add(new FinishOffGoal(_personality.FinishOff));

            if (_personality.PunishReload > 0f)
                goals.Add(new PunishReloadGoal(_personality.PunishReload));
        }

        private IReadOnlyList<GoapAction> Actions(DiContainer container)
        {
            var actions = new List<GoapAction>();
            if (_difficulty.Hostile)
                AddShooting(actions, container);

            if (_difficulty.Hostile && _difficulty.Brawling)
                AddBrawling(actions, container);

            if (IsShielding())
                AddShielding(actions, container);

            if (IsRetreating())
                actions.Add(Action(BotFact.TargetNoticed, BotFact.None, BotFact.Retreated, BotFact.None, _bots.RetreatCost, container.Instantiate<RetreatSkill>()));

            if (IsKiting())
                actions.Add(Action(BotFact.TargetInSight | BotFact.GunDrawn, BotFact.None, BotFact.Kited, BotFact.None, _bots.KiteCost, container.Instantiate<KiteSkill>()));

            if (IsParrying())
                actions.Add(Action(BotFact.Threatened | BotFact.GuardReady, BotFact.PropHeld, BotFact.Parried, BotFact.None, _bots.ParryCost, container.Instantiate<GuardSkill>()));

            if (_difficulty.Moving)
                actions.Add(Action(BotFact.None, BotFact.None, BotFact.Roamed, BotFact.None, _bots.RoamCost, container.Instantiate<RoamSkill>()));

            return actions;
        }

        private void AddShooting(List<GoapAction> actions, DiContainer container)
        {
            const BotFact aimed = BotFact.TargetInSight | BotFact.GunDrawn;
            var tooClose = _difficulty.Brawling ? BotFact.TargetClose : BotFact.None;
            actions.Add(Action(aimed | BotFact.GunReady, tooClose, BotFact.TargetDown, BotFact.None, _bots.ShootCost, container.Instantiate<ShootSkill>()));
            actions.Add(Action(aimed, tooClose | BotFact.GunReady, BotFact.TargetDown, BotFact.None, _bots.ShootCost + _bots.WaitCost, container.Instantiate<ShootSkill>()));
            actions.Add(Action(BotFact.TargetNoticed, BotFact.GunDrawn | BotFact.PropHeld, BotFact.GunDrawn, BotFact.FistsDrawn, _bots.DrawCost, Draw(container, WeaponKind.Gun)));
            if (_difficulty.Moving)
                actions.Add(Action(BotFact.TargetNoticed, BotFact.TargetInSight, BotFact.TargetInSight, BotFact.None, _bots.ApproachCost, container.Instantiate<ApproachSkill>()));
        }

        private void AddBrawling(List<GoapAction> actions, DiContainer container)
        {
            actions.Add(Action(BotFact.TargetClose, BotFact.FistsDrawn | BotFact.PropHeld, BotFact.FistsDrawn, BotFact.GunDrawn, MeleeCost(_bots.DrawCost), Draw(container, WeaponKind.Fists)));
            actions.Add(Action(BotFact.FistsDrawn | BotFact.TargetInReach, BotFact.None, BotFact.TargetDown, BotFact.None, MeleeCost(_bots.PunchCost), container.Instantiate<PunchSkill>()));
            if (_difficulty.Moving)
            {
                actions.Add(Action(BotFact.TargetClose, BotFact.TargetInReach, BotFact.TargetInReach, BotFact.None, MeleeCost(_bots.CloseInCost), CloseIn(container, BotFact.TargetInReach)));
                actions.Add(Action(BotFact.TargetInSight, BotFact.TargetClose, BotFact.TargetClose, BotFact.None, MeleeCost(_bots.RushCost), CloseIn(container, BotFact.TargetClose)));
            }

            if (_difficulty.ChargingPunches)
                actions.Add(Action(BotFact.FistsDrawn | BotFact.TargetClose, BotFact.None, BotFact.TargetDown, BotFact.None, MeleeCost(_bots.ChargedPunchCost), container.Instantiate<ChargedPunchSkill>()));
        }

        private void AddShielding(List<GoapAction> actions, DiContainer container)
        {
            actions.Add(Action(BotFact.PropNearby, BotFact.PropHeld, BotFact.PropHeld, BotFact.GunDrawn | BotFact.FistsDrawn, ShieldCost(_bots.FetchPropCost), container.Instantiate<FetchPropSkill>()));
            actions.Add(Action(BotFact.PropHeld | BotFact.TargetInSight, BotFact.TargetClose, BotFact.TargetClose, BotFact.None, ShieldCost(_bots.ShieldCost), CloseIn(container, BotFact.TargetClose)));
            actions.Add(Action(BotFact.PropHeld | BotFact.TargetClose, BotFact.None, BotFact.TargetDown, BotFact.PropHeld, _bots.ThrowCost, container.Instantiate<ThrowSkill>()));
            if (IsRetreating())
                actions.Add(Action(BotFact.PropHeld | BotFact.TargetNoticed, BotFact.None, BotFact.Retreated, BotFact.None, ShieldCost(_bots.ShieldedRetreatCost), container.Instantiate<RetreatSkill>()));
        }

        private float MeleeCost(float cost) =>
            cost * _personality.MeleeCostScale;

        private float ShieldCost(float cost) =>
            cost * _personality.ShieldCostScale;

        private bool IsShielding() =>
            _difficulty.Shielding && _difficulty.Hostile && _difficulty.Moving;

        private bool IsRetreating() =>
            _difficulty.Hostile && _difficulty.Moving && _personality.Retreat > 0f;

        private bool IsKiting() =>
            _difficulty.Hostile && _difficulty.Moving && _personality.Kite > 0f;

        private bool IsParrying() =>
            _difficulty.Parrying && _difficulty.Hostile && _personality.Guard > 0f;

        private static DrawSkill Draw(DiContainer container, WeaponKind kind) =>
            container.Instantiate<DrawSkill>(new object[] { kind });

        private static CloseInSkill CloseIn(DiContainer container, BotFact until) =>
            container.Instantiate<CloseInSkill>(new object[] { until });

        private static GoapAction Action(
            BotFact needs,
            BotFact forbids,
            BotFact gives,
            BotFact takes,
            float cost,
            IBotSkill skill) =>
            new(new GoapCondition(needs, forbids), new GoapEffect(gives, takes), cost, skill);
    }
}
