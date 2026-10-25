using System;
using System.Collections.Generic;
using System.Linq;
using Bw.Tests.UseCases.PropsMatchBots.Doubles;
using Bw.UseCases.Bots.Goap;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Bots.Goap
{
    public sealed class GoapPlannerTests
    {
        private const BotFact X = BotFact.GunDrawn;
        private const BotFact Y = BotFact.TargetInSight;
        private const BotFact Z = BotFact.TargetClose;
        private const BotFact Wanted = BotFact.TargetDown;

        private List<GoapAction> _plan;

        [SetUp]
        public void SetUp() =>
            _plan = new List<GoapAction>();

        [Test]
        public void GoalAlreadyMetNeedsNoActions()
        {
            var planner = new GoapPlanner(new[] { Action("shoot", BotFact.None, Wanted, 1f) });

            Assert.That(planner.TryPlan(Wanted | X, Wanted, _plan), Is.True);
            Assert.That(_plan, Is.Empty);
        }

        [Test]
        public void SingleActionReachesTheGoal()
        {
            var planner = new GoapPlanner(new[] { Action("shoot", X, Wanted, 1f) });

            Assert.That(planner.TryPlan(X, Wanted, _plan), Is.True);
            Assert.That(Names(), Is.EqualTo(new[] { "shoot" }));
        }

        [Test]
        public void ChainIsOrderedByWhatEachActionNeeds()
        {
            var planner = new GoapPlanner(new[]
            {
                Action("third", Y, Wanted, 1f),
                Action("first", BotFact.None, X, 1f),
                Action("second", X, Y, 1f)
            });

            Assert.That(planner.TryPlan(BotFact.None, Wanted, _plan), Is.True);
            Assert.That(Names(), Is.EqualTo(new[] { "first", "second", "third" }));
        }

        [Test]
        public void CheaperLongerPlanWinsOverAnExpensiveShortcut()
        {
            var planner = new GoapPlanner(new[]
            {
                Action("shortcut", BotFact.None, Wanted, 5f),
                Action("prepare", BotFact.None, X, 1f),
                Action("finish", X, Wanted, 1f)
            });

            planner.TryPlan(BotFact.None, Wanted, _plan);

            Assert.That(Names(), Is.EqualTo(new[] { "prepare", "finish" }));
        }

        [Test]
        public void CheaperShortcutWinsOverALongerPlan()
        {
            var planner = new GoapPlanner(new[]
            {
                Action("shortcut", BotFact.None, Wanted, 1.5f),
                Action("prepare", BotFact.None, X, 1f),
                Action("finish", X, Wanted, 1f)
            });

            planner.TryPlan(BotFact.None, Wanted, _plan);

            Assert.That(Names(), Is.EqualTo(new[] { "shortcut" }));
        }

        [Test]
        public void ForbiddenActionIsLeftOutOfThePlan()
        {
            var planner = new GoapPlanner(new[]
            {
                new GoapAction(new GoapCondition(BotFact.None, Z), new GoapEffect(Wanted, BotFact.None), 1f, new ScriptedSkill("shoot")),
                Action("punch", Z, Wanted, 3f)
            });

            planner.TryPlan(Z, Wanted, _plan);

            Assert.That(Names(), Is.EqualTo(new[] { "punch" }));
        }

        [Test]
        public void TakenFactsMustBeRegainedBeforeTheyAreNeeded()
        {
            var planner = new GoapPlanner(new[]
            {
                new GoapAction(new GoapCondition(BotFact.None, BotFact.None), new GoapEffect(X, Y), 1f, new ScriptedSkill("draw")),
                Action("look", BotFact.None, Y, 1f),
                Action("shoot", X | Y, Wanted, 1f)
            });

            planner.TryPlan(Y, Wanted, _plan);

            Assert.That(Names(), Is.EqualTo(new[] { "draw", "look", "shoot" }));
        }

        [Test]
        public void UnreachableGoalFailsWithAnEmptyPlan()
        {
            var planner = new GoapPlanner(new[] { Action("draw", BotFact.None, X, 1f), Action("shoot", Y, Wanted, 1f) });
            _plan.Add(Action("stale", BotFact.None, X, 1f));

            Assert.That(planner.TryPlan(BotFact.None, Wanted, _plan), Is.False);
            Assert.That(_plan, Is.Empty);
        }

        [Test]
        public void NoActionsReachNoGoal()
        {
            var planner = new GoapPlanner(Array.Empty<GoapAction>());

            Assert.That(planner.TryPlan(BotFact.None, Wanted, _plan), Is.False);
            Assert.That(planner.TryPlan(Wanted, Wanted, _plan), Is.True);
        }

        [Test]
        public void PlanOfSixActionsIsFound()
        {
            var planner = new GoapPlanner(Chain(6));

            Assert.That(planner.TryPlan(BotFact.None, Last(6), _plan), Is.True);
            Assert.That(_plan, Has.Count.EqualTo(6));
        }

        [Test]
        public void PlanLongerThanSixActionsIsNotSearched()
        {
            var planner = new GoapPlanner(Chain(7));

            Assert.That(planner.TryPlan(BotFact.None, Last(7), _plan), Is.False);
        }

        [Test]
        public void ActionThatChangesNothingIsNeverPlanned()
        {
            var planner = new GoapPlanner(new[]
            {
                Action("idle", BotFact.None, BotFact.None, 0.1f),
                Action("shoot", X, Wanted, 1f)
            });

            planner.TryPlan(X, Wanted, _plan);

            Assert.That(Names(), Is.EqualTo(new[] { "shoot" }));
        }

        [Test]
        public void PlannerCanBeAskedAgainWithOtherFacts()
        {
            var planner = new GoapPlanner(new[] { Action("draw", BotFact.None, X, 1f), Action("shoot", X, Wanted, 1f) });
            planner.TryPlan(BotFact.None, Wanted, _plan);

            planner.TryPlan(X, Wanted, _plan);

            Assert.That(Names(), Is.EqualTo(new[] { "shoot" }));
        }

        [Test]
        public void SeveralWantedFactsAreAllReached()
        {
            var planner = new GoapPlanner(new[] { Action("draw", BotFact.None, X, 1f), Action("look", BotFact.None, Y, 1f) });

            Assert.That(planner.TryPlan(BotFact.None, X | Y, _plan), Is.True);
            Assert.That(Names(), Is.EquivalentTo(new[] { "draw", "look" }));
        }

        private IEnumerable<string> Names() =>
            _plan.Select(action => action.Skill.ToString());

        private static GoapAction Action(
            string name,
            BotFact needs,
            BotFact gives,
            float cost) =>
            new(new GoapCondition(needs, BotFact.None), new GoapEffect(gives, BotFact.None), cost, new ScriptedSkill(name));

        private static GoapAction[] Chain(int length)
        {
            var actions = new GoapAction[length];
            for (var index = 0; index < length; index++)
                actions[index] = Action("step " + index, index == 0 ? BotFact.None : (BotFact)(1 << (index - 1)), (BotFact)(1 << index), 1f);

            return actions;
        }

        private static BotFact Last(int length) =>
            (BotFact)(1 << (length - 1));
    }
}
