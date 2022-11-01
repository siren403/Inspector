using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Inspector
{
    [CustomPropertyDrawer(typeof(PreviewAttribute))]
    public class PreviewPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.ObjectReference &&
                fieldInfo.FieldType == typeof(Sprite))
            {
                var container = new VisualElement();

                var field = new PropertyField(property);
                container.Add(field);

                var image = new VisualElement();
                container.Add(image);


                field.RegisterValueChangeCallback(e =>
                {
                    if (e.changedProperty.objectReferenceValue is Sprite sprite)
                    {
                        SetSprite(this, image, sprite);
                    }
                    else
                    {
                        image.style.backgroundImage = null;
                    }
                });

                if (property.objectReferenceValue != null && property.objectReferenceValue is Sprite spr)
                {
                    SetSprite(this, image, spr);
                }

                return container;
            }
            else
            {
                return new PropertyField(property);
            }
        }

        private void SetSprite(PropertyDrawer drawer, VisualElement container, Sprite sprite)
        {
            var height = 0;
            if (drawer.attribute is PreviewAttribute previewAttribute)
            {
                height = previewAttribute.Height;
            }

            // var texture = new StyleBackground(AssetPreview.GetAssetPreview(sprite));
            container.style.height = height == 0 ? sprite.textureRect.height : height;
            container.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
            container.style.backgroundImage = sprite.texture;
        }
    }
}