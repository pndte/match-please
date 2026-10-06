using System;
using Cysharp.Threading.Tasks;
using JetBrains.Lifetimes;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Bw.Entities.Extensions.Addressable
{
    public static class AssetReferenceExtensions
    {
        public static async UniTask<T> LoadAssetAsync<T>(this AssetReference reference, Lifetime lifetime) where T : class
        {
            lifetime.ThrowIfNotAlive();

            var handle = Addressables.LoadAssetAsync<T>(reference);

            try
            {
                var asset = await handle.WithCancellation(lifetime);
                if (asset == null)
                    throw new InvalidOperationException($"Failed to load asset for reference: {reference}.");

                lifetime.OnTermination(() => Release(handle));
                return asset;
            }
            catch
            {
                Release(handle);
                throw;
            }
        }

        private static void Release<T>(AsyncOperationHandle<T> handle)
        {
            if (handle.IsValid())
                Addressables.Release(handle);
        }
    }
}
