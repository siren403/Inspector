using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Inspector
{
    [CustomPropertyDrawer(typeof(StringDropdownHookAttribute))]
    public class StringDropdownHookPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var target = property.serializedObject.targetObject;
            var targetType = target.GetType();
            if (attribute is not StringDropdownHookAttribute attr)
            {
                return new PropertyField(property);
            }

            var flags = BindingFlags.Instance |
                        BindingFlags.Public |
                        BindingFlags.NonPublic;
            var dropdownSourceMethod = targetType.GetMethod(attr.SourceMethodName, flags);
            var hookMethod = targetType.GetMethod(attr.MethodName, flags);
            if (dropdownSourceMethod == null || hookMethod == null) return null;

            var source = dropdownSourceMethod.Invoke(target, null);
            if (source is not List<string> strings) return null;

            var value = fieldInfo.GetValue(target) as string;
            if (value == null) return null;

            var index = strings.IndexOf(value);

            var container = new VisualElement()
            {
                style =
                {
                    flexDirection = FlexDirection.Row
                }
            };

            var field = new DropdownField(strings, index)
            {
                style =
                {
                    flexGrow = 1,
                    marginRight = 2
                }
            };


            field.RegisterValueChangedCallback(e =>
            {
                fieldInfo.SetValue(target, e.newValue);
                hookMethod.Invoke(target, null);
                EditorUtility.SetDirty(target);
            });

            container.Add(field);

            void MoveIndex(int amount)
            {
                var v = fieldInfo.GetValue(target) as string;
                var i = strings.IndexOf(v);
                var nextIndex = i + amount;
                if (nextIndex < 0 || nextIndex >= strings.Count) return;

                fieldInfo.SetValue(target, strings[nextIndex]);
                hookMethod.Invoke(target, null);
                EditorUtility.SetDirty(target);
                field.value = strings[nextIndex];
            }

            Button CreateButton(string label, Action onClick)
            {
                return new Button(onClick)
                {
                    text = label,
                    style =
                    {
                        marginLeft = 0,
                        marginRight = 0
                    }
                };
            }

            container.Add(CreateButton("<", () => MoveIndex(-1)));
            container.Add(CreateButton(">", () => MoveIndex(1)));

            return container;
        }
    }
}