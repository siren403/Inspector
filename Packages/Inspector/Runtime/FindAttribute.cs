using System;
using UnityEngine;

namespace Inspector
{
    [AttributeUsage(AttributeTargets.Field)]
    public class FindAttribute : PropertyAttribute
    {
        public string Path { get; private set; }
        public string Name { get; private set; }

        public FindAttribute()
        {
            Path = string.Empty;
            Name = string.Empty;
        }

        public FindAttribute(string path)
        {
            Path = path;
            Name = string.Empty;
        }

        public FindAttribute(string path = null, string name = null)
        {
            Path = path ?? string.Empty;
            Name = name ?? string.Empty;
        }

    }
}