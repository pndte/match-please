using UnityEngine;

namespace Bw.Entities.Content
{
    public abstract class ContentAsset : ScriptableObject
    {
        public const string KeyField = nameof(_key);

        [SerializeField] private int _key;

        public int Key => _key;
    }
}
