using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Inspector
{
    [CustomPropertyDrawer(typeof(HookAttribute))]
    public class HookPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            // TODO: range support
            // if (property.propertyType == SerializedPropertyType.Float)
            // {
            //     var floatField = property.serializedObject.targetObject.GetType().GetField(property.name);
            //     var rangeFloat = floatField.GetCustomAttribute<HookRangeFloatAttribute>();
            //     if (rangeFloat != null)
            //     {
            //         var rangeField = new SliderInt()
            //         return null;
            //     }
            // }

            var field = new PropertyField(property);

            var target = property.serializedObject.targetObject;
            var targetType = target.GetType();
            if (attribute is not HookAttribute attr)
            {
                return field;
            }

            var flags = BindingFlags.Instance |
                        BindingFlags.Public |
                        BindingFlags.NonPublic;
            var hookMethod = targetType.GetMethod(attr.MethodName, flags);

            if (hookMethod == null) return field;

            field.RegisterValueChangeCallback(e => { hookMethod.Invoke(target, null); });
            return field;
        }
    }
}