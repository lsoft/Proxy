old way compile and test:
"C:\Program Files (x86)\Microsoft Visual Studio\Preview\Community\MSBuild\15.0\Bin\MSBuild.exe" "C:\projects\proxy.2017\Proxy.sln"  /property:Configuration=Release "/property:Platform=Any CPU"  /verbosity:minimal
"C:\Program Files (x86)\Microsoft Visual Studio\Preview\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" "C:\projects\Proxy.2017\PerformanceTelemetry.Tests\bin\Release\net4.7\PerformanceTelemetry.Tests.dll" "C:\projects\Proxy.2017\ProxyGenerator.Tests\bin\Release\net4.7\ProxyGenerator.Tests.dll"

new way compile, run and test:
dotnet build Proxy.sln
dotnet run ProxyNinjectDemonstration.csproj  --framework netcoreapp2.0
dotnet test ProxyGenerator.Tests.csproj --framework netcoreapp2.0
dotnet test ProxyGenerator.Tests.csproj --framework net4.7 --filter "FullyQualifiedName~ValueTuple"

nuget packing:
.nuget/nuget pack Ninject.Extensions.YetAnotherProxy.nuspec
.nuget/nuget pack YetAnotherProxy.nuspec

add tag:
git push origin tagName
git tag -a tagName -m "tagDescription" 2b30ad_xxx

delete tag:
git push --delete origin tagName 
git tag -d tagName 

