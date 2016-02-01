using System;

namespace ProxyGenerator.Tests.ProxyGeneratorTests
{
    /// <summary>
    /// атрибут, которым надо помечать методы интерфейса, замеры времени которых надо проверять
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    internal class TestWrapWithProxyAttribute : Attribute
    {

    }
}
