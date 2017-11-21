#if WMI
using System;
using System.Collections.Generic;
using System.Management;
#endif

using System.Collections.Generic;

namespace PerformanceTelemetry.Tests.DB.InstanceProvider
{
#if WMI
    public class WmiSqlServerInstanceProvider : ISqlServerInstanceProvider
    {
        /// <summary>
        /// Enumerates all SQL Server instances on the machine.
        /// </summary>
        /// <returns></returns>
        public List<SqlServerInstanceInformation> EnumerateSqlInstances()
        {
            var result = new List<SqlServerInstanceInformation>();

            var correctNamespace = GetCorrectWmiNameSpace();
            if (!string.Equals(correctNamespace, string.Empty))
            {
                var query = string.Format("select * from SqlServiceAdvancedProperty where SQLServiceType = 1 and PropertyName = 'instanceID'");

                using (var getSqlEngine = new ManagementObjectSearcher(correctNamespace, query))
                {
                    var engines = getSqlEngine.Get();

                    if (engines.Count > 0)
                    {
                        foreach (var o in engines)
                        {
                            using (var sqlEngine = (ManagementObject)o)
                            {
                                var serviceName = sqlEngine["ServiceName"].ToString();
                                var instanceName = GetInstanceNameFromServiceName(serviceName);
                                var version = GetWmiPropertyValueForEngineService(serviceName, correctNamespace, "Version");
                                var edition = GetWmiPropertyValueForEngineService(serviceName, correctNamespace, "SKUNAME");

                                var r = new SqlServerInstanceInformation(
                                    instanceName,
                                    serviceName,
                                    version,
                                    edition
                                    );

                                result.Add(r);
                            }
                        }
                    }
                }
            }

            return
                result;
        }

        /// <summary>
        /// Method returns the correct SQL namespace to use to detect SQL Server instances.
        /// </summary>
        /// <returns>namespace to use to detect SQL Server instances</returns>
        private static string GetCorrectWmiNameSpace()
        {
            var wmiNamespaceToUse = "root\\Microsoft\\sqlserver";
            var namespaces = new List<string>();

            try
            {
                // Enumerate all WMI instances of __namespace WMI class.
                using (var nsClass = new ManagementClass(
                    new ManagementScope(wmiNamespaceToUse),
                    new ManagementPath("__namespace"),
                    null))
                {
                    foreach (var o in nsClass.GetInstances())
                    {
                        using (var ns = (ManagementObject)o)
                        {
                            namespaces.Add(ns["Name"].ToString());
                        }
                    }
                }
            }
            catch (ManagementException e)
            {
                Console.WriteLine("Exception = " + e.Message);
            }

            if (namespaces.Count > 0)
            {
                if (namespaces.Contains("ComputerManagement10")) //10 - means sql 2005/2008
                {
                    //use katmai+ namespace
                    wmiNamespaceToUse = wmiNamespaceToUse + "\\ComputerManagement10";
                }
                if (namespaces.Contains("ComputerManagement11")) //11 - means sql 2012
                {
                    wmiNamespaceToUse = wmiNamespaceToUse + "\\ComputerManagement11";
                }
                else if (namespaces.Contains("ComputerManagement"))
                {
                    //use yukon namespace
                    wmiNamespaceToUse = wmiNamespaceToUse + "\\ComputerManagement";
                }
                else
                {
                    wmiNamespaceToUse = string.Empty;
                }
            }
            else
            {
                wmiNamespaceToUse = string.Empty;
            }

            return
                wmiNamespaceToUse;
        }

        /// <summary>
        /// method extracts the instance name from the service name
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        private static string GetInstanceNameFromServiceName(string serviceName)
        {
            if (string.IsNullOrEmpty(serviceName))
            {
                return string.Empty;
            }

            if (string.Equals(serviceName, "MSSQLSERVER", StringComparison.OrdinalIgnoreCase))
            {
                return serviceName;
            }

            return
                serviceName.Substring(serviceName.IndexOf('$') + 1, serviceName.Length - serviceName.IndexOf('$') - 1);
        }

        /// <summary>
        /// Returns the WMI property value for a given property name for a particular SQL Server service Name
        /// </summary>
        /// <param name="serviceName">The service name for the SQL Server engine serivce to query for</param>
        /// <param name="wmiNamespace">The wmi namespace to connect to </param>
        /// <param name="propertyName">The property name whose value is required</param>
        /// <returns></returns>
        private static string GetWmiPropertyValueForEngineService(
            string serviceName,
            string wmiNamespace,
            string propertyName
            )
        {
            var propertyValue = string.Empty;

            var query = string.Format(
                "select * from SqlServiceAdvancedProperty where SQLServiceType = 1 and PropertyName = '{0}' and ServiceName = '{1}'",
                propertyName,
                serviceName);

            using (var propertySearcher = new ManagementObjectSearcher(wmiNamespace, query))
            {
                foreach (var o in propertySearcher.Get())
                {
                    using (var sqlEdition = (ManagementObject)o)
                    {
                        propertyValue = sqlEdition["PropertyStrValue"].ToString();
                    }
                }
            }

            return
                propertyValue;
        }
    }
#else
    public class WmiSqlServerInstanceProvider : ISqlServerInstanceProvider
    {
        public List<SqlServerInstanceInformation> EnumerateSqlInstances()
        {
            return
                new List<SqlServerInstanceInformation>();
        }
    }
#endif
}