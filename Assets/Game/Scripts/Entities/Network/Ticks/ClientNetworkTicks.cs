using System;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using Unity.Netcode;
using UnityEngine;

namespace Bw.Entities.Network.Ticks
{
    public sealed class ClientNetworkTicks : INetworkTicks, IInterpolationTicks, IInputMarginFeedback, INetworkUpdateSystem
    {
        public float Duration { get; private set; }
        public int Current { get; private set; }
        public float Progress => Mathf.Clamp01((float)(_time / Duration - Current));
        public double InterpolationTick => _network.ServerTime.Time / Duration - _config.InterpolationDelayTicks;
        public ISource<int> Ticked => _ticked;

        private readonly Signal<int> _ticked = new();
        private readonly NetworkTicksConfig _config;
        private NetworkManager _network;
        private double _time;
        private bool _synchronized;
        private float _leadTicks;

        public ClientNetworkTicks(Lifetime lifetime, INetworkHolder networkHolder, NetworkTicksConfig config)
        {
            _config = config;
            _leadTicks = config.InitialLeadTicks;

            networkHolder.NetworkManager.AdviseNotNull(lifetime, network =>
            {
                _network = network;
                Duration = 1f / network.NetworkConfig.TickRate;

                this.RegisterNetworkUpdate(NetworkUpdateStage.PreUpdate);
                lifetime.OnTermination(() => this.UnregisterNetworkUpdate(NetworkUpdateStage.PreUpdate));
            });
        }

        public void NetworkUpdate(NetworkUpdateStage updateStage)
        {
            if (!_network.IsConnectedClient)
                return;

            AdvanceTime(PredictedTime());

            var tick = (int)Math.Floor(_time / Duration);
            if (tick - Current > _config.MaxTicksPerFrame)
                Current = tick - 1;

            while (Current < tick)
                _ticked.Fire(++Current);
        }

        public void Report(int marginTicks)
        {
            var error = _config.TargetMarginTicks - marginTicks;
            var rate = error > 0f ? _config.LeadRaiseRate : _config.LeadLowerRate;
            _leadTicks = Mathf.Clamp(_leadTicks + error * rate, _config.MinLeadTicks, _config.MaxLeadTicks);
        }

        private void AdvanceTime(double target)
        {
            var error = target - _time;
            if (!_synchronized || Math.Abs(error) > _config.HardResetSeconds)
            {
                _time = target;
                _synchronized = true;
                return;
            }

            var correction = Math.Clamp(error, -_config.MaxTimeScaleCorrection, _config.MaxTimeScaleCorrection);
            _time += Time.unscaledDeltaTime * (1d + correction);
        }

        private double PredictedTime()
        {
            var roundTrip = _network.NetworkConfig.NetworkTransport.GetCurrentRtt(NetworkManager.ServerClientId) / 1000d;
            return _network.ServerTime.Time + _network.NetworkTimeSystem.ServerBufferSec + roundTrip + _leadTicks * Duration;
        }
    }
}
