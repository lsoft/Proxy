using System;

namespace PerformanceTelemetry.Tests.DB.InstanceProvider
{
    public class SqlServerInstanceInformation
    {
        public enum SqlServerInstanceVersionEnum
        {
            Unknown,
            Standard,
            Developer,
            Enterprise
        }

        public string InstanceName
        {
            get;
            private set;
        }

        public string ServiceName
        {
            get;
            private set;
        }

        public string Version
        {
            get;
            private set;
        }

        public string Edition
        {
            get;
            private set;
        }

        public SqlServerInstanceVersionEnum SqlServerInstanceVersion
        {
            get
            {
                if (Edition.ToLower().Contains("enterprise"))
                {
                    return
                        SqlServerInstanceVersionEnum.Enterprise;
                }

                if (Edition.ToLower().Contains("standard"))
                {
                    return
                        SqlServerInstanceVersionEnum.Standard;
                }

                if (Edition.ToLower().Contains("developer"))
                {
                    return
                        SqlServerInstanceVersionEnum.Developer;
                }

                return
                    SqlServerInstanceVersionEnum.Unknown;
            }
        }

        public string FullInformation
        {
            get
            {
                var result = string.Format(
                    "{0};{1};{2};{3}",
                    InstanceName,
                    ServiceName,
                    Edition,
                    Version
                    );

                return result;
            }
        }

        public string DatabasePath
        {
            get
            {
                var result = "localhost";

                if (!string.IsNullOrEmpty(this.InstanceName))
                {
                    result += "\\" + this.InstanceName;
                }

                return result;
            }
        }

        public SqlServerInstanceInformation(
            string instanceName,
            string serviceName,
            string version,
            string edition
            )
        {
            if (instanceName == null)
            {
                throw new ArgumentNullException("instanceName");
            }
            if (serviceName == null)
            {
                throw new ArgumentNullException("serviceName");
            }
            if (version == null)
            {
                throw new ArgumentNullException("version");
            }
            if (edition == null)
            {
                throw new ArgumentNullException("edition");
            }

            InstanceName = instanceName;
            ServiceName = serviceName;
            Version = version;
            Edition = edition;
        }
    }
}
