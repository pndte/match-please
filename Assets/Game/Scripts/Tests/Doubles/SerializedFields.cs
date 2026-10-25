using UnityEditor;
using UnityEngine;

namespace Bw.Tests.Doubles
{
    public static class SerializedFields
    {
        public static T With<T>(this T target, string field, Vector2 value) where T : Object
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(field).vector2Value = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return target;
        }

        public static T With<T>(this T target, string field, float value) where T : Object
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(field).floatValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return target;
        }

        public static T With<T>(this T target, string field, int value) where T : Object
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(field).intValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return target;
        }

        public static T With<T>(this T target, string field, bool value) where T : Object
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(field).boolValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return target;
        }

        public static T With<T>(this T target, string field, Object value) where T : Object
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(field).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return target;
        }
    }
}
