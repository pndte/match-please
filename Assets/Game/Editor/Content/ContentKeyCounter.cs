using System;
using System.IO;
using UnityEngine;

namespace Bw.EditorTools.Content
{
    [Serializable]
    public sealed class ContentKeyCounter
    {
        private const string FilePath = "ProjectSettings/ContentKeys.json";

        [SerializeField] private int _next = 1;

        public int Next => _next;

        public static ContentKeyCounter Saved() =>
            File.Exists(FilePath) ? JsonUtility.FromJson<ContentKeyCounter>(File.ReadAllText(FilePath)) : new ContentKeyCounter();

        public void Advance() =>
            _next++;

        public void RaiseAbove(int key)
        {
            if (key >= _next)
                _next = key + 1;
        }

        public void Save() =>
            File.WriteAllText(FilePath, JsonUtility.ToJson(this, true));
    }
}
