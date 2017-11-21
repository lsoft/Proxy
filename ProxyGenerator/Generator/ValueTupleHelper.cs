using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxyGenerator.Generator
{
#if VALUETUPLE_NATIVESYNTAX
    internal static class ValueTupleHelper
    {
        public static List<Type> ParseTupleArguments(
            Type type
            )
        {
            var args = new List<Type>();

            ParseTupleArguments(ref args, type);

            return
                args;
        }

        public static void ParseTupleArguments(
            ref List<Type> args,
            Type type
            )
        {
            var tupleTypes = type.GetGenericArguments();

            foreach (var tt in tupleTypes)
            {
                if (!ValueTupleHelper.IsValueTuple(tt))
                {
                    args.Add(tt);
                }
                else
                {
                    ParseTupleArguments(ref args, tt);
                }
            }
        }

        public static bool IsValueTuple(
            Type type
            )
        {
            return
                type.IsGenericType
                && _tupleTypes.Contains(type.GetGenericTypeDefinition())
                ;
        }

        private static readonly HashSet<Type> _tupleTypes = new HashSet<Type>(
            new Type[] {
                typeof(ValueTuple<>),
                typeof(ValueTuple<,>),
                typeof(ValueTuple<,,>),
                typeof(ValueTuple<,,,>),
                typeof(ValueTuple<,,,,>),
                typeof(ValueTuple<,,,,,>),
                typeof(ValueTuple<,,,,,,>),
                typeof(ValueTuple<,,,,,,,>),
            }
        );
    }
#endif
}
