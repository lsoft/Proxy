using System.Collections.Generic;

namespace PerformanceTelemetry.Tests.DB.InstanceProvider
{
    public interface ISqlServerInstanceProvider
    {
        List<SqlServerInstanceInformation> EnumerateSqlInstances();
    }
}