using System;
using System.Collections.Generic;
using System.Linq;
using Bw.Entities.Utilities;
using UnityEditor;
using UnityEngine;

namespace Bw.EditorTools.Inspector
{
    [CustomPropertyDrawer(typeof(TypePickerAttribute))]
    public sealed class TypePickerDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
            Children(property).Aggregate(EditorGUIUtility.singleLineHeight,
                (height, child) => height + EditorGUIUtility.standardVerticalSpacing + EditorGUI.GetPropertyHeight(child, true));

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (Shared(property))
                Unshare(property);

            EditorGUI.BeginProperty(position, label, property);
            var line = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            var picker = EditorGUI.PrefixLabel(line, label);
            if (EditorGUI.DropdownButton(picker, new GUIContent(Name(property.managedReferenceFullTypename)), FocusType.Keyboard))
                Menu(property).DropDown(picker);

            EditorGUI.indentLevel++;
            var y = line.yMax;
            foreach (var child in Children(property))
            {
                var height = EditorGUI.GetPropertyHeight(child, true);
                y += EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, height), child, true);
                y += height;
            }

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }

        private GenericMenu Menu(SerializedProperty property)
        {
            var serializedObject = property.serializedObject;
            var path = property.propertyPath;
            var menu = new GenericMenu();
            foreach (var type in TypeCache.GetTypesDerivedFrom(fieldInfo.FieldType).Where(Pickable).OrderBy(type => type.Name))
                menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(type.Name)), FullName(type) == property.managedReferenceFullTypename, () =>
                {
                    serializedObject.Update();
                    serializedObject.FindProperty(path).managedReferenceValue = Activator.CreateInstance(type);
                    serializedObject.ApplyModifiedProperties();
                });
            return menu;
        }

        private static bool Shared(SerializedProperty property)
        {
            if (property.managedReferenceFullTypename.Length == 0)
                return false;

            var other = property.serializedObject.GetIterator();
            while (other.Next(true))
                if (other.propertyType == SerializedPropertyType.ManagedReference && other.propertyPath != property.propertyPath &&
                    other.managedReferenceId == property.managedReferenceId)
                    return true;
            return false;
        }

        private static void Unshare(SerializedProperty property)
        {
            var value = property.managedReferenceValue;
            var copy = Activator.CreateInstance(value.GetType());
            EditorJsonUtility.FromJsonOverwrite(EditorJsonUtility.ToJson(value), copy);
            property.managedReferenceValue = copy;
            property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static bool Pickable(Type type) =>
            !type.IsAbstract && !type.IsGenericType && type.IsDefined(typeof(SerializableAttribute), false) &&
            !typeof(UnityEngine.Object).IsAssignableFrom(type) && type.GetConstructor(Type.EmptyTypes) != null;

        private static string FullName(Type type) =>
            $"{type.Assembly.GetName().Name} {type.FullName}";

        private static string Name(string fullTypename)
        {
            if (fullTypename.Length == 0)
                return "None";

            var typeName = fullTypename.Substring(fullTypename.IndexOf(' ') + 1);
            return ObjectNames.NicifyVariableName(typeName.Substring(typeName.LastIndexOf('.') + 1));
        }

        private static IEnumerable<SerializedProperty> Children(SerializedProperty property)
        {
            var end = property.GetEndProperty();
            var child = property.Copy();
            var enter = true;
            while (child.Next(enter) && !SerializedProperty.EqualContents(child, end))
            {
                enter = false;
                yield return child.Copy();
            }
        }
    }
}
