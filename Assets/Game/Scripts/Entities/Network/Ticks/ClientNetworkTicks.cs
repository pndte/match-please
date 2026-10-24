using System;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using Unity.Netcode;
using UnityEngine;

namespace Bw.Entities.Network.Ticks
{
    public sealed class ClientNetworkTicks : INetworkTicks, IWorldClock, IInterpolationTicks, IServerTime, IInputMarginFeedback, INetworkUpdateSystem
    {
        public float Duration { get; private set; }
        public int Current { get; private set; }
        public float Progress => Mathf.Clamp01((float)(_time / Duration - Current));
        public double InterpolationTick => _network.ServerTime.Time / Duration - _config.InterpolationDelayTicks;
        int IServerTime.Tick => (int)Math.Floor(_network.ServerTime.Time / Duration);
        public ISource<int> Ticked => _ticked;

        private readonly Signal<int> _ticked = new();
        private readonly NetworkTicksConfig _config;
        private NetworkManager _network;
        private double _time;
        private bool _synchronized;
        private float _leadTicks;
        private bool _leadMeasured; //tODO: многовато всего, мб декомпозировать
        private int _reportedMargin;
        private bool _reported;

        public ClientNetworkTicks(Lifetime lifetime, INetworkHolder networkHolder, NetworkTicksConfig config)
        {
            _config = config;
            _leadTicks = config.InitialLeadTicks; //TODO: до первого отчёта сервера (полсекунды и пинг) опережение — догадка: при пинге от ~40 мс ввод этого времени опаздывает, и сервер ещё не шагает персонажа

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

            if (_reported)
                CorrectLead();

            AdvanceTime(PredictedTime());

            var tick = (int)Math.Floor(_time / Duration);
            if (tick - Current > _config.MaxTicksPerFrame)
                Current = tick - 1;

            while (Current < tick)
                _ticked.Fire(++Current);
        }

        public void Report(int marginTicks)
        {
            _reportedMargin = _reported ? Math.Min(_reportedMargin, marginTicks) : marginTicks;
            _reported = true;
        }

        private void CorrectLead()
        {
            var error = _config.TargetMarginTicks - _reportedMargin;
            _reported = false;
            if (!_leadMeasured) //TODO: следующий отчёт ещё мерит ввод, отправленный со старым опережением, и добавляет ту же поправку второй раз (×0.5 или ×0.25) — после старта на высоком пинге опережение перелетает и несколько секунд возвращается
            {
                _leadMeasured = true;
                _synchronized = false;
                _leadTicks = ClampedLead(_leadTicks + error);
                return;
            }

            var rate = error > 0f ? _config.LeadRaiseRate : _config.LeadLowerRate;
            _leadTicks = ClampedLead(_leadTicks + error * rate);
        }

        private float ClampedLead(float leadTicks) =>
            Mathf.Clamp(leadTicks, _config.MinLeadTicks, _config.MaxLeadTicks);

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

        private double PredictedTime() =>
            _network.ServerTime.Time + _network.NetworkTimeSystem.ServerBufferSec + _leadTicks * Duration; //TODO: фриз клиента или сервера от ~0,2 с сбивает ServerTime: NGO раз в секунду присылает время сервера, не учитывая задержку обработки, и после фриза его время перескакивает назад, потом вперёд. Около секунды ввод опаздывает (клики теряются), опережение взлетает до MaxLeadTicks и несколько секунд спускается — другие видят игрока позже. Ограничить одну поправку опережения и не учитывать отчёты сразу после сброса часов или вести свою синхронизацию времени
    }
}
