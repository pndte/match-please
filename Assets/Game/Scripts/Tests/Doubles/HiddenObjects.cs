using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bw.Tests.Doubles
{
    public sealed class HiddenObjects : IDisposable
    {
        private readonly List<GameObject> _created = new();

        public GameObject Create(string name, Vector2 position = default, int layer = 0)
        {
            var created = EditorUtility.CreateGameObjectWithHideFlags(name, HideFlags.HideAndDontSave);
            created.transform.position = position;
            created.layer = layer;
            _created.Add(created);
            return created;
        }

        public T Instantiate<T>(T prefab) where T : Object
        {
            var instance = Object.Instantiate(prefab);
            var root = instance switch
            {
                GameObject gameObject => gameObject,
                Component component => component.gameObject,
                _ => throw new ArgumentException($"{prefab.name} is neither a GameObject nor a Component.", nameof(prefab))
            };

            root.hideFlags = HideFlags.HideAndDontSave;
            _created.Add(root);
            return instance;
        }

        public void Dispose()
        {
            for (var index = _created.Count - 1; index >= 0; index--)
                if (_created[index] != null)
                    Object.DestroyImmediate(_created[index]);

            _created.Clear();
        }
    }
}
