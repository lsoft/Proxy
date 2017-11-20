# Proxy, also known as YetAnotherProxy :) 

[![Build status](https://ci.appveyor.com/api/projects/status/kjo2pj1k59fsxs8l?svg=true)](https://ci.appveyor.com/project/lsoft/proxy)

Without Ninject
[![NuGet Version](https://img.shields.io/nuget/v/YetAnotherProxy.svg)](https://www.nuget.org/packages/YetAnotherProxy/)
[![NuGet Version](https://img.shields.io/nuget/dt/YetAnotherProxy.svg)](https://www.nuget.org/packages/YetAnotherProxy/)

As Ninject extension
[![NuGet Version](https://img.shields.io/nuget/v/Ninject.Extensions.YetAnotherProxy.svg)](https://www.nuget.org/packages/Ninject.Extensions.YetAnotherProxy/)
[![NuGet Version](https://img.shields.io/nuget/dt/Ninject.Extensions.YetAnotherProxy.svg)](https://www.nuget.org/packages/Ninject.Extensions.YetAnotherProxy/)

Runtime code generation proxy. Useful telemetry for your C# application.
It easily binds with [Ninject](https://github.com/ninject), but not require it, and is able to store telemetry to SQL Server, binary files or others containers.

## Proxy

```C#
public interface IInterface1ThatNeedToBeProxied
{
    [ProxyAttribute]
    SomeObject DoSomething(SomeOtherObject argument);
}

public class Class1ThatNeedToBeProxied : IInterface1ThatNeedToBeProxied
{
    public SomeObject DoSomething(SomeOtherObject argument)
    {
        //useful code
    }
}
```

You can create a proxy manually:
```C#
var proxiedObject = constructor.CreateProxy<IInterface1ThatNeedToBeProxied, Class1ThatNeedToBeProxied>(
    payloadFactory,
    typeof (ProxyAttribute)
    );
```
or through Ninject:
```C#
kernel
    .Bind<IInterface1ThatNeedToBeProxied>()
    .ToProxy<IInterface1ThatNeedToBeProxied, Class1ThatNeedToBeProxied, ProxyAttribute>()
;

var proxiedObject = kernel.Get<IInterface1ThatNeedToBeProxied>();
```


and use it instead of Class1ThatNeedToBeProxied. For example usage (in Ninject scope, or not) please refer to 2 demo projects.

## Payload

Default payload stores a class name, method name, start time, execution time span, and execution exception if raised. Also default payload combines profiling events in stack order for each execution thread.
Default payload can be replaced by the user's one.
