using System;
using System.Reflection;

namespace ProxyGenerator.PropertyLogic
{
    internal class PropertyDefinition
    {
        public MethodInfo Method
        {
            get;
            private set;
        }

        public string PropertyName
        {
            get;
            private set;
        }

        public bool IsGet
        {
            get;
            private set;
        }

        public PropertyDefinition(
            MethodInfo method)
        {
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }
            if (!IsMethodInfoProperty(method))
            {
                throw new ArgumentException("!IsMethodInfoProperty(method)");
            }

            Method = method;
            PropertyName = ExtractPropertyName(method);
            IsGet = ExtractIsGet(method);
        }

        private bool ExtractIsGet(MethodInfo mi)
        {
            if (mi == null)
            {
                throw new ArgumentNullException("mi");
            }

            return
                mi.Name.Substring(0, 4) == "get_";
        }

        private string ExtractPropertyName(MethodInfo mi)
        {
            if (mi == null)
            {
                throw new ArgumentNullException("mi");
            }
            if (!IsMethodInfoProperty(mi))
            {
                throw new ArgumentException("!IsMethodInfoProperty(mi)");
            }


            return
                mi.Name.Substring(4);
        }

        public static bool IsMethodInfoProperty(MethodInfo mi)
        {
            if (mi == null)
            {
                throw new ArgumentNullException("mi");
            }

            return
                mi.IsSpecialName && (mi.Name.StartsWith("set_") || mi.Name.StartsWith("get_"));
        }


    }
}