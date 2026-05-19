using System;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using Unity.Netcode;
using UnityEngine;

namespace Bw.Entities.Network
{
    public static class NetworkLifetimeExtensions
    { // TODO: restyle
        public static void ConnectTo<T>(this NetworkVariable<T> variable, Lifetime lifetime, IViewableProperty<T> property)
        {
            property.SynchWithNetworkProperty(lifetime, variable);
            if (variable.CanWriteVariable())
                property.Advise(lifetime, x => variable.Value = x);
        }
        
        private static void SynchWithNetworkProperty<T>(this IViewableProperty<T> property, Lifetime lifetime, NetworkVariable<T> variable)
        {
            property.Value = variable.Value;

            var handler = new NetworkVariable<T>.OnValueChangedDelegate((_, newValue) => property.Value = newValue);
            variable.OnValueChanged += handler;
            lifetime.OnTermination(() => variable.OnValueChanged -= handler);
        }
        
        private static bool CanWriteVariable(this NetworkVariableBase variable)
        {
            var networkBehaviour = variable.GetBehaviour();
            if (networkBehaviour == null)
            {
                Debug.LogError(
                    $"Variable {variable.Name} не инициализирована");
                return false;
            }

            var localClientId = networkBehaviour.NetworkManager.LocalClientId;
            return variable.WritePerm switch
            {
                NetworkVariableWritePermission.Server => localClientId == NetworkManager.ServerClientId,
                NetworkVariableWritePermission.Owner  => localClientId == networkBehaviour.NetworkObject.OwnerClientId,
                _                                     => throw new ArgumentOutOfRangeException()
            };
        }
    }
}