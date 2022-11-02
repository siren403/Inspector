using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Inspector
{
#if !UNITY_2022
    [CustomEditor(typeof(Object), true, isFallback = true)]
#else
    [CustomEditor(typeof(Object), true)]
#endif
    public class DefaultInspector : UnityEditor.Editor
    {
#if !UNITY_2022
        private static VisualElement CreateScriptReadonlyField(SerializedProperty property)
        {
            var propertyField = new VisualElement {name = $"PropertyField:{property.propertyPath}"};
            var objectField = new ObjectField("Script") {name = "unity-input-m_Script"};
            objectField.BindProperty(property);
            // スペースキーを押してもスクリプトを選択するウィンドウを表示しないようにする
            objectField.focusable = false;
            propertyField.Add(objectField);
            propertyField.Q(null, "unity-object-field__selector")?.SetEnabled(false);
            propertyField.Q(null, "unity-base-field__label")?.AddToClassList("unity-disabled");
            propertyField.Q(null, "unity-base-field__input")?.AddToClassList("unity-disabled");
            return propertyField;
        }
#endif
        public override VisualElement CreateInspectorGUI()
        {
            var container = new VisualElement();
#if !UNITY_2022
            var iterator = serializedObject.GetIterator();
            
            if (iterator.NextVisible(true))
            {
                do
                {
                    var serializedProperty = iterator.Copy();
                    VisualElement propertyField;
            
                    if (iterator.propertyPath == "m_Script" && serializedObject.targetObject != null)
                    {
                        propertyField = CreateScriptReadonlyField(serializedProperty);
                    }
                    else
                    {
                        propertyField = new PropertyField(iterator.Copy())
                            {name = "PropertyField:" + iterator.propertyPath};
                    }
            
                    container.Add(propertyField);
                } while (iterator.NextVisible(false));
            }
#else
            InspectorElement.FillDefaultInspector(container, serializedObject, this);
#endif

            #region Button

            System.Type targetType = serializedObject.targetObject.GetType();
            while (targetType != null && targetType != typeof(MonoBehaviour) && targetType != typeof(ScriptableObject))
            {
                var methods = targetType
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(_ => _.GetCustomAttribute<ButtonAttribute>() != null)
                    .ToArray();

                foreach (var method in methods)
                {
                    // TODO: parameters
                    // var parameters = method.GetParameters();
                    var button = new Button(() => { method.Invoke(serializedObject.targetObject, null); })
                    {
                        text = method.Name
                    };
                    container.Add(button);
                }

                targetType = targetType.BaseType;
            }

            #endregion

            return container;
            // var container = new VisualElement();
            //
            // // IMGUI同様のInspectorを実装
            // InspectorElement.FillDefaultInspector(container, serializedObject, this);
            //
            // return container;
        }
    }
}
