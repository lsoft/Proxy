using System;
using System.Reflection;

namespace ProxyGenerator.Wrapper.Property
{
    internal class PropertyElement
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

        public PropertyElement(
            MethodInfo method)
        {
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }
            if (!ReflectionHelper.IsMethodInfoIsProperty(method))
            {
                throw new ArgumentException("!IsMethodInfoIsProperty(method)");
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
            if (!ReflectionHelper.IsMethodInfoIsProperty(mi))
            {
                throw new ArgumentException("!IsMethodInfoIsProperty(mi)");
            }


            return
                mi.Name.Substring(4);
        }


    }
}