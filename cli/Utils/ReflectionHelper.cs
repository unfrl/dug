using System;

namespace dug.Utils
{
    public static class ReflectionHelper
    {
        public static object GetDefaultValue(Type t)
        {
            if (t.IsValueType)
                return Activator.CreateInstance(t);

            return null;
        }

        public static string GetDefaultValueAsString(Type t)
        {
           return GetDefaultValue(t)?.ToString() ?? "";
        }
    }
}