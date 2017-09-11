using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CSharp;
using ProxyGenerator.Payload;
using ProxyGenerator.Wrapper.Constructor;
using ProxyGenerator.Wrapper.Event;
using ProxyGenerator.Wrapper.Method;
using ProxyGenerator.Wrapper.Property;

namespace ProxyGenerator.Generator
{
    /// <summary>
    /// Генератор прокси-объектов
    /// Один должен быть создан в одном экземпляре на  каждый ТИП фабрики полезной нагрузки
    /// </summary>
    public class ProxyTypeGenerator : IProxyTypeGenerator
    {
        private readonly ConcurrentDictionary<ProxyKey, Type> _preCompiledCache = new ConcurrentDictionary<ProxyKey, Type>();

        /// <summary>
        /// Хранилище сгенерированных сборок, чтобы их не добавлять в референсы новых генерируемых сборок
        /// </summary>
        private readonly AssemblyContainer _compiledAssemblyContainer = new AssemblyContainer();

        /// <summary>
        /// Создание прокси-генератора
        /// </summary>
        public ProxyTypeGenerator(
            )
        {
        }

        /// <summary>
        /// Создание прокси-объекта
        /// </summary>
        /// <typeparam name="TInterface">Интерфейс, выдаваемый наружу</typeparam>
        /// <typeparam name="TClass">Тип оборачиваемого объекта</typeparam>
        /// <param name="p">Настройки генерации</param>
        /// <returns>Сформированный ТИП прокси, который после создания ЭКЗЕМПЛЯРА с помощью интерфейса прикидывается оборачиваемым объектом</returns>
        public Type CreateProxyType<TInterface, TClass>(
            Parameters p
            )
                where TInterface : class
                where TClass : class
        {
            if (p == null)
            {
                throw new ArgumentNullException("p");
            }

            var tc = typeof(TClass);
            var ti = typeof(TInterface);

            #region validate

            if (!tc.IsClass)
            {
                throw new ArgumentException(
                    string.Format(
                        "{0} is not a class",
                        SourceHelper.FullNameConverter(tc.FullName)));
            }

            if (!ti.IsInterface)
            {
                throw new ArgumentException(
                    string.Format(
                        "{0} is not an interface",
                        SourceHelper.FullNameConverter(ti.FullName)));
            }

            if (!tc.GetInterfaces().Contains(ti))
            {
                throw new ArgumentException(
                    string.Format(
                        "{0} does not expose {1}",
                        SourceHelper.FullNameConverter(tc.FullName),
                        SourceHelper.FullNameConverter(ti.FullName)));
            }

            #endregion

            var key = new ProxyKey(ti, tc);

            //смотрим в кеш, вдруг уже есть
            Type proxyType;
            if (!_preCompiledCache.TryGetValue(key, out proxyType))
            {
                //в кеше не найдено, необходимо компилировать

                //компилируем, это долгий процесс
                var type = CompileProxyType<TInterface, TClass>(
                    p
                    );

                //пытаемся сохранить в кеш
                //если в кеше уже есть версия этого прокси-типа, то метод вернет ее, так как в кеше более ранняя версия
                //получится, что мы зря компилили, но это не страшно дял логики работы
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

        private Type CompileProxyType<TInterface, TClass>(
            Parameters p
            )
        {
            if (p == null)
            {
                throw new ArgumentNullException("p");
            }

            var tc = typeof(TClass);
            var ti = typeof(TInterface);

            #region constructor

            var constructors = new StringBuilder();

            var tccList = tc.GetConstructors().ToList();

            var constructorWrapperList = tccList.ConvertAll(
                j => new ConstructorWrapper(
                    tc,
                    j
                    )
                );

            foreach (var tcc in constructorWrapperList)
            {
                var constructorSource = tcc.ToSourceCode();

                constructors.AppendLine(constructorSource);
                constructors.AppendLine();
            }

            #endregion

            #region events

            var events = new StringBuilder();

            var eList = new List<EventInfo>();
            ExtractAllEventsFromInterfaceHierarchy(
                ti,
                eList
                );

            var eventWrapperList = eList.ConvertAll(
                j => new EventWrapper(
                    tc,
                    j
                    )
                );

            foreach (var e in eventWrapperList)
            {
                var eventSource = e.ToSourceCode();

                events.AppendLine(eventSource);
                events.AppendLine();
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
            var pdList = tpList.ConvertAll(j => new PropertyElement(j));

            var tppDict = new Dictionary<string, PropertyWrapper>();
            foreach (var item in pdList)
            {
                PropertyWrapper pp;
                if (tppDict.ContainsKey(item.PropertyName))
                {
                    pp = tppDict[item.PropertyName];
                }
                else
                {
                    pp = new PropertyWrapper(item.PropertyName);
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

            var properties = new StringBuilder();

            foreach (var ppk in tppDict)
            {
                var pp = ppk.Value;

                var src = pp.ToSourceCode();

                properties.AppendLine(src);
                properties.AppendLine();
            }

            #endregion

            var allIntefaces = tc.GetInterfaces().ToList();

            #region public methods

            var methods = new StringBuilder();

            foreach (var ii in allIntefaces)
            {
                var timList = new List<MethodInfo>();
                
                ExtractAllMethodsFromInterface(
                    ii,
                    timList
                    );

                var methodWrapperList = timList.ConvertAll(
                    j => new MethodWrapper(
                        tc,
                        p.WrapResolver,
                        j
                        )
                    );

                foreach (var mw in methodWrapperList)
                {
                    var methodSourceCode = mw.ToSourceCode();

                    methods.AppendLine(methodSourceCode);
                    methods.AppendLine();
                }
            }

            #endregion

            #region combine all parts into complete source

            var proxyClassName = SourceHelper.GetProxyClassName(tc);

            var class0 = ProtoClass
                .Replace(
                    "{_ProxyClassName_}",
                    proxyClassName
                    )
                .Replace(
                    "{_IProxyPayloadFactory_}",
                    typeof(IProxyPayloadFactory).FullName
                    )
                ;

            var interfaceMap = string.Join(
                ",",
                allIntefaces.ConvertAll(j => SourceHelper.GetClassName(j))
                );

            var class1 = class0.Replace(
                "{_InterfaceMap_}",
                interfaceMap
                );

            var className = SourceHelper.GetClassName(tc);

            var class2 = class1.Replace(
                "{_ClassName_}",
                className
                );

            var class3 = class2.Replace(
                "{_EventList_}",
                events.ToString()
                );

            var class4 = class3.Replace(
                "{_ConstructorList_}",
                constructors.ToString()
                );

            var class5 = class4.Replace(
                "{_MethodList_}",
                methods.ToString()
                );

            var class6 = class5.Replace(
                "{_PropertiesList_}",
                properties.ToString()
                );

            var factoryAssembly = p.ProxyPayloadFactory.GetType().Assembly;
            var factoryAssemblyUsing = factoryAssembly.FullName.Split(',')[0];

            var class7 = class6.Replace(
                "{_PayloadFactoryAssembly_}",
                factoryAssemblyUsing
                );

            var sources = class7;

            #endregion

            #region debug output

            // если дебажный вывод, то и выводить в дебажный вывод, не в консоль
            Debug.WriteLine(ti.FullName);
            Debug.WriteLine(tc.FullName);
            Debug.WriteLine(sources);

            #endregion

            Assembly compiledAssembly;

            #region compile with .NET 4.0 into compiledAssembly

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
                compilerParameters.ReferencedAssemblies.Add(p.ProxyPayloadFactory.GetType().Assembly.Location);

                //добавляем сборку интерфейса
                compilerParameters.ReferencedAssemblies.Add(ti.Assembly.Location);

                //добавляем сборку класса
                compilerParameters.ReferencedAssemblies.Add(tc.Assembly.Location);

                //добавляем добавочные сборки
                if (p.AdditionalReferencedAssembliesLocation != null)
                {
                    foreach (var aral in p.AdditionalReferencedAssembliesLocation)
                    {
                        compilerParameters.ReferencedAssemblies.Add(aral);
                    }
                }

                compilerParameters.GenerateInMemory = true;

                if (!string.IsNullOrEmpty(p.GeneratedAssemblyName))
                {
                    compilerParameters.OutputAssembly = p.GeneratedAssemblyName;
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

        private static void ExtractAllEventsFromInterfaceHierarchy(
            Type ti,
            List<EventInfo> result
            )
        {
            result.AddRange(
                ti
                    .GetEvents()
                );

            var baseInterfaceList = ti.GetInterfaces();
            if (baseInterfaceList != null && baseInterfaceList.Length > 0)
            {
                foreach (var bi in baseInterfaceList)
                {
                    ExtractAllEventsFromInterfaceHierarchy(
                        bi,
                        result
                        );
                }
            }
        }

        private static void ExtractAllPropertiesFromInterfaceHierarchy(
            Type ti,
            List<MethodInfo> result 
            )
        {
            result.AddRange(
                ti
                    .GetMethods()
                    .Where(mi => ReflectionHelper.IsMethodInfoIsProperty(mi))
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

        private static void ExtractAllMethodsFromInterface(
            Type ti,
            List<MethodInfo> result
            )
        {
            result.AddRange(
                ti
                    .GetMethods()
                    .Where(mi => !ReflectionHelper.IsMethodInfoIsProperty(mi) && !ReflectionHelper.IsMethodInfoIsEvent(mi))
                    .ToList()
                );
        }

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

        {_EventList_}

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
