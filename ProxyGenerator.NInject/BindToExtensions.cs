using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Ninject;
using Ninject.Activation;
using Ninject.Extensions.Factory;
using Ninject.Syntax;
using ProxyGenerator;
using ProxyGenerator.C;
using ProxyGenerator.G;
using ProxyGenerator.WrapMethodResolver;

namespace ProxyGenerator.NInject
{
    /// <summary>
    /// Extension methods for <see cref="IBindingToSyntax{T1}"/>
    /// </summary>
    public static class BindToExtensions
    {

        /// <summary>
        /// Defines that the interface shall be bound to an automatically created factory proxy.
        /// </summary>
        /// <typeparam name="TInterface">The type of the interface.</typeparam>
        /// <typeparam name="TClass">The type of the target class</typeparam>
        /// <param name="syntax">The syntax.</param>
        /// <param name="wrapResolver">Делегат-определитель, надо ли проксить метод</param>
        /// <param name="proxyPayloadFactoryProvider">Поставщик фабрики объектов полезной нагрузки</param>
        /// <param name="generatedAssemblyName">Необязательный параметр имени генерируемой сборки</param>
        /// <param name="additionalReferencedAssembliesLocation">Сборки, на которые надо дополнительно сделать референсы при компиляции прокси</param>
        /// <returns>The <see cref="IBindingWhenInNamedWithOrOnSyntax{TInterface}"/> to configure more things for the binding.</returns>
        public static IBindingWhenInNamedWithOrOnSyntax<TInterface> ToProxy<TInterface, TClass>(
            this IBindingToSyntax<TInterface> syntax,
            WrapResolverDelegate wrapResolver,
            IProxyPayloadFactoryProvider proxyPayloadFactoryProvider = null,
            string generatedAssemblyName = null,
            string[] additionalReferencedAssembliesLocation = null
            )
            where TInterface : class
            where TClass : class
        {
            if (syntax == null)
            {
                throw new ArgumentNullException("syntax");
            }
            if (wrapResolver == null)
            {
                throw new ArgumentNullException("wrapResolver");
            }

            if (proxyPayloadFactoryProvider == null)
            {
                proxyPayloadFactoryProvider = new NamedProxyPayloadFactoryProvider();
            }

            var proxyPayloadFactory = proxyPayloadFactoryProvider.GetProxyPayloadFactory(syntax.Kernel);

            var p = new Parameters(
                proxyPayloadFactory,
                wrapResolver,
                generatedAssemblyName,
                additionalReferencedAssembliesLocation
                );

            var result = Do<TInterface, TClass>(
                syntax,
                p
                );

            return
                result;
        }

        /// <summary>
        /// Defines that the interface shall be bound to an automatically created factory proxy.
        /// </summary>
        /// <typeparam name="TInterface">The type of the interface.</typeparam>
        /// <typeparam name="TClass">The type of the target class</typeparam>
        /// <typeparam name="TAttribute">The type of method mark attribute</typeparam>
        /// <param name="syntax">The syntax.</param>
        /// <param name="proxyPayloadFactoryProvider">Поставщик фабрики объектов полезной нагрузки</param>
        /// <param name="generatedAssemblyName">Необязательный параметр имени генерируемой сборки</param>
        /// <param name="additionalReferencedAssembliesLocation">Сборки, на которые надо дополнительно сделать референсы при компиляции прокси</param>
        /// <returns>The <see cref="IBindingWhenInNamedWithOrOnSyntax{TInterface}"/> to configure more things for the binding.</returns>
        public static IBindingWhenInNamedWithOrOnSyntax<TInterface> ToProxy<TInterface, TClass, TAttribute>(
            this IBindingToSyntax<TInterface> syntax,
            IProxyPayloadFactoryProvider proxyPayloadFactoryProvider = null,
            string generatedAssemblyName = null,
            string[] additionalReferencedAssembliesLocation = null
            )
            where TInterface : class
            where TClass : class
            where TAttribute : Attribute

        {
            if (proxyPayloadFactoryProvider == null)
            {
                proxyPayloadFactoryProvider = new NamedProxyPayloadFactoryProvider();
            }

            var proxyPayloadFactory = proxyPayloadFactoryProvider.GetProxyPayloadFactory(syntax.Kernel);

            var p = new Parameters(
                proxyPayloadFactory,
                AttributeWrapMethodResolver.NeedToWrap<TAttribute>,
                generatedAssemblyName,
                additionalReferencedAssembliesLocation
                );

            var result = Do<TInterface, TClass>(
                syntax,
                p
                );

            return
                result;
        }

        private static IBindingWhenInNamedWithOrOnSyntax<TInterface> Do<TInterface, TClass>(
            IBindingToSyntax<TInterface> syntax,
            Parameters p
            )
            where TInterface : class
            where TClass : class
        {
            if (p == null)
            {
                throw new ArgumentNullException("p");
            }

            var generator = syntax.Kernel.Get<IProxyTypeGenerator>();

            var proxy = generator.CreateProxyType<TInterface, TClass>(
                p
                );

            var result = syntax.To(proxy);

            result
                .WithConstructorArgument(
                    "factory",
                    p.ProxyPayloadFactory
                    );

            var instanceProviderFunc = new Func<IContext, IInstanceProvider>((c) => c.Kernel.Get<IInstanceProvider>());

            var bindingConfiguration = syntax.BindingConfiguration; // Do not pass syntax to the lambda!!! We do not want the lambda referencing the syntax!!!

            syntax.Kernel
                .Bind<IInstanceProvider>()
                .ToMethod(instanceProviderFunc)
                .When(
                    r => r.ParentRequest != null
                         && r.ParentRequest.ParentContext.Binding.BindingConfiguration == bindingConfiguration
                )
                .InScope(ctx => bindingConfiguration);

            return
                result;
        }

    }
}
