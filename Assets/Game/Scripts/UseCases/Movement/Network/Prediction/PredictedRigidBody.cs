using System;
using Bw.Entities.Extensions;
using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Replication;
using Bw.Entities.Network.Ticks;
using Bw.UseCases.Movement.Extensions;
using Bw.UseCases.Movement.Physics.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Movement.Network.Prediction
{
    public sealed class PredictedRigidBody : IShovable, IPredictable, IPredictedBody, IRollbackBody, IPredictedRigidBody, IStateCorrections<RigidState>
    {
        private const int Capacity = 64;
        private const int QuietTicks = 30;
        private const float Resting = 0.05f;
        private const float Distance = 0.01f;
        private const float Turn = 1f;
        private const float Speed = 0.1f;

        public PhysicsBody Body => _body;
        public ISource<StateCorrection<RigidState>> Corrected => _corrected;

        private readonly Signal<StateCorrection<RigidState>> _corrected = new();
        private readonly int[] _stateTicks = Unrecorded();
        private readonly RigidState[] _states = new RigidState[Capacity];
        private readonly int[] _shoveTicks = Unrecorded();
        private readonly Vector2[] _shoves = new Vector2[Capacity];
        private readonly SequentialLifetimes _predictions;
        private readonly PhysicsBody _body;
        private readonly IPhysicsRollback _rollback;
        private readonly IReplicatedStates<RigidState> _replicated;
        private readonly IRigidStateEffects _effects;
        private readonly INetworkTicks _ticks;

        private bool _predicted;
        private bool _correcting;
        private bool _reposed;
        private int _newest = int.MinValue;
        private int _posedTick;
        private int _activeTick = int.MinValue;
        private int _checkedTick = int.MinValue;
        private int _beforeTick;
        private double _beforeShownTick;
        private double _followedTick;
        private RigidState _before;
        private RigidState _followed;

        public PredictedRigidBody(
            Lifetime lifetime,
            PhysicsBody body,
            IPhysicsRollback rollback,
            IReplicatedStates<RigidState> replicated,
            IRigidStateEffects effects,
            INetworkTicks ticks)
        {
            _predictions = new SequentialLifetimes(lifetime);
            _body = body;
            _rollback = rollback;
            _replicated = replicated;
            _effects = effects;
            _ticks = ticks;
            _posedTick = ticks.Current;
            _followed = body.State();
            _followedTick = ticks.Current;

            ticks.Ticked(TickPhase.Early).Advise(lifetime, PoseFor);
            effects.Changed.Advise(lifetime, Affect);
        }

        public float RetreatSpeed(Vector2 direction) =>
            _predicted ? Mathf.Max(0f, Vector2.Dot(_body.linearVelocity, direction)) : 0f;

        public void Shove(Vector2 force)
        {
            if (!float.IsFinite(force.x) || !float.IsFinite(force.y))
                throw new ArgumentOutOfRangeException(nameof(force), force, "A shove pushes with a finite force.");

            if (!_predicted)
                StartPredicting();

            var slot = Slot(_posedTick);
            var recorded = _shoveTicks[slot] == _posedTick ? _shoves[slot] : Vector2.zero;
            if (_reposed || _shoveTicks[slot] != _posedTick)
            {
                _shoveTicks[slot] = _posedTick;
                _shoves[slot] = Vector2.zero;
            }

            _shoves[slot] += force;
            _reposed = false;
            _activeTick = Math.Max(_activeTick, _posedTick);
            if (_posedTick < _ticks.Current && _shoves[slot] != recorded)
                _rollback.Request(_posedTick);
        }

        public void Predict(int tick)
        {
            if (!_predicted)
                StartPredicting();

            _activeTick = Math.Max(_activeTick, tick);
        }

        public void PoseFor(int tick)
        {
            _posedTick = tick;
            _reposed = true;
            if (_predicted && TryGetRecorded(tick, out var state))
                _body.ApplyRigidState(state);
        }

        public void Restore(int tick)
        {
            if (!_correcting)
            {
                _correcting = true;
                _beforeTick = _newest;
                _beforeShownTick = _newest;
                _before = Recorded(_newest);
            }

            var state = TryGetRecorded(tick, out var recorded) ? recorded : Oldest();
            _body.ApplyRigidState(state);
            Remember(tick, state);
            if (tick == _beforeTick)
                FinishCorrection();
        }

        public void Prepare(int tick)
        {
            var previous = tick - 1;
            if (_effects.TryGetAffected(previous, _body.State(), out var affected))
                _body.ApplyRigidState(affected);

            var slot = Slot(previous);
            if (_shoveTicks[slot] == previous)
                _body.ApplyForce(_shoves[slot], _body.worldCenterOfMass, true);
        }

        public void Finish(int tick)
        {
            if (_body.TryGetLaunchSpeed(out var speed))
                _body.linearVelocity = _body.linearVelocity.WithY(speed);

            var state = _body.State();
            Remember(tick, state);
            if (_correcting && tick == _beforeTick)
                FinishCorrection();
        }

        public void PredictTouched(int tick, IPredictableCollection predictables)
        {
            if (tick - _activeTick > QuietTicks)
                return;

            using var contacts = _body.GetContacts(Allocator.Temp);
            for (var index = 0; index < contacts.Length; index++)
            {
                var contact = contacts[index];
                var other = contact.shapeA.body.Equals(_body) ? contact.shapeB.body : contact.shapeA.body;
                if (contact.manifold.pointCount > 0 && predictables.TryGetValue(other, out var touched))
                    touched.Predict(_activeTick);
            }
        }

        public void Verify(int tick)
        {
            if (_replicated.TryGetAt(int.MaxValue, out var latest) && latest.Tick > _checkedTick)
            {
                _checkedTick = latest.Tick;
                if (TryGetRecorded(latest.Tick, out var predicted) && Differs(predicted, latest.State))
                {
                    Remember(latest.Tick, latest.State);
                    _rollback.Request(latest.Tick);
                }
            }

            if (Quiet(tick))
                StopPredicting();
        }

        public bool TryGetShown(double tick, out RigidState shown)
        {
            if (!_predicted || !TryGetRecorded((int)Math.Floor(tick), out _))
            {
                shown = default;
                return false;
            }

            shown = RecordedAt(tick);
            return true;
        }

        public void Follow(double tick, RigidState shown)
        {
            _followed = shown;
            _followedTick = tick;
            if (!_predicted)
                Place(shown);
        }

        private void Affect(int tick)
        {
            Predict(tick);
            _rollback.Request(tick);
        }

        private void StartPredicting() //TODO: копия разгоняется от последнего снимка сервера своими толчками, своими предсказанными ударами и копиями, которых она касается, — чужие толчки и чужие удары приходят только поправками
        {
            var basis = Basis();
            _predicted = true;
            Array.Fill(_stateTicks, int.MinValue);
            Array.Fill(_shoveTicks, int.MinValue);
            _newest = int.MinValue;
            Remember(basis.Tick, basis.State);
            _body.type = PhysicsBody.BodyType.Dynamic;
            _body.ApplyRigidState(basis.State);
            _correcting = _followedTick >= basis.Tick;
            _beforeTick = (int)Math.Ceiling(_followedTick);
            _beforeShownTick = _followedTick;
            _before = _followed;
            _rollback.Track(_predictions.Next(), this);
            _rollback.Request(basis.Tick);
        }

        private void StopPredicting()
        {
            _followed = _body.State();
            _followedTick = _newest;
            _predicted = false;
            _correcting = false;
            _predictions.TerminateCurrent();
            _body.type = PhysicsBody.BodyType.Kinematic;
            _body.linearVelocity = Vector2.zero;
            _body.angularVelocity = 0f;
        }

        private TickedState<RigidState> Basis()
        {
            var earliest = _ticks.Current - Capacity / 2;
            if (!_replicated.TryGetAt(int.MaxValue, out var latest))
                return new TickedState<RigidState>(_ticks.Current, _followed);

            return latest.Tick >= earliest ? latest : new TickedState<RigidState>(earliest, latest.State);
        }

        private bool Quiet(int tick)
        {
            var newest = Recorded(_newest);
            return tick - _activeTick > QuietTicks
                   && newest.Velocity.magnitude < Resting
                   && Mathf.Abs(newest.AngularVelocity) < Turn
                   && _replicated.TryGetAt(int.MaxValue, out var latest)
                   && !Differs(newest, latest.State);
        }

        private void FinishCorrection()
        {
            _correcting = false;
            var after = RecordedAt(_beforeShownTick);
            if (!_before.Equals(after))
                _corrected.Fire(new StateCorrection<RigidState>(_before, after));
        }

        private void Place(RigidState state)
        {
            var pose = new PhysicsTransform(state.Position, new PhysicsRotate(state.Angle * Mathf.Deg2Rad));
            var current = _body.transform;
            if (current.position != pose.position || current.rotation.angle != pose.rotation.angle)
                _body.transform = pose;
        }

        private void Remember(int tick, RigidState state)
        {
            var slot = Slot(tick);
            _stateTicks[slot] = tick;
            _states[slot] = state;
            _newest = Math.Max(_newest, tick);
        }

        private bool TryGetRecorded(int tick, out RigidState state)
        {
            var slot = Slot(tick);
            state = _states[slot];
            return _stateTicks[slot] == tick;
        }

        private RigidState Recorded(int tick) =>
            _states[Slot(tick)];

        private RigidState RecordedOrNewest(int tick) =>
            TryGetRecorded(tick, out var state) ? state : Recorded(_newest);

        private RigidState RecordedAt(double tick)
        {
            var from = (int)Math.Floor(tick);
            var before = Recorded(from);
            var after = RecordedOrNewest(from + 1);
            var progress = (float)(tick - from);
            return new RigidState(
                Vector2.Lerp(before.Position, after.Position, progress),
                Mathf.LerpAngle(before.Angle, after.Angle, progress),
                Vector2.Lerp(before.Velocity, after.Velocity, progress),
                Mathf.Lerp(before.AngularVelocity, after.AngularVelocity, progress));
        }

        private RigidState Oldest()
        {
            var oldest = int.MaxValue;
            for (var slot = 0; slot < Capacity; slot++)
                if (_stateTicks[slot] != int.MinValue)
                    oldest = Math.Min(oldest, _stateTicks[slot]);

            return Recorded(oldest);
        }

        private static bool Differs(RigidState predicted, RigidState actual) =>
            Vector2.Distance(predicted.Position, actual.Position) > Distance
            || Mathf.Abs(Mathf.DeltaAngle(predicted.Angle, actual.Angle)) > Turn
            || Vector2.Distance(predicted.Velocity, actual.Velocity) > Speed;

        private static int Slot(int tick) =>
            (tick % Capacity + Capacity) % Capacity;

        private static int[] Unrecorded()
        {
            var ticks = new int[Capacity];
            Array.Fill(ticks, int.MinValue);
            return ticks;
        }
    }
}
