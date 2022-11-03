using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Inspector.Unity.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Inspector
{
    [CustomPropertyDrawer(typeof(FindAttribute))]
    public class FindPropertyDrawer : PropertyDrawer
    {
        private readonly Dictionary<string, List<Object>> _fieldToObjects = new();

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var field = new PropertyField(property);
            if (attribute is FindAttribute findAttribute)
            {
                // field.label = $"[*] {property.displayName}";
                FindComponentFromPath(property, findAttribute.Path, findAttribute.Name);
            }

            return field;
        }

        private void FindComponentFromPath(SerializedProperty property, string path, string name)
        {
            var isListElement = IsListElement(property);
            if (!IsNullProperty(property) && !isListElement) return;
            if (!IsAttachableType(property))
            {
                return;
            }

            var targetObject = property.serializedObject.targetObject;
            if (targetObject is not Component component) return;

            GameObject currentGameObject = component.gameObject;

            var additionalPath = GetAdditionalPath();
            string[] paths = null;
            if (string.IsNullOrWhiteSpace(path) || path == ".") // self
            {
                paths = Array.Empty<string>();
            }
            else if (path.StartsWith("/")) // from root
            {
                if (!TryInitializeFromRoot(path, additionalPath, currentGameObject, out var initial))
                {
                    return;
                }

                (paths, currentGameObject) = initial;
            }
            else if (isListElement) // IList element 
            {
                paths = path.Split("/");
            }
            else // from self to children
            {
                paths = $"{path}/{additionalPath}".Split("/");
            }

            
            var pathIndex = 0;
            // skip self path
            if (paths.Any() && paths[pathIndex] == ".")
            {
                pathIndex++;
            }

            do
            {
                var currentPath = paths.Any() ? paths[pathIndex] : string.Empty;

                if (!string.IsNullOrWhiteSpace(currentPath))
                {
                    if (TryFindChild(currentGameObject, currentPath, out var lastChild))
                    {
                        currentGameObject = lastChild;
                    }
                    else
                    {
                        Debug.LogError($"not found last child: {property.name}, {currentPath}");
                        return;
                    }
                }

                pathIndex++;
            } while (pathIndex < paths.Length);

            if (isListElement)
            {
                SetListElement(property, component, currentGameObject, _fieldToObjects);
            }
            else
            {
                SetField(property, currentGameObject, component);
            }

            string GetAdditionalPath()
            {
                return string.IsNullOrWhiteSpace(name)
                    ? PascalToKebabCase(property.name)
                    : name.Contains("-")
                        ? name
                        : PascalToKebabCase(name);
            }
        }

        bool TryInitializeFromRoot(string path, string additionalPath,
            GameObject startGameObject, out (string[] paths, GameObject rootGameObject) result)
        {
            result = (null, null);

            var removeRootPath =
                (path.Length == 1 ? $"{path}{additionalPath}" : $"{path}/{additionalPath}").Substring(1);
            result.paths = removeRootPath.Split("/");

            var findName = result.paths.First();
            result.rootGameObject = startGameObject.scene.GetRootGameObjects()
                .FirstOrDefault(_ => _.name == findName);

            if (result.rootGameObject == null)
            {
                Debug.LogError($"not found root gameobject: {findName}");
                return false;
            }

            // rebuild path
            if (result.paths.Any())
            {
                var removeFindNamePath = removeRootPath.Remove(0, findName.Length);
                if (removeFindNamePath.StartsWith("/"))
                {
                    removeFindNamePath = removeFindNamePath.Remove(0, 1);
                }

                result.paths = string.IsNullOrWhiteSpace(removeFindNamePath)
                    ? Array.Empty<string>()
                    : removeFindNamePath.Split("/");
            }

            return true;
        }

        void SetField(SerializedProperty property, GameObject destGameObject, Component targetComponent)
        {
            var fieldType = fieldInfo.FieldType;
            bool fieldTypeIsGameObject = fieldType == typeof(GameObject);

            if (destGameObject == targetComponent.gameObject && fieldTypeIsGameObject)
            {
                fieldInfo.SetValue(targetComponent, destGameObject);
                EditorGUIUtility.PingObject(destGameObject);
                Debug.LogError($"use this.gameobject -> {destGameObject.name}.{property.name}");
                return;
            }

            Object findObject = null;
            if (fieldTypeIsGameObject)
            {
                findObject = destGameObject;
            }
            else
            {
                findObject = destGameObject.GetComponent(fieldType);
                if (findObject == null)
                {
                    Debug.LogError($"not found component: {fieldType.Name}");
                    EditorGUIUtility.PingObject(destGameObject);
                    return;
                }
            }

            if (findObject != null)
            {
                fieldInfo.SetValue(targetComponent, findObject);
                Debug.Log($"find target: {property.name}");
            }
            else
            {
                Debug.LogError($"not found reference: {fieldType.Name}");
            }
        }

        void SetListElement(SerializedProperty property, object targetObject, GameObject destGameObject,
            Dictionary<string, List<Object>> cache)
        {
            var fieldName = property.propertyPath.Split(".").FirstOrDefault();
            if (string.IsNullOrEmpty(fieldName)) return;

            if (!cache.TryGetValue(fieldName, out var objects))
            {
                Type fieldType = null;
                if (fieldInfo.FieldType.Name.Contains("List"))
                {
                    fieldType = fieldInfo.FieldType.GenericTypeArguments.First();
                }
                else if (fieldInfo.FieldType.Name.Contains("[]"))
                {
                    fieldType = fieldInfo.FieldType.GetElementType();
                }

                if (fieldType == null) return;
                bool fieldTypeIsGameObject = fieldType == typeof(GameObject);

                if (fieldTypeIsGameObject)
                {
                    objects = destGameObject.Children()
                        .Select(_ => _ as Object)
                        .Where(_ => _ != null)
                        .ToList();
                }
                else
                {
                    objects = destGameObject.Children()
                        .Select(_ => _.GetComponent(fieldType) as Object)
                        .Where(_ => _ != null)
                        .ToList();
                }

                cache[fieldName] = objects;
            }

            var index = Convert.ToInt32(property.displayName.Split(" ").Last());
            if (fieldInfo.GetValue(targetObject) is IList list)
            {
                if (index < objects.Count)
                {
                    list[index] = objects[index];
                }
                else
                {
                    list[index] = null;
                    Debug.LogError($"not found components: {fieldName}[{index}]");
                }

                if (index == list.Count - 1) // is last
                {
                    cache.Remove(fieldName);
                }
            }
        }

        bool IsNullProperty(SerializedProperty property)
        {
            return property.objectReferenceValue == null ||
                   fieldInfo.GetValue(property.serializedObject.targetObject) == null;
        }


        bool IsAttachableType(SerializedProperty property)
        {
            return property.propertyType == SerializedPropertyType.ObjectReference &&
                   (fieldInfo.FieldType == typeof(GameObject) || fieldInfo.FieldType.BaseType != typeof(Object));
        }

        bool IsListElement(SerializedProperty p) => p.name == "data";

        bool TryFindChild(GameObject target, string childName, out GameObject child)
        {
            child = target.Child(childName);
            return child != null;
        }

        static string PascalToKebabCase(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            return Regex.Replace(
                    value,
                    "(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z])",
                    "-$1",
                    RegexOptions.Compiled)
                .Trim()
                .ToLower();
        }
    }
}