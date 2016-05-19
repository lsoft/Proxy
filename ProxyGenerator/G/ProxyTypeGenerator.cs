using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.CSharp;
using ProxyGenerator.PL;
using ProxyGenerator.PropertyLogic;
using ProxyGenerator.WrapMethodResolver;

namespace ProxyGenerator.G
{
    /// <summary>
    /// Генератор прокси-объектов
    /// Один должен быть создан в одном экземпляре на  каждый ТИП фабрики полезной нагрузки
    /// </summary>
    public class ProxyTypeGenerator : IProxyTypeGenerator
    {
        private readonly IProxyPayloadFactory _payloadFactory;
        private readonly ConcurrentDictionary<ProxyKey, Type> _preCompiledCache;


        /// <summary>
        /// Хранилище сгенерированных сборок, чтобы их не добавлять в референсы новых генерируемых сборок
        /// </summary>
        private static readonly AssemblyContainer _compiledAssemblyContainer;

        static ProxyTypeGenerator()
        {
            _compiledAssemblyContainer = new AssemblyContainer();
        }

        /// <summary>
        /// Создание прокси-генератора
        /// </summary>
        /// <param name="payloadFactory">Фабрика полезной нагрузки прокси-объекта</param>
        public ProxyTypeGenerator(IProxyPayloadFactory payloadFactory)
        {
            if (payloadFactory == null)
            {
                throw new ArgumentNullException("payloadFactory");
            }

            _payloadFactory = payloadFactory;
            _preCompiledCache = new ConcurrentDictionary<ProxyKey, Type>();
        }

        /// <summary>
        /// Фабрика полезной нагрузки, которая зарегистрирована в этом генераторе
        /// </summary>
        public IProxyPayloadFactory PayloadFactory
        {
            get
            {
                return
                    _payloadFactory;
            }
        }

        /// <summary>
        /// Создание прокси-объекта
        /// </summary>
        /// <typeparam name="TInterface">Интерфейс, выдаваемый наружу</typeparam>
        /// <typeparam name="TClass">Тип оборачиваемого объекта</typeparam>
        /// <param name="wrapResolver">Делегат-определитель, надо ли проксить метод</param>
        /// <param name="generatedAssemblyName">Необязательный параметр имени генерируемой сборки</param>
        /// <param name="additionalReferencedAssembliesLocation">Сборки, на которые надо дополнительно сделать референсы при компиляции прокси</param>
        /// <returns>Сформированный ТИП прокси, который после создания ЭКЗЕМПЛЯРА с помощью интерфейса прикидывается оборачиваемым объектом</returns>
        public Type CreateProxyType<TInterface, TClass>(
            WrapResolverDelegate wrapResolver,
            string generatedAssemblyName = null,
            string[] additionalReferencedAssembliesLocation = null
            )
                where TInterface : class
                where TClass : class
        {
            if (wrapResolver == null)
            {
                throw new ArgumentNullException("wrapResolver");
            }

            var tc = typeof(TClass);
            var ti = typeof(TInterface);

            #region validate

            if (!tc.IsClass)
            {
                throw new ArgumentException(
                    string.Format(
                        "{0} is not a class",
                        ProxySourceGenerator.FullNameConverter(tc.FullName)));
            }

            if (!ti.IsInterface)
            {
                throw new ArgumentException(
                    string.Format(
                        "{0} is not an interface",
                        ProxySourceGenerator.FullNameConverter(ti.FullName)));
            }

            if (!tc.GetInterfaces().Contains(ti))
            {
                throw new ArgumentException(
                    string.Format(
                        "{0} does not expose {1}",
                        ProxySourceGenerator.FullNameConverter(tc.FullName),
                        ProxySourceGenerator.FullNameConverter(ti.FullName)));
            }

            #endregion

            var key = new ProxyKey(ti, tc);

            //смотрим в кеш, вдруг уже есть
            Type proxyType;
            if (!_preCompiledCache.TryGetValue(key, out proxyType))
            {
                //в кеше не найдено, необходимо компилировать

                //компилируем, это долгий процесс (поэтому не должен быть под блокировкой)
                var type = CompileProxyType<TInterface, TClass>(
                    wrapResolver,
                    _payloadFactory,
                    generatedAssemblyName,
                    additionalReferencedAssembliesLocation
                    );

                //пытаемся сохранить в кеш
                //если в кеше уже есть версия этого прокси-типа, то метод вернет ее, так как в кеше более ранняя версия
                proxyType = TryToSaveProxyTypeToCache(key, type);
            }


            return proxyType;
        }

        #region private

        /// <summary>
        /// Пытается сохранить тип прокси в кеш
        /// Если в кеше уже есть такой ключ, то он возвращает тип прокси ИЗ КЕША (так как он первым скомпилировался)
        /// </summary>
        private Type TryToSaveProxyTypeToCache(ProxyKey key, Type proxyType)
        {
            Type result;

            if (_preCompiledCache.TryAdd(key, proxyType))
            {
                result = proxyType;
            }
            else
            {
                result = _preCompiledCache[key];
            }

            return result;
        }

        /// <summary>
        /// Попробовать достать прокси из кеша
        /// </summary>
        private Type GetProxyTypeFromCache(ProxyKey key)
        {
            Type result = null;

            _preCompiledCache.TryGetValue(key, out result);

            return result;
        }

        #endregion

        #region private static

        private static Type CompileProxyType<TInterface, TClass>(
            WrapResolverDelegate wrapResolver,
            IProxyPayloadFactory factory,
            string generatedAssemblyName,
            string[] additionalReferencedAssembliesLocation)
        {
            if (wrapResolver == null)
            {
                throw new ArgumentNullException("wrapResolver");
            }
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }
            //generatedAssemblyName allowed to be null
            //additionalReferencedAssembliesLocation allowed to be null

            var tc = typeof(TClass);
            var ti = typeof(TInterface);

            var sg = new ProxySourceGenerator(DateTime.Now);

            #region constructor

            var constructorList = string.Empty;

            var tccList = tc.GetConstructors();
            foreach (var tcc in tccList)
            {
                var piList = tcc.GetParameters();

                var argTypeAndNameString = sg.GetArgumentTypeAndNameList(
                    "{_IProxyPayloadFactory_} factory".Replace("{_IProxyPayloadFactory_}", typeof(IProxyPayloadFactory).FullName),
                    piList);
                var argNameString = sg.GetArgumentNameList(piList);

                var constructorClassName = sg.GetConstructorProxyClassName(tc);
                var proxyConstructor0 = ProxyConstructor.Replace(
                    "{_ProxyClassName_}",
                    constructorClassName);

                var fullName = sg.GetClassName(tc);
                var proxyConstructor1 = proxyConstructor0.Replace(
                    "{_ClassName_}",
                    fullName);

                var proxyConstructor2 = proxyConstructor1.Replace(
                    "{_ArgTypeAndNameList_}",
                    argTypeAndNameString);

                var proxyConstructor3 = proxyConstructor2.Replace(
                    "{_ArgNameList_}",
                    argNameString);

                constructorList += proxyConstructor3 + "\r\n\r\n";
            }

            #endregion

            #region properties

            //получаем все проперти
            var tpList = new List<MethodInfo>();
            ExtractAllPropertiesFromInterfaceHierarchy(
                ti,
                tpList
                );

            //группируем проперти
            var pdList = tpList.ConvertAll(j => new PropertyDefinition(j));

            var tppDict = new Dictionary<string, PairProperty>();
            foreach (var item in pdList)
            {
                PairProperty pp;
                if (tppDict.ContainsKey(item.PropertyName))
                {
                    pp = tppDict[item.PropertyName];
                }
                else
                {
                    pp = new PairProperty(sg, item.PropertyName);
                    tppDict.Add(item.PropertyName, pp);
                }

                if (item.IsGet)
                {
                    pp.GetProp = item;
                }
                else
                {
                    pp.SetProp = item;
                }
            }

            var propertiesList = string.Empty;

            foreach (var ppk in tppDict)
            {
                var pp = ppk.Value;

                var src = pp.ToSourceCode();

                propertiesList += src + "\r\n\r\n";
            }

            #endregion

            var allIntefaces = tc.GetInterfaces().ToList();

            #region public methods

            var methodList = string.Empty;

            foreach (var ii in allIntefaces)
            {
                var timList = ExtractAllMethodsFromInterface(ii);
                foreach (var tim in timList)
                {
                    var piList = tim.GetParameters();

                    var argTypeAndNameString = sg.GetArgumentTypeAndNameList(null, piList);
                    var argNameString = sg.GetArgumentNameList(piList);

                    var retType = tim.ReturnType;
                    var retTypeName =
                        retType != typeof (void)
                            ? sg.ParameterTypeConverter(retType)
                            : "void";
                    var notVoid = retType != typeof (void);

                    string preMethod;

                    if (wrapResolver(tim))
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
                        tim.Name);

                    var proxyMethod1 = proxyMethod0.Replace(
                        "{_ClassName_}",
                        sg.GetClassName(tc));

                    var proxyMethod2 = proxyMethod1.Replace(
                        "{_ArgTypeAndNameList_}",
                        argTypeAndNameString);

                    var proxyMethod3 = proxyMethod2.Replace(
                        "{_ArgNameList_}",
                        argNameString);

                    var proxyMethod4 = proxyMethod3.Replace(
                        "{_ReturnType_}",
                        ProxySourceGenerator.FullNameConverter(retTypeName));

                    var proxyMethod5 = proxyMethod4.Replace(
                        "{_ReturnClause_}",
                        notVoid ? "return" : string.Empty);

                    methodList += proxyMethod5 + "\r\n\r\n";
                }
            }

            #endregion

            var proxyClassName = sg.GetProxyClassName(tc);

            var class0 = ProtoClass
                .Replace(
                    "{_ProxyClassName_}",
                    proxyClassName)
                .Replace(
                    "{_IProxyPayloadFactory_}",
                    typeof(IProxyPayloadFactory).FullName)
                ;

            var interfaceMap = string.Join(
                ",",
                allIntefaces.ConvertAll(j => sg.GetClassName(j))
                );

            var class1 = class0.Replace(
                "{_InterfaceMap_}",
                interfaceMap
                );

            var className = sg.GetClassName(tc);

            var class2 = class1.Replace(
                "{_ClassName_}",
                className
                );

            var class3 = class2.Replace(
                "{_ConstructorList_}",
                constructorList);

            var class4 = class3.Replace(
                "{_MethodList_}",
                methodList);

            var class5 = class4.Replace(
                "{_PropertiesList_}",
                propertiesList);

            var factoryAssembly = factory.GetType().Assembly;
            var factoryAssemblyUsing = factoryAssembly.FullName.Split(',')[0];

            var class6 = class5.Replace(
                "{_PayloadFactoryAssembly_}",
                factoryAssemblyUsing);

            var sources = class6;

            #region debug output

            // если дебажный вывод, то и выводить в дебажный вывод, не в консоль
            Debug.WriteLine(ti.FullName);
            Debug.WriteLine(tc.FullName);
            Debug.WriteLine(sources);

            #endregion

            Assembly compiledAssembly;

            #region compile with .NET 4.0

            using (var compiler = new CSharpCodeProvider(new Dictionary<String, String> { { "CompilerVersion", "v4.0" } }))
            {
                var compilerParameters = new CompilerParameters();

                foreach (var usingAssembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    #region skip dynamic assemblies

                    if (usingAssembly.IsDynamic)
                    {
                        continue;
                    }

                    #endregion

                    #region skip previously generated proxy assemblies

                    if (_compiledAssemblyContainer.IsExists(usingAssembly))
                    {
                        continue;
                    }

                    #endregion

                    compilerParameters.ReferencedAssemblies.Add(usingAssembly.Location);
                }

                //добавляем сборку фактори
                compilerParameters.ReferencedAssemblies.Add(factory.GetType().Assembly.Location);

                //добавляем сборку интерфейса
                compilerParameters.ReferencedAssemblies.Add(ti.Assembly.Location);

                //добавляем сборку класса
                compilerParameters.ReferencedAssemblies.Add(tc.Assembly.Location);

                //добавляем добавочные сборки
                if (additionalReferencedAssembliesLocation != null)
                {
                    foreach (var aral in additionalReferencedAssembliesLocation)
                    {
                        compilerParameters.ReferencedAssemblies.Add(aral);
                    }
                }

                compilerParameters.GenerateInMemory = true;

                if (!string.IsNullOrEmpty(generatedAssemblyName))
                {
                    compilerParameters.OutputAssembly = generatedAssemblyName;
                }

                var compilerResults = compiler.CompileAssemblyFromSource(compilerParameters, sources);

                if (compilerResults.Errors.HasErrors)
                {
                    throw new Exception(
                        compilerResults.Errors.Cast<CompilerError>().Aggregate(
                            "", (text, error) => text + error.Line + ": " + error.ErrorText + "\r\n"));
                }

                compiledAssembly = compilerResults.CompiledAssembly;
            }

            #endregion

            _compiledAssemblyContainer.Add(compiledAssembly);

            var type = compiledAssembly.GetTypes()[0];

            return type;
        }

        private static void ExtractAllPropertiesFromInterfaceHierarchy(
            Type ti,
            List<MethodInfo> result 
            )
        {
            result.AddRange(
                ti
                    .GetMethods()
                    .Where(mi => PropertyDefinition.IsMethodInfoProperty(mi))
                );

            var baseInterfaceList = ti.GetInterfaces();
            if (baseInterfaceList != null && baseInterfaceList.Length > 0)
            {
                foreach (var bi in baseInterfaceList)
                {
                    ExtractAllPropertiesFromInterfaceHierarchy(
                        bi,
                        result
                        );
                }
            }
        }

        private static IEnumerable<MethodInfo> ExtractAllMethodsFromInterface(Type ti)
        {
            var timList = ti
                .GetMethods()
                .Where(mi => !PropertyDefinition.IsMethodInfoProperty(mi))
                ;

            return
                timList;
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

        private const string ProtoClass = @"
using System;
using System.Collections.Generic;
using System.Linq;
using {_PayloadFactoryAssembly_};

namespace ProxyGenerator.Proxies
{
    public class {_ProxyClassName_} : {_InterfaceMap_}
    {
        private readonly {_ClassName_} _wrappedObject;
        private readonly {_IProxyPayloadFactory_} _factory;

        {_ConstructorList_}

        #region Implementation of interfaces
   
        {_PropertiesList_}

        {_MethodList_}

        #endregion
    }
}
";

        #endregion
    }
}
