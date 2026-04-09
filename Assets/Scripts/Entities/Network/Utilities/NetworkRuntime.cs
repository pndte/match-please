#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Lifetimes;
using Unity.Netcode;

public sealed class NetworkYieldService
{
    private readonly NetworkManager _networkManager;

    public NetworkYieldService(NetworkManager networkManager)
    {
        _networkManager = networkManager;
    }

    public UniTask NetworkYield(Lifetime lifetime)
    {
        if (_networkManager == null || !_networkManager.IsListening || _networkManager.NetworkTickSystem == null)
            throw new Exception("newtorkManager not initialized");

        var promise = TickPromise.Rent();
        promise.Init(_networkManager, lifetime);
        return new UniTask(promise, promise.Version);
    }

    private sealed class TickPromise : IUniTaskSource, ITaskPoolNode<TickPromise>
    {
        private static TaskPool<TickPromise> _pool;

        public ref TickPromise? NextNode => ref _nextNode;

        private NetworkManager? _networkManager;
        private Action? _tickHandler;
        private short _version;

        private UniTaskCompletionSourceCore<AsyncUnit> _core;
        private TickPromise? _nextNode;

        public short Version => _version;

        public static TickPromise Rent()
        {
            if (!_pool.TryPop(out var node))
                node = new TickPromise();
            return node;
        }

        public void Init(NetworkManager networkManager, Lifetime lifetime)
        {
            _networkManager = networkManager;

            _tickHandler ??= OnTick;
            lifetime.OnTermination(Cancel);
            networkManager.NetworkTickSystem.Tick += _tickHandler;
        }

        private void OnTick()
        {
            CleanupSubscription();
            _core.TrySetResult(AsyncUnit.Default);
        }

        private void Cancel()
        {
            CleanupSubscription();
            _core.TrySetCanceled(CancellationToken.None);
        }

        private void CleanupSubscription()
        {
            var networkManager = _networkManager;
            var handler = _tickHandler;

            if (networkManager != null && handler != null && networkManager.NetworkTickSystem != null)
                networkManager.NetworkTickSystem.Tick -= handler;

            _networkManager = null;
        }

        public void GetResult(short token)
        {
            try
            {
                _core.GetResult(token);
            }
            finally
            {
                ReturnToPool();
            }
        }

        public UniTaskStatus GetStatus(short token) => _core.GetStatus(token);
        public UniTaskStatus UnsafeGetStatus() => _core.UnsafeGetStatus();

        public void OnCompleted(Action<object?> continuation, object? state, short token)
            => _core.OnCompleted(continuation, state, token);

        private void ReturnToPool()
        {
            _core.Reset();
            unchecked { _version++; }
            _pool.TryPush(this);
        }
    }
}