using System;
using System.Linq;
using System.Reflection;

namespace ProxyGenerator.Generator
{
    internal static class SourceHelper
    {
        private static readonly string _suffix = "_" + Guid.NewGuid().ToString().Replace("-", string.Empty);


        //---------------------------- public  -----------------------------


        public static string GetProxyClassName(Type classType)
        {
            string result;

            if (classType.IsGenericType)
            {
                //var p = string.Join(
                //    ",",
                //    classType
                //        //.GetGenericTypeDefinition()
                //        .GetGenericArguments()
                //        .ToList()
                //        .ConvertAll(j => ParameterTypeStringConverter(j.ToString()))
                //        .ToArray());

                var ppp = classType.Name;
                ppp = ppp.Substring(0, ppp.IndexOf('`'));
                ppp = ParameterTypeStringConverter(ppp);

                result = string.Format(
                    "{0}{1}Proxy",
                    ppp,
                    _suffix
                    );
            }
            else
            {
                result = string.Format(
                    "{0}{1}Proxy",
                    classType.Name,
                    _suffix
                    );
            }

            return result;

        }

        public static string GetConstructorProxyClassName(Type classType)
        {
            string result;

            if (classType.IsGenericType)
            {
                var ppp = FullNameConverter(classType.Name);
                ppp = ppp.Substring(0, ppp.IndexOf('`'));
                ppp = ParameterTypeStringConverter(ppp);

                result = string.Format(
                    "{0}{1}Proxy",
                    ppp,
                    _suffix
                    );
            }
            else
            {
                result = string.Format(
                    "{0}{1}Proxy",
                    classType.Name,
                    _suffix
                    );
            }

            return result;

        }


        public static string GetClassName(Type classType)
        {
            string result;

            if (classType.IsGenericType)
            {
                var p = string.Join(
                    ",",
                    classType
                        //.GetGenericTypeDefinition()
                        .GetGenericArguments()
                        .ToList()
                        .ConvertAll(j => GetClassName(j))
                        .ToArray());

                var ppp = FullNameConverter(classType.FullName);
                ppp = ppp.Substring(0, ppp.IndexOf('`'));
                ppp = ParameterTypeStringConverter(ppp);

                result = string.Format(
                    "{0}<{1}>",
                    ppp,
                    p);
            }
            else
            {
                result = FullNameConverter(classType.FullName);
            }

            return result;

        }


        public static string GetArgumentNameList(ParameterInfo[] parameters)
        {
            string result;

            result = string.Join(
                ", ",
                parameters
                    .ToList()
                    .ConvertAll(
                        j => string.Format(
                                "{0} {1}",
                                ParameterModifierConverter(j),
                                j.Name))
                    .ToArray());

            return result;
        }


        public static string GetArgumentTypeAndNameList(
            string fixedParameter,
            ParameterInfo[] parameters
            )
        {
            var pList = parameters
                .ToList()
                .ConvertAll(
                    j => string.Format(
                        "{0} {1} {2}",
                        ParameterModifierConverter(j),
                        ParameterTypeConverter(j.ParameterType),
                        j.Name));

            if (!string.IsNullOrEmpty(fixedParameter))
            {
                pList.Insert(0, fixedParameter);
            }

            var result = string.Join(", ", pList.ToArray());

            return result;
        }

        public static string ParameterTypeConverter(Type parameterType)
        {
            string result;

            if (parameterType.IsGenericType || (parameterType.IsByRef && parameterType.GetElementType().IsGenericType))
            {
                var p = string.Join(
                    ",",
                    parameterType
                        .GetGenericArguments()
                        .ToList()
                        .ConvertAll(j => ParameterTypeConverter(j))
                        .ToArray());

                var ppp = ParameterTypeStringConverter(parameterType.ToString());
                ppp = ppp.Substring(0, ppp.IndexOf('`'));

                result = string.Format("{0}<{1}>", ppp, p);
            }
            else
            {
                result = ParameterTypeStringConverter(parameterType.ToString());
            }

            return result;
        }


        public static string FullNameConverter(string fullname)
        {
            //вложенные классы в рефлексии отображаются через знак плюс
            var fn0 = fullname.Replace(
                "+",
                "."
                );

            return fn0;
        }






        //---------------------------- private  -----------------------------









        private static string ParameterModifierConverter(ParameterInfo pi)
        {
            var result = string.Empty;

            if (pi.ParameterType.IsByRef && !pi.IsOut)
            {
                result = "ref";
            }
            else
                if (pi.ParameterType.IsByRef && pi.IsOut)
                {
                    result = "out";
                }

            return result;
        }

        private static string ParameterTypeStringConverter(string pt)
        {
            var pt0 = pt.Replace(
                "&",
                string.Empty);

            var pt1 = pt0.Replace(
                "+",
                ".");

            return pt1;
        }

    }
}
