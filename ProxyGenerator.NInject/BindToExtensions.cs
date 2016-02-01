using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Ninject;
using Ninject.Activation;
using Ninject.Extensions.Factory;
using Ninject.Syntax;
using ProxyGenerator;
using ProxyGenerator.C;
using ProxyGenerator.G;

namespace ProxyGenerator.NInject
{

    /// <summary>
    /// Extension methods for <see cref="IBindingToSyntax{T1}"/>
    /// </summary>
    public static class BindToExtensions
    {
        #region private code

        private static readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        private static readonly Dictionary<int, IProxyTypeGenerator> _generatorDict = new Dictionary<int, IProxyTypeGenerator>();

        private static IProxyTypeGenerator ProvideProxyTypeGenerator(
            IResolutionRoot root)
        {
            if (root == null)
            {
                throw new ArgumentNullException("root");
            }

            var hash = root.GetHashCode().GetHashCode();

            //initially try to obtain ProxyConstructor under read-lock (frequent operation)
            _locker.EnterReadLock();
            try
            {
                if (_generatorDict.ContainsKey(hash))
                {
                    return
                        _generatorDict[hash];
                }
            }
            finally
            {
                _locker.ExitReadLock();
            }

            //if it fails, obtain write-lock, once again try to obtain, otherwise construct ProxyConstructor (rare operation)
            _locker.EnterWriteLock();
            try
            {
                if (!_generatorDict.ContainsKey(hash))
                {
                    var proxyGeneratorProvider = root.Get<IGeneratorProvider>();

                    var generator = proxyGeneratorProvider.ProvideGenerator();

                    _generatorDict.Add(
                        hash,
                        generator);
                }

                return
                    _generatorDict[hash];
            }
            finally
            {
                _locker.ExitWriteLock();
            }
        }

        #endregion


        //#endregion

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
            where TAttribute : class

        {
            var generator = ProvideProxyTypeGenerator(syntax.Kernel);

            var proxy = generator.CreateProxyType<TInterface, TClass>(
                typeof(TAttribute),
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
            
            syntax.Kernel.Bind<IInstanceProvider>().ToMethod(instanceProviderFunc)
                .When(r => r.ParentRequest != null && r.ParentRequest.ParentContext.Binding.BindingConfiguration == bindingConfiguration)
                .InScope(ctx => bindingConfiguration);

            return result;
        }

    }
}
