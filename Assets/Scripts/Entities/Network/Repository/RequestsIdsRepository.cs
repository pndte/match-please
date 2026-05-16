using System;
using System.Collections.Generic;

namespace Bw.Entities.Network.Repository
{
    public interface IRequestIdsRepository //TODO: rename
    {
        public bool ContainsIdFor<TDto>(ulong id);
        public ulong NextIdFor<TDto>();
        public void RemoveIdFor<TDto>(ulong id);
    }
    public class RequestIdsRepository : IRequestIdsRepository
    {
        private readonly Dictionary<Type, Pair> _byType = new();
        
        public bool ContainsIdFor<TDto>(ulong id)
        {
            return _byType[typeof(TDto)].UniqueIds.Contains(id);
        }

        public ulong NextIdFor<TDto>()
        {
            var type = typeof(TDto);
            
            if (!_byType.TryGetValue(type, out var pair))
            {
                pair = new Pair();
                _byType[type] = pair;
            }

            pair.Counter++;
            pair.UniqueIds.Add(pair.Counter);
            
            return pair.Counter;
        }

        public void RemoveIdFor<TDto>(ulong id)
        {
            _byType[typeof(TDto)].UniqueIds.Remove(id);
        }

        private class Pair
        {
            public readonly HashSet<ulong> UniqueIds = new();
            public ulong Counter = 0;
        }
    }
}