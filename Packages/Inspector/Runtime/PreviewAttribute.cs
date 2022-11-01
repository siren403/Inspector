using System;
using UnityEngine;

namespace Inspector
{
    [AttributeUsage(AttributeTargets.Field)]
    public class PreviewAttribute : PropertyAttribute
    {
        public readonly int Height = 0;

        public PreviewAttribute()
        {
        }

        public PreviewAttribute(int height)
        {
            Height = height;
        }
    }
}