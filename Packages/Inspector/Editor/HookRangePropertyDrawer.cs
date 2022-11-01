using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Inspector
{
    [CustomPropertyDrawer(typeof(HookRangeAttribute))]
    public class HookRangePropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            if (attribute is HookRangeAttribute hookRange && property.propertyType == SerializedPropertyType.Integer)
            {
                var flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                var target = property.serializedObject.targetObject;
                var targetType = property.serializedObject.targetObject.GetType();
                var rangeMethod = targetType.GetMethod(hookRange.RangeMethodName, flags);
                var hookMethod = targetType.GetMethod(hookRange.HookMethodName, flags);

                if (rangeMethod != null && hookMethod != null &&
                    rangeMethod.Invoke(target, null) is ValueTuple<int, int> range)
                {
                    var slider = new SliderInt($"{property.name} ({property.intValue})", range.Item1, range.Item2);
                    slider.value = property.intValue;
                    slider.RegisterValueChangedCallback(e =>
                    {
                        fieldInfo.SetValue(target, e.newValue);
                        slider.label = $"{property.name} ({e.newValue})";
                        hookMethod.Invoke(target, null);
                        EditorUtility.SetDirty(target);
                    });
                    return slider;
                }
            }

            return new PropertyField(property);
        }
    }
}