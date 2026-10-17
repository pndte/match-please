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
    //TODO: чудовищный класс, нуждающийся в декомпозиции
    public sealed class PredictedRigidBody : IShovable, IPredictable, IPredictedBody, IRollbackBody, IPredictedRigidBody, IStateCorrections<RigidState>
    {
        private const int QuietTicks = 30;
        private const float Resting = 0.05f;
        private const float Distance = 0.01f;
        private const float Turn = 1f;
        private const float Speed = 0.1f;

        public PhysicsBody Body => _body;
        public ISource<StateCorrection<RigidState>> Corrected => _corrections.Corrected;

        private readonly RigidStateHistory _states = new();
        private readonly ShoveHistory _shoves = new();
        private readonly RigidCorrections _corrections;
        private readonly SequentialLifetimes _predictions;
        private readonly PhysicsBody _body;
        private readonly IPhysicsRollback _rollback;
        private readonly IReplicatedStates<RigidState> _replicated;
        private readonly IRigidStateEffects _effects;
        private readonly INetworkTicks _ticks;

        private bool _predicted;
        private bool _reposed;
        private int _posedTick;
        private int _activeTick = int.MinValue;
        private int _launchedTick = int.MinValue;
        private int _checkedTick = int.MinValue;
        private double _followedTick;
        private RigidState _followed;

        public PredictedRigidBody(
            Lifetime lifetime,
            PhysicsBody body,
            IPhysicsRollback rollback,
            IReplicatedStates<RigidState> replicated,
            IRigidStateEffects effects,
            INetworkTicks ticks)
        {
            _corrections = new RigidCorrections(_states);
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

            var recorded = _shoves.At(_posedTick);
            if (_reposed)
                _shoves.Forget(_posedTick);

            _shoves.Add(_posedTick, force);
            _reposed = false;
            _activeTick = Math.Max(_activeTick, _posedTick);
            if (_posedTick < _ticks.Current && _shoves.At(_posedTick) != recorded)
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
            if (_predicted && _states.TryGetAt(tick, out var state))
                _body.ApplyRigidState(state);
        }

        public void Restore(int tick)
        {
            if (!_corrections.IsPending)
                _corrections.Remember(_states.NewestTick, _states.NewestTick, _states.Newest);

            var state = _states.TryGetAt(tick, out var recorded) ? recorded : _states.Oldest;
            _body.ApplyRigidState(state);
            _states.Record(tick, state);
            _corrections.Reach(tick);
        }

        public void Prepare(int tick)
        {
            var previous = tick - 1;
            if (_effects.TryGetAffected(previous, _body.State(), out var affected))
                _body.ApplyRigidState(affected);

            if (_shoves.TryGetAt(previous, out var shove))
                _body.ApplyForce(shove, _body.worldCenterOfMass, true);
        }

        public void Finish(int tick)
        {
            if (_body.TryGetLaunchSpeed(out var speed))
                _body.linearVelocity = _body.linearVelocity.WithY(speed);

            _states.Record(tick, _body.State());
            _corrections.Reach(tick);
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
            if (!_body.enabled)
            {
                StopPredicting();
                return;
            }

            if (_replicated.TryGetAt(int.MaxValue, out var latest) && latest.Tick > _checkedTick)
            {
                _checkedTick = latest.Tick;
                if (_states.TryGetAt(latest.Tick, out var predicted) && Differs(predicted, latest.State))
                {
                    _states.Record(latest.Tick, latest.State);
                    _rollback.Request(latest.Tick);
                }
            }

            if (Quiet(tick))
                StopPredicting();
        }

        public bool TryGetShown(double tick, out RigidState shown)
        {
            if (_predicted && tick < _launchedTick)
            {
                shown = _states.At(_launchedTick);
                return true;
            }

            if (!_predicted || !_states.TryGetAt((int)Math.Floor(tick), out _))
            {
                shown = default;
                return false;
            }

            shown = _states.Interpolated(tick);
            return true;
        }

        public void Follow(double tick, RigidState shown)
        {
            _followed = shown;
            _followedTick = tick;
            if (!_predicted)
                Place(shown);
        }

        public void Launch(RigidState state)
        {
            var tick = _ticks.Current;
            _predicted = true;
            _states.Clear();
            _shoves.Clear();
            _states.Record(tick, state);
            _body.type = PhysicsBody.BodyType.Dynamic;
            _body.ApplyRigidState(state);
            _corrections.Forget();
            _activeTick = tick;
            _launchedTick = tick;
            _rollback.Track(_predictions.Next(), this);
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
            _launchedTick = int.MinValue;
            _states.Clear();
            _shoves.Clear();
            _states.Record(basis.Tick, basis.State);
            _body.type = PhysicsBody.BodyType.Dynamic;
            _body.ApplyRigidState(basis.State);
            if (_followedTick >= basis.Tick)
                _corrections.Remember((int)Math.Ceiling(_followedTick), _followedTick, _followed);
            else
                _corrections.Forget();

            _rollback.Track(_predictions.Next(), this);
            _rollback.Request(basis.Tick);
        }

        private void StopPredicting()
        {
            _followed = _body.State();
            _followedTick = _states.NewestTick;
            _predicted = false;
            _corrections.Forget();
            _predictions.TerminateCurrent();
            _body.type = PhysicsBody.BodyType.Kinematic;
            _body.linearVelocity = Vector2.zero;
            _body.angularVelocity = 0f;
        }

        private TickedState<RigidState> Basis()
        {
            var earliest = _ticks.Current - RigidStateHistory.Capacity / 2;
            if (!_replicated.TryGetAt(int.MaxValue, out var latest))
                return new TickedState<RigidState>(_ticks.Current, _followed);

            return latest.Tick >= earliest ? latest : new TickedState<RigidState>(earliest, latest.State);
        }

        private bool Quiet(int tick)
        {
            var newest = _states.Newest;
            return tick - _activeTick > QuietTicks
                   && newest.Velocity.magnitude < Resting
                   && Mathf.Abs(newest.AngularVelocity) < Turn
                   && _replicated.TryGetAt(int.MaxValue, out var latest)
                   && !Differs(newest, latest.State);
        }

        private void Place(RigidState state)
        {
            var pose = new PhysicsTransform(state.Position, new PhysicsRotate(state.Angle * Mathf.Deg2Rad));
            var current = _body.transform;
            if (current.position != pose.position || current.rotation.angle != pose.rotation.angle)
                _body.transform = pose;
        }

        private static bool Differs(RigidState predicted, RigidState actual) =>
            Vector2.Distance(predicted.Position, actual.Position) > Distance
            || Mathf.Abs(Mathf.DeltaAngle(predicted.Angle, actual.Angle)) > Turn
            || Vector2.Distance(predicted.Velocity, actual.Velocity) > Speed;
    }
}
