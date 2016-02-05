using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ProxyGenerator.WrapMethodResolver
{
    public class AttributeWrapMethodResolver
    {
        public static bool NeedToWrap(
            Type attributeType,
            MethodInfo mi
            )
            
        {
            if (attributeType == null)
            {
                throw new ArgumentNullException("attributeType");
            }
            if (mi == null)
            {
                throw new ArgumentNullException("mi");
            }

            var result = Do(
                mi,
                attributeType
                );

            return
                result;
        }
        
        public static bool NeedToWrap<T>(
            MethodInfo mi
            )
            where T : Attribute
        {
            if (mi == null)
            {
                throw new ArgumentNullException("mi");
            }

            var attributeType = typeof (T);

            var result = Do(
                mi,
                attributeType
                );

            return
                result;
        }

        private static bool Do(
            MethodInfo mi,
            Type attributeType
            )
        {
            var result = false;

            var cal = mi.GetCustomAttributes(attributeType, true);
            if (cal != null && cal.Length > 0)
            {
                //метод надо оборачивать во враппер
                result = true;
            }
            else
            {
                //метод не надо оборачивать во враппер
            }

            return
                result;
        }

    }
}
