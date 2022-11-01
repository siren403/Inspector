using UnityEditor;
using UnityEngine;

namespace Inspector
{
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public sealed class ReadOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // PropertyDrawerHelper.LoadAttributeTooltip( this, label );

            if (attribute is ReadOnlyAttribute readOnlyAttribute)
            {
                var enabled = GUI.enabled;
                GUI.enabled = false;
                EditorGUI.PropertyField(position, property, label, true);
                GUI.enabled = enabled;

                // var readOnlyAttribute = attribute as ReadOnlyAttribute;
                //
                // using( DisabledGroup.Do( !readOnlyAttribute.onlyInPlaymode || EditorApplication.isPlayingOrWillChangePlaymode ) )
                // {
                // 	EditorGUI.PropertyField( position, property, label, true );
                // }
            }
        }
    }
}