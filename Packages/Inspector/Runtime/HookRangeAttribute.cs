using System;
using UnityEngine;

namespace Inspector
{
    [AttributeUsage(AttributeTargets.Field)]
    public class HookRangeAttribute : PropertyAttribute
    {
        public readonly string RangeMethodName;
        public readonly string HookMethodName;

        public HookRangeAttribute(string rangeMethodName, string hookMethodName)
        {
            RangeMethodName = rangeMethodName;
            HookMethodName = hookMethodName;
        }
        
        public struct Range
        {
            public int Min;
            public int Max;
        }
    }
}