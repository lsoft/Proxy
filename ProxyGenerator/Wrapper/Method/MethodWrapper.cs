using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ProxyGenerator.Generator;
using ProxyGenerator.WrapMethodResolver;

namespace ProxyGenerator.Wrapper.Method
{
    internal class MethodWrapper : IWrapper
    {
        private readonly Type _targetClassType;
        private readonly WrapResolverDelegate _wrapResolver;
        private readonly MethodInfo _method;

        public MethodWrapper(
            Type targetClassType,
            WrapResolverDelegate wrapResolver,
            MethodInfo method
            )
        {
            if (targetClassType == null)
            {
                throw new ArgumentNullException("targetClassType");
            }
            if (wrapResolver == null)
            {
                throw new ArgumentNullException("wrapResolver");
            }
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }

            _targetClassType = targetClassType;
            _wrapResolver = wrapResolver;
            _method = method;
        }

        public string ToSourceCode()
        {
            var piList = _method.GetParameters();

            var argTypeAndNameString = SourceHelper.GetArgumentTypeAndNameList(null, piList);
            var argNameString = SourceHelper.GetArgumentNameList(piList);

            var retType = _method.ReturnType;
            var retTypeName =
                retType != typeof(void)
                    ? SourceHelper.ParameterTypeConverter(retType)
                    : "void";
            var notVoid = retType != typeof(void);

            string preMethod;

            if (_wrapResolver(_method))
            {
                //метод надо оборачивать во враппер
                preMethod = ProxyMethod;
            }
            else
            {
                //метод не надо оборачивать во враппер
                preMethod = NonProxyMethod;
            }

            var proxyMethod0 = preMethod.Replace(
                "{_MethodName_}",
                _method.Name
                );

            var proxyMethod1 = proxyMethod0.Replace(
                "{_ClassName_}",
                SourceHelper.GetClassName(_targetClassType)
                );

            var proxyMethod2 = proxyMethod1.Replace(
                "{_ArgTypeAndNameList_}",
                argTypeAndNameString
                );

            var proxyMethod3 = proxyMethod2.Replace(
                "{_ArgNameList_}",
                argNameString
                );

            var proxyMethod4 = proxyMethod3.Replace(
                "{_ReturnType_}",
                SourceHelper.FullNameConverter(retTypeName)
                );

            var proxyMethod5 = proxyMethod4.Replace(
                "{_ReturnClause_}",
                notVoid ? "return" : string.Empty
                );

            return
                proxyMethod5;
        }

        private const string NonProxyMethod = @"
        public {_ReturnType_} {_MethodName_}({_ArgTypeAndNameList_})
        {
            //не надо телеметрии из этого метода
            {_ReturnClause_} _wrappedObject.{_MethodName_}({_ArgNameList_});
        }
";

        private const string ProxyMethod = @"
        public {_ReturnType_} {_MethodName_}({_ArgTypeAndNameList_})
        {
            //метод с телеметрией

            var pte = _factory.GetProxyPayload(""{_ClassName_}"", ""{_MethodName_}"");

            try
            {
                {_ReturnClause_} _wrappedObject.{_MethodName_}({_ArgNameList_});
            }
            catch (Exception excp)
            {
                pte.SetExceptionFlag(excp);
                throw;
            }
            finally
            {
                pte.Dispose();
            }
        }
";

    }
}
