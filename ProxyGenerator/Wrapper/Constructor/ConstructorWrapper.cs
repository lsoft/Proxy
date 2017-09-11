using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ProxyGenerator.Generator;
using ProxyGenerator.Payload;

namespace ProxyGenerator.Wrapper.Constructor
{
    internal class ConstructorWrapper : IWrapper
    {
        private readonly Type _targetClassType;
        private readonly ConstructorInfo _constructor;

        public ConstructorWrapper(
            Type targetClassType,
            ConstructorInfo constructor
            )
        {
            if (targetClassType == null)
            {
                throw new ArgumentNullException("targetClassType");
            }
            if (constructor == null)
            {
                throw new ArgumentNullException("constructor");
            }
            _targetClassType = targetClassType;
            _constructor = constructor;
        }

        public string ToSourceCode()
        {
            var piList = _constructor.GetParameters();

            var argTypeAndNameString = SourceHelper.GetArgumentTypeAndNameList(
                "{_IProxyPayloadFactory_} factory".Replace("{_IProxyPayloadFactory_}", typeof(IProxyPayloadFactory).FullName),
                piList
                );
            
            var argNameString = SourceHelper.GetArgumentNameList(piList);

            var constructorClassName = SourceHelper.GetConstructorProxyClassName(_targetClassType);
            
            var proxyConstructor0 = ProxyConstructor.Replace(
                "{_ProxyClassName_}",
                constructorClassName
                );

            var fullName = SourceHelper.GetClassName(_targetClassType);
            var proxyConstructor1 = proxyConstructor0.Replace(
                "{_ClassName_}",
                fullName
                );

            var proxyConstructor2 = proxyConstructor1.Replace(
                "{_ArgTypeAndNameList_}",
                argTypeAndNameString
                );

            var proxyConstructor3 = proxyConstructor2.Replace(
                "{_ArgNameList_}",
                argNameString
                );

            return
                proxyConstructor3;
        }

        private const string ProxyConstructor = @"
        public {_ProxyClassName_}({_ArgTypeAndNameList_})
        {
            if(factory == null)
            {
                throw new ArgumentNullException(""factory"");
            }

            _wrappedObject = new {_ClassName_}({_ArgNameList_});
            _factory = factory;
        }
";
    }
}
