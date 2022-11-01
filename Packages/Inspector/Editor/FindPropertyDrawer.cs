using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
                if (IsArrayProperty(property)) // Array elements
                {
                    FindComponents(property, findAttribute);
                }
                else if (string.IsNullOrWhiteSpace(findAttribute.Path))
                {
                    FindFirstComponent(property, findAttribute);
                }
                else if (TryGetFindInfo(property, out var findInfo))
                {
                    FindFromPath(property, findAttribute.Path, findInfo, findAttribute);
                }
            }

            return field;

            bool IsArrayProperty(SerializedProperty p) => property.name == "data";
        }

        private void FindComponents(SerializedProperty property, FindAttribute findAttribute)
        {
            if (property.serializedObject.targetObject is not Component component) return;

            var fieldName = property.propertyPath.Split(".").FirstOrDefault();
            if (string.IsNullOrEmpty(fieldName)) return;

            if (!_fieldToObjects.TryGetValue(fieldName, out var components))
            {
                var fieldInfo = component.GetType().GetField(
                    fieldName,
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic
                );

                if (fieldInfo == null) return;

                if (!TryFindGameObjectFromPath(component.gameObject, findAttribute.Path, out var result))
                    return;

                Type itemType = null;
                if (fieldInfo.FieldType.Name.Contains("List"))
                {
                    itemType = fieldInfo.FieldType.GenericTypeArguments.First();
                }
                else if (fieldInfo.FieldType.Name.Contains("[]"))
                {
                    itemType = fieldInfo.FieldType.GetElementType();
                }

                if (itemType == null) return;

                if (itemType == typeof(GameObject))
                {
                    components = result.Children()
                        .Select(_ => _ as Object)
                        .Where(_ => _ != null)
                        .ToList();
                }
                else
                {
                    components = result.Children()
                        .Select(_ => _.GetComponent(itemType) as Object)
                        .Where(_ => _ != null)
                        .ToList();
                }

                _fieldToObjects[fieldName] = components;
            }

            var index = Convert.ToInt32(property.displayName.Split(" ").Last());
            if (fieldInfo.GetValue(component) is IList list)
            {
                if (index < components.Count)
                {
                    list[index] = components[index];
                }

                if (index == list.Count - 1) // is last
                {
                    _fieldToObjects.Remove(fieldName);
                }
            }
        }

        private readonly struct FindInfo
        {
            public readonly Object TargetObject;
            public readonly GameObject RootGameObject;
            public readonly SerializedProperty Property;
            public readonly FieldInfo PropertyFieldInfo;
            public readonly Type FieldType;

            public FindInfo(
                Object targetObject,
                GameObject rootGameObject,
                SerializedProperty property,
                FieldInfo propertyFieldInfo,
                Type fieldType
            )
            {
                TargetObject = targetObject;
                RootGameObject = rootGameObject;
                Property = property;
                PropertyFieldInfo = propertyFieldInfo;
                FieldType = fieldType;
            }

            public static readonly FindInfo Empty = new FindInfo();
        }

        private string GetFindName(SerializedProperty property, FindAttribute findAttribute)
        {
            return string.IsNullOrWhiteSpace(findAttribute.Name)
                ? PascalToKebabCase(property.name)
                : findAttribute.Name.Contains("-")
                    ? findAttribute.Name
                    : PascalToKebabCase(findAttribute.Name);
        }

        private bool TryGetFindInfo(SerializedProperty property, out FindInfo info)
        {
            info = FindInfo.Empty;
            if (property.objectReferenceValue != null) return false;
            if (property.propertyType != SerializedPropertyType.ObjectReference) return false;

            var targetObject = property.serializedObject.targetObject;

            var targetField = targetObject.GetType()
                .GetField(
                    property.name,
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic
                );

            if (targetField == null) return false;

            var fieldType = targetField.FieldType;
            if (targetObject is not Component targetComponent) return false;

            info = new FindInfo(
                targetObject,
                targetComponent.gameObject,
                property,
                targetField,
                fieldType
            );

            return true;
        }

        private bool TryFindGameObjectFromPath(GameObject root, string path, out GameObject result)
        {
            if (string.IsNullOrEmpty(path))
            {
                result = root;
                return true;
            }

            var paths = path.Split("/");
            int pathIndex = 0;

            result = null;
            GameObject currentGameObject = root;
            var currentPath = paths[0];

            while (result == null)
            {
                var findGameObject = currentGameObject.Child(currentPath);
                if (findGameObject == null)
                {
                    Debug.LogError($"not found path: {currentGameObject.name} {currentPath}");
                    break;
                }

                if (++pathIndex == paths.Length)
                {
                    result = findGameObject;
                }
                else
                {
                    currentPath = paths[pathIndex];
                    currentGameObject = findGameObject;
                }
            }

            return result != null;
        }

        private void FindFromPath(SerializedProperty property, string path, FindInfo findInfo,
            FindAttribute findAttribute)
        {
            if (path.StartsWith("/")) //TODO: remove return
            {
                FindFromRoot(property, findInfo, findAttribute);
                return;
            }

            var paths = path.Split("/");
            int pathIndex = 0;

            Object findTarget = null;
            GameObject currentGameObject = findInfo.RootGameObject;
            var currentPath = paths[0];

            while (findTarget == null)
            {
                var findGameObject = currentGameObject.Child(currentPath);
                if (findGameObject == null)
                {
                    Debug.LogError($"not found path: {currentGameObject.name} {currentPath}");
                    EditorGUIUtility.PingObject(findInfo.RootGameObject);
                    break;
                }

                if (pathIndex == paths.Length - 1) // last path
                {
                    var findName = GetFindName(property, findAttribute);

                    var lastGameObject = findGameObject.Child(findName);
                    if (lastGameObject == null)
                    {
                        Debug.LogError($"not found field target: {findGameObject.name} {findName}");
                        EditorGUIUtility.PingObject(findGameObject);
                        break;
                    }

                    if (findInfo.FieldType == typeof(GameObject))
                    {
                        findTarget = lastGameObject;
                    }
                    else
                    {
                        findTarget = lastGameObject.GetComponent(findInfo.FieldType);
                        if (findTarget == null)
                        {
                            Debug.LogError($"not found component: {findInfo.FieldType.Name}");
                            EditorGUIUtility.PingObject(lastGameObject);
                            break;
                        }
                    }

                    Debug.Log($"find target: {findInfo.Property.name}");
                    break;
                }

                pathIndex++;
                if (pathIndex >= paths.Length)
                {
                    Debug.LogError($"out of path index: {path}");
                    break;
                }

                currentPath = paths[pathIndex];
                currentGameObject = findGameObject;
            }

            if (findTarget != null)
            {
                findInfo.PropertyFieldInfo.SetValue(findInfo.TargetObject, findTarget);
            }
        }

        private void FindFromRoot(SerializedProperty property, FindInfo findInfo, FindAttribute findAttribute)
        {
            var findName = GetFindName(property, findAttribute);

            var findGameObject = GameObject.Find(findName);
            if (findGameObject == null) return;

            if (findInfo.FieldType == typeof(GameObject))
            {
                findInfo.PropertyFieldInfo.SetValue(findInfo.TargetObject, findGameObject);
            }
            else if (findGameObject.TryGetComponent(fieldInfo.FieldType, out var component))
            {
                findInfo.PropertyFieldInfo.SetValue(findInfo.TargetObject, component);
            }
        }

        private void FindFirstComponent(SerializedProperty property, FindAttribute findAttribute)
        {
            if (property.objectReferenceValue != null) return;
            if (property.propertyType != SerializedPropertyType.ObjectReference) return;

            var targetObject = property.serializedObject.targetObject;
            var targetField = targetObject.GetType()
                .GetField(
                    property.name,
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic
                );

            if (targetField == null) return;

            var fieldType = targetField.FieldType;
            if (targetObject is not Component targetComponent) return;

            var root = targetComponent.gameObject;
            var findName = GetFindName(property, findAttribute);

            if (fieldType == typeof(GameObject))
            {
                var findObjects = root.Descendants()
                    .Where(_ => _.name.Equals(findName))
                    .Where(_ => _ != null)
                    .ToArray();
                if (findObjects.Length == 1)
                {
                    targetField.SetValue(targetObject, findObjects.First());
                }
                else
                {
                    Debug.LogError("find multiple");
                    foreach (var gameObject in findObjects)
                    {
                        EditorGUIUtility.PingObject(gameObject);
                    }
                }
            }
            else
            {
                var findComponents = root.Descendants()
                    .Where(_ => _.name.Equals(findName))
                    .Select(_ => _.GetComponent(fieldType))
                    .Where(_ => _ != null)
                    .ToArray();
                if (findComponents.Length == 1)
                {
                    targetField.SetValue(targetObject, findComponents.First());
                }
                else
                {
                    Debug.LogError("find multiple");
                    foreach (var component in findComponents)
                    {
                        EditorGUIUtility.PingObject(component.gameObject);
                    }
                }
            }
        }

        public static string PascalToKebabCase(string value)
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