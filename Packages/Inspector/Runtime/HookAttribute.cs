using System;
using UnityEngine;

namespace Inspector
{
    [AttributeUsage(AttributeTargets.Field)]
    public class HookAttribute : PropertyAttribute
    {
        public string MethodName { get; private set; }

        public HookAttribute(string methodName)
        {
            MethodName = methodName;
        }
    }
}