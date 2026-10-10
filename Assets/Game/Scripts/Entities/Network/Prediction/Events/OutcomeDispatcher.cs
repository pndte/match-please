using System;
using System.Collections.Generic;
using Bw.Entities.Network.Prediction.Events.Requests;
using Bw.Entities.Network.Variables;
using JetBrains.Lifetimes;
using Unity.Netcode;

namespace Bw.Entities.Network.Prediction.Events
{
    public sealed class OutcomeDispatcher<TInitiator, TTarget, TEffect> where TInitiator : class where TTarget : class where TEffect : struct
    {
        private readonly TInitiator _initiator;
        private readonly NetworkObject _networkObject;
        private readonly IControlledBy _controlledBy;
        private readonly IClientPlayerCollection _clientPlayers;
        private readonly IAffectables<TEffect> _affectables;
        private readonly INetResultSender<OutcomeReport<TEffect>> _report;
        private readonly List<IAffectable<TEffect>> _targets = new();

        private int _lastTick = int.MinValue;

        public OutcomeDispatcher(
            Lifetime lifetime,
            TInitiator initiator,
            NetworkObject networkObject,
            IControlledBy controlledBy,
            IClientPlayerCollection clientPlayers,
            IAffectables<TEffect> affectables,
            IOutcomes<TInitiator, TTarget, TEffect> outcomes,
            INetResultSender<OutcomeReport<TEffect>> report)
        {
            _initiator = initiator;
            _networkObject = networkObject;
            _controlledBy = controlledBy;
            _clientPlayers = clientPlayers;
            _affectables = affectables;
            _report = report;

            outcomes.Reported.Advise(lifetime, Dispatch);
        }

        private void Dispatch(ActionOutcome<TInitiator, TTarget, TEffect> outcome)
        {
            if (!ReferenceEquals(outcome.Initiator, _initiator))
                return;

            if (outcome.Tick <= _lastTick)
                throw new InvalidOperationException(
                    $"An initiator acts once per tick: its outcome of tick {outcome.Tick.ToString()} came after tick {_lastTick.ToString()}.");

            var reported = TryGetController(out var controller);
            var affected = outcome.Affected.Count == 0
                ? Array.Empty<AffectedTarget<ulong, TEffect>>()
                : new AffectedTarget<ulong, TEffect>[outcome.Affected.Count];

            _targets.Clear();
            for (var index = 0; index < affected.Length; index++)
                affected[index] = Resolve(outcome.Affected, index);

            _lastTick = outcome.Tick;

            var action = new ActionId(_networkObject.NetworkObjectId, outcome.Tick);
            for (var index = 0; index < _targets.Count; index++)
                _targets[index].Affect(action, outcome.Affected[index].Effect);

            if (reported)
                _report.SendTo(controller, new OutcomeReport<TEffect>(outcome.Tick, affected));
        }

        private AffectedTarget<ulong, TEffect> Resolve(IReadOnlyList<AffectedTarget<TTarget, TEffect>> affected, int index)
        {
            for (var previous = 0; previous < index; previous++)
                if (EqualityComparer<TTarget>.Default.Equals(affected[previous].Target, affected[index].Target))
                    throw new InvalidOperationException("An action affects each target once: its effects on one target must come as one effect.");

            if (!_affectables.TryGet(affected[index].Target, out var targetId, out var affectable))
                throw new InvalidOperationException(
                    $"The outcome names a {typeof(TTarget).Name} that is not registered as affectable: install a target part on its object, such as EventPredictionTargetInstaller<{typeof(TTarget).Name}, its state type, {typeof(TEffect).Name}>, and name the object its parts are registered under.");

            _targets.Add(affectable);
            return new AffectedTarget<ulong, TEffect>(targetId, affected[index].Effect);
        }

        private bool TryGetController(out IClient controller)
        {
            controller = default;
            if (_controlledBy.Users.Count == 0)
                return false;

            if (_controlledBy.Users.Count > 1)
                throw new InvalidOperationException("An outcome is reported to the one player who controls the initiator, and several players control it.");

            controller = _clientPlayers.ByClient.Inverse[_controlledBy.Users[0]]; //TODO: у бота клиента нет — когда появятся боты, их итоги применять без отчёта
            return true;
        }
    }
}
