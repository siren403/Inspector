using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Inspector;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace MetaRun.Editor.Extensions
{
    public static class InspectorExtensions
    {
        public static void AddProperties(this UnityEditor.Editor editor,
            VisualElement container,
            Dictionary<string, PropertyField> cache)
        {
            var serializedObject = editor.serializedObject;
            var iterator = serializedObject.GetIterator();
            if (iterator.NextVisible(true))
            {
                do
                {
                    var serializedProperty = iterator.Copy();
                    var propertyField = new PropertyField(serializedProperty)
                        {name = "PropertyField:" + iterator.propertyPath};

                    if (iterator.propertyPath == "m_Script" && serializedObject.targetObject != null)
                    {
                        propertyField.SetEnabled(value: false);
                        var scriptContainer = new VisualElement()
                        {
                            style = {flexDirection = FlexDirection.Row}
                        };
                        propertyField.style.flexGrow = 1;
                        scriptContainer.Add(propertyField);
                        scriptContainer.Add(new Button(() =>
                        {
                            EditorGUIUtility.PingObject(serializedProperty.objectReferenceValue);
                        }) {text = "*"});

                        container.Add(scriptContainer);
                    }
                    else
                    {
                        container.Add(propertyField);
                    }

                    if (cache != null)
                    {
                        cache.Add(iterator.name, propertyField);
                    }
                } while (iterator.NextVisible(false));
            }
        }

        public static void AddProperties(this UnityEditor.Editor editor, VisualElement container)
        {
            editor.AddProperties(container, null);
        }

        public static void AddButtons(this UnityEditor.Editor editor, VisualElement container)
        {
            var target = editor.target;
            var buttonMethods = target.GetType()
                .GetMethods(
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic
                )
                .Where(_ => _.GetCustomAttribute<ButtonAttribute>() != null);

            foreach (var method in buttonMethods)
            {
                //TODO: method args
                var m = method;
                container.Add(new Button(() => { m.Invoke(target, null); })
                {
                    text = method.Name
                });
            }
        }
    }
}