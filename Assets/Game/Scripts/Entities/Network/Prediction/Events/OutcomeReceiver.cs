using System;
using System.Collections.Generic;
using Bw.Entities.Network.Prediction.Events.Requests;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Network.Variables;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Prediction.Events
{
    public sealed class OutcomeReceiver<TTarget, TEffect> where TTarget : class where TEffect : struct
    {
        private readonly IServerTime _serverTime;
        private readonly IPredictionTargets<TEffect> _targets;
        private readonly EventPredictionConfig _config;
        private readonly OutcomePrediction<TTarget, TEffect> _outcomePrediction;
        private readonly List<AffectedTarget<IPredictionTarget<TEffect>, TEffect>> _affected = new();

        public OutcomeReceiver( //TODO: чрезвычайно много параметров конструктора
            Lifetime lifetime,
            IReadonlyControlledBy controlledBy,
            INetResultReceiver<OutcomeReport<TEffect>> report,
            INetworkTicks ticks,
            IServerTime serverTime,
            IPredictionTargets<TEffect> targets,
            EventPredictionConfig config,
            OutcomePrediction<TTarget, TEffect> outcomePrediction)
        {
            _serverTime = serverTime;
            _targets = targets;
            _config = config;
            _outcomePrediction = outcomePrediction;

            controlledBy.Me.WhenTrue(lifetime, controlledLifetime =>
            {
                report.Received.Advise(controlledLifetime, Receive);
                ticks.Ticked(TickPhase.Default).Advise(controlledLifetime, Expire); //TODO: отчёт и состояние цели, задержанные повторными отправками дольше ExpiryTicks, придут после отмены — снятое попадание покажется второй раз, со вспышкой
                controlledLifetime.OnTermination(outcomePrediction.RejectAll);
            });
        }

        private void Receive(OutcomeReport<TEffect> report)
        {
            _affected.Clear();
            for (var index = 0; index < report.Affected.Count; index++)
                _affected.Add(new AffectedTarget<IPredictionTarget<TEffect>, TEffect>
                    (TargetOf(report.Affected[index].Target), report.Affected[index].Effect));

            _outcomePrediction.Answer(report.Tick, _affected);
        }

        private void Expire(int tick) =>
            _outcomePrediction.ExpireBefore(_serverTime.Tick - _config.ExpiryTicks);

        private IPredictionTarget<TEffect> TargetOf(ulong networkObjectId)
        {
            if (!_targets.TryGet(networkObjectId, out var target))
                throw new InvalidOperationException(
                    $"The outcome names object {networkObjectId.ToString()}, which is not a spawned prediction target on this client: its object needs a target part for {typeof(TEffect).Name}, such as EventPredictionTargetInstaller<{typeof(TTarget).Name}, its state type, {typeof(TEffect).Name}>.");

            return target;
        }
    }
}
