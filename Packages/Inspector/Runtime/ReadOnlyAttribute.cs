using UnityEngine;
using System;

namespace Inspector
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ReadOnlyAttribute : PropertyAttribute
    {
        public bool onlyInPlaymode = false;
    }
}