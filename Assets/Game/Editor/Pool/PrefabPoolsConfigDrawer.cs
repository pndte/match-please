using System.Collections.Generic;
using System.Linq;
using Bw.Entities.Pool.GameObjects;
using UnityEditor;
using UnityEngine;

namespace Bw.EditorTools.Pool
{
    [CustomPropertyDrawer(typeof(PrefabPoolsConfig))]
    public sealed class PrefabPoolsConfigDrawer : PropertyDrawer
    {
        private static float ProblemHeight => EditorGUIUtility.singleLineHeight * 2f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
            EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing +
            EditorGUI.GetPropertyHeight(Prefabs(property), label, true) +
            Problems(property).Count() * (EditorGUIUtility.standardVerticalSpacing + ProblemHeight);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var unlisted = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(unlisted, property.FindPropertyRelative(nameof(PrefabPoolsConfig.UnlistedLimit)));

            var prefabs = Prefabs(property);
            var y = unlisted.yMax + EditorGUIUtility.standardVerticalSpacing;
            var height = EditorGUI.GetPropertyHeight(prefabs, label, true);
            EditorGUI.PropertyField(new Rect(position.x, y, position.width, height), prefabs, label, true);

            y += height;
            foreach (var problem in Problems(property))
            {
                y += EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.HelpBox(new Rect(position.x, y, position.width, ProblemHeight), problem, MessageType.Error);
                y += ProblemHeight;
            }
        }

        private static SerializedProperty Prefabs(SerializedProperty property) =>
            property.FindPropertyRelative(nameof(PrefabPoolsConfig.Prefabs));

        private static IEnumerable<string> Problems(SerializedProperty property)
        {
            var prefabs = Prefabs(property);
            var listed = new HashSet<GameObject>();
            for (var index = 0; index < prefabs.arraySize; index++)
            {
                var pool = prefabs.GetArrayElementAtIndex(index);
                var prefab = (GameObject)pool.FindPropertyRelative(nameof(PrefabPoolConfig.Prefab)).objectReferenceValue;
                if (prefab == null)
                {
                    yield return $"Pool {index} has no prefab.";
                    continue;
                }

                if (!listed.Add(prefab))
                    yield return $"{prefab.name} is in the list twice.";
                if (pool.FindPropertyRelative(nameof(PrefabPoolConfig.Prewarm)).intValue > pool.FindPropertyRelative(nameof(PrefabPoolConfig.Limit)).intValue)
                    yield return $"{prefab.name}: Prewarm is above Limit, the pool can't keep that many free objects.";
                if (pool.FindPropertyRelative(nameof(PrefabPoolConfig.Mode)).managedReferenceFullTypename.Length == 0)
                    yield return $"{prefab.name} has no cap mode.";
            }
        }
    }
}
