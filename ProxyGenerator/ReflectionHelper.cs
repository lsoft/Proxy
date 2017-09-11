using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ProxyGenerator
{
    internal static class ReflectionHelper
    {
        public static bool IsMethodInfoIsProperty(MethodInfo mi)
        {
            if (mi == null)
            {
                throw new ArgumentNullException("mi");
            }

            return
                mi.IsSpecialName && (mi.Name.StartsWith("set_") || mi.Name.StartsWith("get_"));
        }

        public static bool IsMethodInfoIsEvent(MethodInfo mi)
        {
            if (mi == null)
            {
                throw new ArgumentNullException("mi");
            }

            return
                mi.IsSpecialName && (mi.Name.StartsWith("add_") || mi.Name.StartsWith("remove_"));
        }
    }
}
