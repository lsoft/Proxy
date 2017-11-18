# Proxy
Runtime code generation proxy. Useful telemetry for your C# application.
It easily binds with [Ninject](https://github.com/ninject), but not require it, and is able to store telemetry to SQL Server, binary files or others containers.

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


and use it instead of Class1ThatNeedToBeProxied.
