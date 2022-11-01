using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Inspector
{
    [AttributeUsage(AttributeTargets.Field)]
    public class DropdownHookAttribute : HookAttribute
    {
        public string SourceMethodName { get; private set; }

        public DropdownHookAttribute(string sourceMethodName, string methodName)
            : base(methodName)
        {
            SourceMethodName = sourceMethodName;
        }

        public virtual VisualElement CreateDropdownField(
            Type targetType,
            Object target,
            FieldInfo fieldInfo,
            MethodInfo hookMethod)
        {
            return null;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class StringDropdownHookAttribute : DropdownHookAttribute
    {
        public StringDropdownHookAttribute(string sourceMethodName, string methodName) : base(sourceMethodName,
            methodName)
        {
        }

#if UNITY_EDITOR
        public override VisualElement CreateDropdownField(
            Type targetType,
            Object target,
            FieldInfo fieldInfo,
            MethodInfo hookMethod)
        {
            var flags = BindingFlags.Instance |
                        BindingFlags.Public |
                        BindingFlags.NonPublic;
            var dropdownSourceMethod = targetType.GetMethod(SourceMethodName, flags);

            if (dropdownSourceMethod == null) return null;

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
                    flexGrow = 1
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
#endif
    }
}