using System;
using UnityEditor;
using UnityEngine;

namespace Bw.EditorTools.Shooting
{
    public sealed class LinkedConfigSection : IDisposable
    {
        private readonly UnityEngine.Object _config;
        private readonly SerializedObject _serialized;
        private readonly string _title;
        private readonly string[] _properties;

        public LinkedConfigSection(UnityEngine.Object config, string title, params string[] properties)
        {
            _config = config;
            _serialized = new SerializedObject(config);
            _title = title;
            _properties = properties;
        }

        public void Draw()
        {
            EditorGUILayout.Space(8f);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField($"{_title} — {_config.name}", EditorStyles.boldLabel);
                if (GUILayout.Button("Ping", EditorStyles.miniButton, GUILayout.Width(44f)))
                    EditorGUIUtility.PingObject(_config);
            }

            _serialized.Update();
            foreach (var property in _properties)
                EditorGUILayout.PropertyField(_serialized.FindProperty(property), true);

            _serialized.ApplyModifiedProperties();
        }

        public void Dispose() =>
            _serialized.Dispose();
    }
}
