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
        /// <param name="generatedAssemblyName">Необязательный параметр имени генерируемой сборки</param>
        /// <param name="additionalReferencedAssembliesLocation">Сборки, на которые надо дополнительно сделать референсы при компиляции прокси</param>
        /// <returns>The <see cref="IBindingWhenInNamedWithOrOnSyntax{TInterface}"/> to configure more things for the binding.</returns>
        public static IBindingWhenInNamedWithOrOnSyntax<TInterface> ToProxy<TInterface, TClass>(
            this IBindingToSyntax<TInterface> syntax,
            WrapResolverDelegate wrapResolver,
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

            var result = Do<TInterface, TClass>(
                syntax,
                wrapResolver,
                generatedAssemblyName,
                additionalReferencedAssembliesLocation
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
        /// <param name="generatedAssemblyName">Необязательный параметр имени генерируемой сборки</param>
        /// <param name="additionalReferencedAssembliesLocation">Сборки, на которые надо дополнительно сделать референсы при компиляции прокси</param>
        /// <returns>The <see cref="IBindingWhenInNamedWithOrOnSyntax{TInterface}"/> to configure more things for the binding.</returns>
        public static IBindingWhenInNamedWithOrOnSyntax<TInterface> ToProxy<TInterface, TClass, TAttribute>(
            this IBindingToSyntax<TInterface> syntax,
            string generatedAssemblyName = null,
            string[] additionalReferencedAssembliesLocation = null
            )
            where TInterface : class
            where TClass : class
            where TAttribute : Attribute

        {
            var result = Do<TInterface, TClass>(
                syntax,
                AttributeWrapMethodResolver.NeedToWrap<TAttribute>,
                generatedAssemblyName,
                additionalReferencedAssembliesLocation
                );

            return
                result;
        }

        private static IBindingWhenInNamedWithOrOnSyntax<TInterface> Do<TInterface, TClass>(
            IBindingToSyntax<TInterface> syntax,
            WrapResolverDelegate wrapResolver,
            string generatedAssemblyName = null,
            string[] additionalReferencedAssembliesLocation = null
            )
            where TInterface : class
            where TClass : class
        {
            var generator = ProxyTypeGeneratorCache.ProvideProxyTypeGenerator(syntax.Kernel);

            var proxy = generator.CreateProxyType<TInterface, TClass>(
                wrapResolver,
                generatedAssemblyName,
                additionalReferencedAssembliesLocation
                );

            var result = syntax.To(proxy);

            result
                .WithConstructorArgument(
                    "factory",
                    generator.PayloadFactory);

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
